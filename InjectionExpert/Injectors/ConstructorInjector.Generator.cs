using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using EmitToolbox.Extensions;
using EmitToolbox.Framework;

namespace InjectionExpert.Injectors;

using InjectionItem = (object, InjectionLifespan);

public partial class ConstructorInjector
{
    public static void Export(AssemblyBuildingContext assemblyContext, Type type)
    {
        GenerateInjector(assemblyContext, type);
    }

    private static ConstructorInjector CreateInjector(AssemblyBuildingContext assemblyContext, Type type)
    {
        var functor = GenerateInjector(assemblyContext, type)
            .GetMethod("TryInject")!
            .CreateDelegate<Func<object, IInjectionProvider, InjectionTarget?>>();
        return new ConstructorInjector(type, functor);
    }

    private static Type GenerateInjector(AssemblyBuildingContext assemblyContext, Type type)
    {
        if (type.IsPrimitive || type == typeof(string))
            throw new InvalidOperationException($"Cannot inject primitive type or string \"{type.Name}\".");
        if (type.IsAbstract || type.IsInterface)
            throw new InvalidOperationException($"Cannot inject abstract or interface type \"{type.Name}\".");
        if (type.IsGenericTypeDefinition)
            throw new InvalidOperationException($"Cannot inject generic type definition \"{type.Name}\".");

        var typeContext = assemblyContext.DefineClass("ConstructorInjector_" + type);

        var methodContext = typeContext.Functors.Static("TryInject",
            [
                ParameterDefinition.Value<object>("target"),
                ParameterDefinition.Value<IInjectionProvider>("provider")
            ],
            ResultDefinition.Value<InjectionTarget?>());

        var code = methodContext.Code;

        var constructors = type
            .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .OrderBy(constructor => constructor.GetParameters().Length).ToList();

        var maxVariableCount = constructors[^1].GetParameters().Length;

        var injectionVariables = new List<LocalBuilder>(maxVariableCount);
        for (var index = 0; index < maxVariableCount; ++index)
        {
            injectionVariables.Add(code.DeclareLocal(typeof(InjectionItem?)));
        }

        var context = new EmittingContext(type, code)
        {
            NullableInjectionVariables = injectionVariables,
        };

        foreach (var constructor in constructors)
        {
            EmitTryConstructor(ref context, constructor);
        }

        if (type.IsValueType)
        {
            code.LoadArgument0();
            code.Call(typeof(Unsafe).GetMethod("Unbox")!.MakeGenericMethod(type));
            code.LoadLocal(context.VariableTarget);
            code.Emit(OpCodes.Stobj, type);
        }

        code.LoadLocal(context.VariableLatestTarget);
        code.NewObject(typeof(InjectionTarget?).GetConstructor([typeof(InjectionTarget)])!);
        code.MethodReturn();

        typeContext.Build();
        
        return typeContext.BuildingType;
    }

    private static void EmitTryConstructor(ref EmittingContext context, ConstructorInfo constructor)
    {
        var code = context.Code;

        code.EmitConstructorInfo(constructor);
        code.Emit(OpCodes.Callvirt,
            typeof(MethodBase).GetMethod(nameof(MethodBase.GetParameters))!);
        code.StoreLocal(context.VariableParameters);

        var labelFailed = code.DefineLabel();

        var parameters = constructor.GetParameters();
        for (var index = 0; index < parameters.Length; ++index)
        {
            var parameter = parameters[index];
            var attribute = parameter.GetCustomAttribute<InjectionAttribute>();
            var injectionType = parameter.ParameterType;
            var variableNullableInjection = context.NullableInjectionVariables[index];

            EmitGettingInjection(ref context, injectionType, index, attribute?.Key);
            // Load the injection to check if it is null.
            code.LoadLocalAddress(variableNullableInjection);
            code.NullableHasValue<InjectionItem>();

            if (!parameter.HasDefaultValue)
                code.GotoIfFalse(labelFailed);
            else
            {
                var labelInjection = code.DefineLabel();
                code.GotoIfTrue(labelInjection);
                code.LoadParameterDefaultValue(parameter);
                if (injectionType.IsValueType)
                    code.Emit(OpCodes.Box, injectionType);
                code.LoadLiteral(InjectionLifespan.Transient);
                code.NewObject(typeof(InjectionItem).GetConstructor(
                    [typeof(object), typeof(InjectionLifespan)])!);
                code.ToNullable<InjectionItem>();
                code.StoreLocal(variableNullableInjection);
                code.MarkLabel(labelInjection);
            }
        }

        // Load target object.
        context.EmitLoadTarget();

        var variableInjection = code.DeclareLocal(typeof(InjectionItem));

        // Load arguments.
        for (var index = 0; index < parameters.Length; ++index)
        {
            code.LoadLocalAddress(context.NullableInjectionVariables[index]);
            code.NullableGetValue<InjectionItem>();
            code.StoreLocal(variableInjection);
            code.LoadLocalAddress(variableInjection);
            code.LoadField(typeof(InjectionItem).GetField(nameof(InjectionItem.Item1))!);
            if (parameters[index].ParameterType.IsValueType)
                code.Emit(OpCodes.Unbox_Any, parameters[index].ParameterType);
        }

        // Invoke the constructor.
        code.Call(constructor);

        if (context.Type.IsValueType)
        {
            code.LoadArgument0();
            code.Call(typeof(Unsafe).GetMethod("Unbox")!.MakeGenericMethod(context.Type));
            code.LoadLocal(context.VariableTarget);
            code.Emit(OpCodes.Stobj, context.Type);
        }

        code.LoadLocalAddress(context.VariableMissingRequester);
        code.Emit(OpCodes.Initobj, typeof(InjectionTarget?));
        code.LoadLocal(context.VariableMissingRequester);
        code.MethodReturn();

        code.MarkLabel(labelFailed);
    }

    // Generate code for getting injection from the source.
    private static void EmitGettingInjection(ref EmittingContext context,
        Type category, int parameterIndex, object? key)
    {
        var code = context.Code;

        // Load injection source.
        code.Emit(OpCodes.Ldarg_1);

        // Load injection type.
        code.EmitTypeInfo(category);
        // Load injection key.
        code.LoadBoxedLiteral(key);

        // Load injection target.
        code.LoadArgument0();
        code.LoadNull();
        code.LoadLocal(context.VariableParameters);
        code.LoadLiteral(parameterIndex);
        code.Emit(OpCodes.Ldelem_Ref);
        code.LoadNull();
        code.NewObject(typeof(InjectionTarget).GetConstructor(
            [typeof(object), typeof(Type), typeof(ParameterInfo), typeof(MemberInfo)])!);
        code.StoreLocal(context.VariableLatestTarget);

        code.LoadLocal(context.VariableLatestTarget);

        // Query the container for specific injection.
        code.Emit(OpCodes.Callvirt,
            typeof(IInjectionProvider).GetMethod(nameof(IInjectionProvider.GetInjection))!);

        // Store the injection to the variable.
        code.Emit(OpCodes.Stloc, context.NullableInjectionVariables[parameterIndex]);
    }

    private readonly struct EmittingContext
    {
        public ILGenerator Code { get; }

        public Type Type { get; }

        public LocalBuilder VariableTarget { get; }

        public required List<LocalBuilder> NullableInjectionVariables { get; init; }

        public LocalBuilder VariableLatestTarget { get; }

        public LocalBuilder VariableParameters { get; }

        public LocalBuilder VariableMissingRequester { get; }

        public EmittingContext(Type type, ILGenerator code)
        {
            Code = code;
            Type = type;

            VariableTarget = code.DeclareLocal(type);
            VariableLatestTarget = code.DeclareLocal(typeof(InjectionTarget));
            VariableParameters = code.DeclareLocal(typeof(ParameterInfo[]));
            VariableMissingRequester = code.DeclareLocal(typeof(InjectionTarget?));
            NullableInjectionVariables = (List<LocalBuilder>)[];

            code.LoadArgument0();
            if (type.IsValueType)
                code.Emit(OpCodes.Unbox_Any, type);
            code.StoreLocal(VariableTarget);
        }

        public void EmitLoadTarget()
        {
            if (Type.IsValueType)
                Code.LoadLocalAddress(VariableTarget);
            else
                Code.LoadLocal(VariableTarget);
        }
    }
}