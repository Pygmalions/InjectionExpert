using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using EmitToolbox.Framework;
using EmitToolbox.Framework.Extensions;
using EmitToolbox.Framework.Symbols;
using EmitToolbox.Framework.Symbols.Literals;
using EmitToolbox.Framework.Utilities;
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
            .CreateDelegate<Func<object, IInjectionProvider, bool, InjectionTarget?>>();

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

        var method = typeContext.MethodFactory.Static.DefineFunctor<InjectionTarget?>("TryInject",
        [
            ParameterDefinition.Value<object>("target"),
            ParameterDefinition.Value<IInjectionProvider>("provider"),
            ParameterDefinition.Value<bool>("onlyNullMembers")
        ]);

        var argumentBoxedTarget = method.Argument<object>(0);
        var argumentProvider = method.Argument<IInjectionProvider>(1);
        var argumentOnlyNullMembers = method.Argument<bool>(2);

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

        var variableInjectionItem = method.Variable<InjectionItem?>();
        var variableInjectionRequester = method.Variable<InjectionTarget>();

        var labelFailed = method.DefineLabel();

        var options = type.GetCustomAttribute<InjectionOptionsAttribute>();

        foreach (var member in type
                     .GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                     .Where(candidate => ShouldMemberBeInjected(candidate, options)))
        {
            var attribute = member.GetCustomAttribute<InjectionAttribute>();
            var required = member.IsDefined(typeof(RequiredMemberAttribute));

            var labelContinue = method.DefineLabel();
            var labelInjection = method.DefineLabel();

            (ISymbol<MemberInfo> Symbol, Type Type) requester = member switch
            {
                FieldInfo field => (method.Value(field), field.FieldType),
                PropertyInfo property => (method.Value(property), property.PropertyType),
                _ => throw new Exception($"Unsupported injecting member type {member.MemberType}.")
            };

            if (!type.IsValueType || type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                // Handle `onlyNullMembers` option.
                using (method.If(argumentOnlyNullMembers))
                {
                    // Check if the member is null.
                    var symbolIsNull = member switch
                    {
                        FieldInfo field => variableUnboxedTarget.Field(field)
                            .HasNullValue(),
                        PropertyInfo property => variableUnboxedTarget.GetPropertyValue(property)
                            .HasNullValue(),
                        _ => throw new Exception($"Unsupported injecting member type {member.MemberType}.")
                    };

                    // Skip the injection if the member is not null.
                    labelContinue.GotoIfFalse(symbolIsNull);
                }
            }

            labelInjection.Mark();

            ISymbol<MemberInfo> symbolRequester = member switch
            {
                FieldInfo fieldRequester => method.Value(fieldRequester),
                PropertyInfo propertyRequester => method.Value(propertyRequester),
                _ => throw new Exception($"Unsupported requester type '{member.GetType()}'.")
            };

            variableInjectionRequester.AssignNew(
                typeof(InjectionTarget).GetConstructor(
                    [typeof(MemberInfo), typeof(object)])!,
                [symbolRequester, argumentBoxedTarget]);

            ISymbol<object> symbolKey = attribute?.Key is { } key
                ? LiteralSymbolFactory.Create(method, key).ToObject()
                : method.Null<object>();

            argumentProvider
                .Invoke(
                    target => target.GetInjectionItem(
                        Any<Type>.Value, Any<object?>.Value, Any<InjectionTarget>.Value),
                    [method.Value(requester.Type), symbolKey, variableInjectionRequester])
                .ToSymbol(variableInjectionItem);

            (required ? labelFailed : labelContinue)
                .GotoIfFalse(variableInjectionItem.HasValue());

            switch (member)
            {
                case FieldInfo field:
                    variableUnboxedTarget.Field(field).AssignValue(
                        variableInjectionItem
                            .GetValue()
                            .GetPropertyValue(target => target.Instance)
                            .ConvertTo(field.FieldType));
                    break;
                case PropertyInfo property:
                    variableUnboxedTarget.SetPropertyValue(
                        property,
                        variableInjectionItem
                            .GetValue()
                            .GetPropertyValue(target => target.Instance)
                            .ConvertTo(property.PropertyType)
                    );
                    break;
                default:
                    throw new Exception($"Unsupported injecting member type {member.MemberType}.");
            }

            injections.Add((requester.Type, attribute?.Key), member);

            labelContinue.Mark();
        }

        var labelReturn = method.DefineLabel();

        labelReturn.Goto();

        labelFailed.Mark();
        method.Return(variableInjectionRequester.ToNullable());

        labelReturn.Mark();
        method.Return(method.Variable<InjectionTarget?>());

        typeContext.Build();

        return typeContext.BuildingType;
    }

    private static bool ShouldMemberBeInjected(MemberInfo member, InjectionOptionsAttribute? options)
    {
        switch (member)
        {
            case FieldInfo { IsLiteral: false, IsInitOnly: false }:
            case PropertyInfo { CanWrite: true }:
                break;
            default:
                return false;
        }

        var attribute = member.GetCustomAttribute<InjectionAttribute>();
        if (attribute?.Ignored == true)
            return false;

        if (options?.WithRequiredMembers != false &&
            member.IsDefined(typeof(RequiredMemberAttribute)))
            return true;
        return options?.WithAttributedMembers != false && attribute != null;
    }
}