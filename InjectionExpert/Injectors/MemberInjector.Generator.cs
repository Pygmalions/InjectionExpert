using System.Reflection;
using System.Runtime.CompilerServices;
using EmitToolbox;
using EmitToolbox.Builders;
using EmitToolbox.Extensions;
using EmitToolbox.Symbols;
using EmitToolbox.Symbols.Literals;
using EmitToolbox.Symbols.Operations;
using EmitToolbox.Utilities;
using InjectionExpert.Utilities.Internal;

namespace InjectionExpert.Injectors;

public partial class MemberInjector
{
    public static void Export(DynamicAssembly assemblyContext, Type type)
    {
        GenerateInjector(assemblyContext, type, out _);
    }

    private static MemberInjector CreateInjector(DynamicAssembly assemblyContext, Type type)
    {
        var functor = GenerateInjector(assemblyContext, type, out var injections)
            .GetMethod("TryInject")!
            .CreateDelegate<InjectorDelegate>();

        if (!type.IsGenericType)
            return new MemberInjector(type, functor, injections);

        /* Note:
         * The dynamic assembly is granted access to non-public members of the target type's assembly,
         * however, if the target type is generic, its type arguments may come from other assemblies.
         * Therefore, it is necessary to grant access to those assemblies as well.
         */
        foreach (var argumentType in type.GetGenericArguments())
            assemblyContext.IgnoreVisibilityChecksToAssembly(argumentType.Assembly);

        return new MemberInjector(type, functor, injections);
    }

    private static Type GenerateInjector(DynamicAssembly assemblyContext, Type type,
        out MultiDictionary<(Type Type, object? Key), MemberInfo> injections)
    {
        if (type.IsPrimitive || type == typeof(string))
            throw new InvalidOperationException($"Cannot inject primitive type or string \"{type.Name}\".");
        if (type.IsGenericTypeDefinition)
            throw new InvalidOperationException($"Cannot inject generic type definition \"{type.Name}\".");

        var typeContext = assemblyContext.DefineClass("MemberInjector_" + type);

        injections = new MultiDictionary<(Type Type, object? Key), MemberInfo>();

        var method = typeContext.MethodFactory.Static.DefineFunctor<bool>("TryInject",
        [
            ParameterDefinition.Value<object>("target"),
            ParameterDefinition.Value<IInjectionProvider>("provider"),
            ParameterDefinition.Value<InjectorOptions>("options")
        ]);

        var argumentBoxedTarget = method.Argument<object>(0);
        var argumentProvider = method.Argument<IInjectionProvider>(1);
        var argumentOptions = method.Argument<InjectorOptions>(2);

        var variableOnlyNullMembers = argumentOptions
            .GetPropertyValue(target => target.OnlyNullMembers)
            .ToSymbol();
        var variableShouldFailFast = argumentOptions
            .GetPropertyValue(target => target.FailFast)
            .ToSymbol();
        var variableAreRequiredMembersSelected = argumentOptions
            .GetPropertyValue(target => target.SelectedMembers)
            .HasFlag(SelectionMode.RequiredMembers)
            .ToSymbol();
        var variableAreAttributedMembersSelected = argumentOptions
            .GetPropertyValue(target => target.SelectedMembers)
            .HasFlag(SelectionMode.AttributedMembers)
            .ToSymbol();
        var variableMissingTargets = argumentOptions
            .GetPropertyValue(target => target.MissingTargets)
            .ToSymbol();
        var variableInjectedTargets = argumentOptions
            .GetPropertyValue(target => target.InjectedTargets)
            .ToSymbol();

        var variableSucceeded = method.Variable<bool>();
        variableSucceeded.AssignValue(method.Literal(true));

        VariableSymbol variableUnboxedTarget;
        if (type.IsValueType)
        {
            variableUnboxedTarget = method.Variable(type.MakeByRefType());
            argumentBoxedTarget
                .Unbox(type, true)
                .ToSymbol(variableUnboxedTarget);
        }
        else
        {
            variableUnboxedTarget = method.Variable(type);
            variableUnboxedTarget.AssignContent(argumentBoxedTarget.CastTo(type));
        }

        var variableCurrentInjection = method.Variable<object>();
        var variableCurrentRequester = method.Variable<InjectionTarget>();
        var variableCurrentKey = method.Variable<object>();

        var labelFailed = method.DefineLabel();

        foreach (var member in type
                     .GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                     .Where(CanMemberBeInjected))
        {
            var attribute = member.GetCustomAttribute<InjectionAttribute>();
            var required = member.IsDefined(typeof(RequiredMemberAttribute));

            if (attribute is null && !required)
                continue;
            if (attribute is { Ignored: true })
                continue;

            var labelContinue = method.DefineLabel();

            // Calculate the member selection condition.
            IOperationSymbol<bool> isMemberSelected = new NoOperation<bool>(method.Literal(false));
            if (attribute != null)
                isMemberSelected = isMemberSelected.Or(variableAreAttributedMembersSelected);
            if (required)
                isMemberSelected = isMemberSelected.Or(variableAreRequiredMembersSelected);

            // Skip the injection if the member is not selected.
            labelContinue.GotoIfFalse(isMemberSelected);

            (ISymbol<MemberInfo> Symbol, Type Type) requester = member switch
            {
                FieldInfo field => (method.Literal(field), field.FieldType),
                PropertyInfo property => (method.Literal(property), property.PropertyType),
                _ => throw new Exception($"Unsupported injecting member type '{member.MemberType}'.")
            };

            if (!type.IsValueType || type.IsGenericType &&
                type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                // Handle `onlyNullMembers` option.
                using (method.If(variableOnlyNullMembers))
                {
                    // Check if the member is null.
                    var symbolIsNull = member switch
                    {
                        FieldInfo field => variableUnboxedTarget.Field(field)
                            .HasNullValue(),
                        PropertyInfo property => variableUnboxedTarget.GetPropertyValue(property)
                            .HasNullValue(),
                        _ => throw new Exception($"Unsupported injecting member type '{member.MemberType}'.")
                    };

                    // Skip the injection if the member is not null.
                    labelContinue.GotoIfFalse(symbolIsNull);
                }
            }

            switch (member)
            {
                case FieldInfo field:
                    variableCurrentRequester.AssignNew(
                        () => new InjectionTarget(Any<FieldInfo>.Value, Any<object?>.Value),
                        [method.Literal(field), argumentBoxedTarget]);
                    break;
                case PropertyInfo property:
                    variableCurrentRequester.AssignNew(
                        () => new InjectionTarget(Any<PropertyInfo>.Value, Any<object?>.Value),
                        [method.Literal(property), argumentBoxedTarget]);
                    break;
                default:
                    throw new Exception($"Unsupported injecting member type '{member.MemberType}'.");
            }

            variableCurrentKey.AssignContent(attribute?.Key is { } key
                ? LiteralSymbolFactory.Create(method, key).ToObject()
                : method.Null<object>());

            // Get the injection item and assign it to the local variable.
            argumentProvider
                .Invoke(
                    target => target.GetInjection(
                        Any<Type>.Value, Any<object?>.Value, Any<InjectionTarget>.Value),
                    [
                        method.Literal(requester.Type),
                        variableCurrentKey,
                        variableCurrentRequester
                    ])
                .ToSymbol(variableCurrentInjection);

            using (method.If(variableCurrentInjection.IsNull()))
            {
                // Record the missing injection target.
                using (method.If(variableMissingTargets.IsNotNull()))
                {
                    variableMissingTargets.Add(variableCurrentRequester);
                }

                if (required)
                {
                    variableSucceeded.AssignValue(method.Literal(false));

                    // Throw the exception if the injector should fail fast.
                    labelFailed.GotoIfTrue(variableShouldFailFast);
                }

                // Continue to the next member.
                labelContinue.Goto();
            }

            switch (member)
            {
                case FieldInfo field:
                    variableUnboxedTarget.Field(field).AssignValue(
                        variableCurrentInjection.ConvertTo(field.FieldType));
                    break;
                case PropertyInfo property:
                    variableUnboxedTarget.SetPropertyValue(
                        property,
                        variableCurrentInjection.ConvertTo(property.PropertyType)
                    );
                    break;
                default:
                    throw new Exception($"Unsupported injecting member type '{member.MemberType}'.");
            }

            using (method.If(variableInjectedTargets.IsNotNull()))
            {
                variableInjectedTargets.Add(
                    method.New(
                        () => new ValueTuple<InjectionTarget, object>(
                            Any<InjectionTarget>.Value, Any<object>.Value),
                        [variableCurrentRequester, variableCurrentInjection])
                );
            }

            injections.Add((requester.Type, attribute?.Key), member);

            labelContinue.Mark();
        }

        using (labelFailed.MarkGotoOnlyScope())
        {
            // Throw the exception.
            method.ThrowException(() => new InjectionFailureException(
                    Any<Type>.Value, Any<object>.Value, Any<IInjectionProvider>.Value,
                    Any<InjectionTarget>.Value, Any<string>.Value),
                [
                    method.Literal(type), variableCurrentKey, argumentProvider,
                    variableCurrentRequester, method.Null<string>()
                ]);
        }

        method.Return(variableSucceeded);

        typeContext.Build();

        return typeContext.BuildingType;
    }

    private static bool CanMemberBeInjected(MemberInfo member)
    {
        switch (member)
        {
            case FieldInfo { IsLiteral: false, IsInitOnly: false }:
            case PropertyInfo { CanWrite: true }:
                break;
            default:
                return false;
        }

        return true;
    }
}