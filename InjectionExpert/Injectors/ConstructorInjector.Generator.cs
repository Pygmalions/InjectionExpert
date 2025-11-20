using System.Reflection;
using System.Reflection.Emit;
using EmitToolbox;
using EmitToolbox.Extensions;
using EmitToolbox.Symbols;
using EmitToolbox.Symbols.Literals;
using EmitToolbox.Utilities;

namespace InjectionExpert.Injectors;

public partial class ConstructorInjector
{
    public static void Export(DynamicAssembly assemblyContext, Type type)
    {
        GenerateInjector(assemblyContext, type);
    }

    private static ConstructorInjector CreateInjector(DynamicAssembly assemblyContext, Type type)
    {
        var functor = GenerateInjector(assemblyContext, type)
            .GetMethod("TryInject")!
            .CreateDelegate<Func<object, IInjectionProvider, bool>>();

        if (!type.IsGenericType)
            return new ConstructorInjector(type, functor);

        /* Note:
         * The dynamic assembly is granted access to non-public members of the target type's assembly,
         * however, if the target type is generic, its type arguments may come from other assemblies.
         * Therefore, it is necessary to grant access to those assemblies as well.
         */
        foreach (var argumentType in type.GetGenericArguments())
            assemblyContext.IgnoreVisibilityChecksToAssembly(argumentType.Assembly);

        return new ConstructorInjector(type, functor);
    }

    private static Type GenerateInjector(DynamicAssembly assemblyContext, Type type)
    {
        if (type.IsPrimitive || type == typeof(string))
            throw new InvalidOperationException($"Cannot inject primitive type or string '{type.Name}'.");
        if (type.IsAbstract || type.IsInterface)
            throw new InvalidOperationException($"Cannot inject abstract or interface type '{type.Name}'.");
        if (type.IsGenericTypeDefinition)
            throw new InvalidOperationException($"Cannot inject generic type definition '{type.Name}'.");

        var dynamicType = assemblyContext.DefineClass("ConstructorInjector_" + type);

        var dynamicMethod = dynamicType.MethodFactory.Static.DefineFunctor<bool>("TryInject",
        [
            ParameterDefinition.Value<object>("target"),
            ParameterDefinition.Value<IInjectionProvider>("provider")
        ]);

        var argumentBoxedTarget = dynamicMethod.Argument<object>(0);
        var argumentProvider = dynamicMethod.Argument<IInjectionProvider>(1);

        var constructors = type
            .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .OrderBy(constructor => constructor.GetParameters().Length)
            .ToList();

        var variablesInjectionItem = Enumerable
            .Range(0, constructors[^1].GetParameters().Length)
            .Select(_ => dynamicMethod.Variable<InjectionItem?>())
            .ToList();

        VariableSymbol variableUnboxedTarget;
        if (!type.IsValueType)
        {
            variableUnboxedTarget = dynamicMethod.Variable(type);
            variableUnboxedTarget.AssignContent(argumentBoxedTarget.ConvertTo(type));
        }
        else
        {
            variableUnboxedTarget = dynamicMethod.Variable(type.MakeByRefType());
            variableUnboxedTarget.AssignContent(argumentBoxedTarget.Unbox(type, true));
        }

        var context = new EmittingContext
        {
            TargetType = type,
            Method = dynamicMethod,
            ArgumentBoxedTarget = argumentBoxedTarget,
            ArgumentProvider = argumentProvider,
            VariableUnboxedTarget = variableUnboxedTarget,
            VariablesInjectionItem = variablesInjectionItem,
            VariableInjectionRequester = dynamicMethod.Variable<InjectionTarget>(),
            VariablesParameterInfo = dynamicMethod.Variable<ParameterInfo[]>(),
        };

        foreach (var constructor in constructors)
        {
            EmitTryConstructor(context, constructor);
        }

        dynamicMethod.Return(dynamicMethod.Value(false));

        dynamicType.Build();

        return dynamicType.BuildingType;
    }

    private readonly struct EmittingContext
    {
        public required Type TargetType { get; init; }

        public required DynamicMethod<Action<ISymbol<bool>>> Method { get; init; }

        public required List<VariableSymbol<InjectionItem?>> VariablesInjectionItem { get; init; }

        public required ArgumentSymbol<object> ArgumentBoxedTarget { get; init; }

        public required ArgumentSymbol<IInjectionProvider> ArgumentProvider { get; init; }

        public required VariableSymbol VariableUnboxedTarget { get; init; }

        public required VariableSymbol<InjectionTarget> VariableInjectionRequester { get; init; }

        public required VariableSymbol<ParameterInfo[]> VariablesParameterInfo { get; init; }
    }

    private static void EmitTryConstructor(
        EmittingContext context, ConstructorInfo constructor)
    {
        var method = context.Method;

        method.Value(constructor)
            .Invoke(target => target.GetParameters())
            .ToSymbol(context.VariablesParameterInfo);

        var labelEndOfThisAttempt = method.DefineLabel();

        foreach (var (index, parameter) in constructor.GetParameters().Index())
        {
            var attribute = parameter.GetCustomAttribute<InjectionAttribute>();

            ISymbol<object?> symbolKey = attribute?.Key is null
                ? method.Null<object>()
                : LiteralSymbolFactory.Create(method, attribute.Key).ToObject();

            // Load injection target.
            context.VariableInjectionRequester.AssignNew(
                () => new InjectionTarget(Any<ParameterInfo>.Value, Any<object>.Value),
                [
                    context.VariablesParameterInfo.ElementAt(index),
                    context.ArgumentBoxedTarget
                ]);

            var variableInjectionItem = context.VariablesInjectionItem[index];

            context.ArgumentProvider
                .Invoke(
                    target => target.GetInjectionItem(
                        Any<Type>.Value, Any<object?>.Value, Any<InjectionTarget>.Value),
                    [method.Value(parameter.ParameterType), symbolKey, context.VariableInjectionRequester])
                .ToSymbol(variableInjectionItem);

            using (method.If(variableInjectionItem.HasValue().Not()))
            {
                if (!parameter.HasDefaultValue)
                {
                    labelEndOfThisAttempt.Goto();
                }
                else
                {
                    var variableItem = method.New(
                        () => new InjectionItem(Any<object>.Value, Any<InjectionLifespan>.Value),
                        [
                            method.Null<object>(),
                            method.Value(InjectionLifespan.Transient)
                        ]);

                    var variableDefaultParameter = 
                        LiteralSymbolFactory.Create(method, parameter.DefaultValue)
                            .ToObject()
                            .ToSymbol();
                    
                    if (parameter.DefaultValue != null)
                        variableItem.SetPropertyValue(
                            target => target.Instance,
                            variableDefaultParameter);

                    variableItem.ToNullable(variableInjectionItem);
                }
            }
        }
        
        context.VariableUnboxedTarget.AssignNew(
            constructor,
            constructor.GetParameters().Index().Select(pair =>
                context.VariablesInjectionItem[pair.Index]
                    .GetValue()
                    .GetPropertyValue(target => target.Instance)
                    .ConvertTo(pair.Item.ParameterType)),
            inplace: true);

        method.Return(method.Value(true));

        labelEndOfThisAttempt.Mark();
    }
}