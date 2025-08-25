using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using EmitToolbox.Extensions;
using EmitToolbox.Framework;
using InjectionExpert.Utilities.Internal;

namespace InjectionExpert.Injectors;

using InjectionItem = (object, InjectionLifespan);

public partial class MemberInjector
{
    public static void Export(AssemblyBuildingContext assemblyContext, Type type)
    {
        GenerateInjector(assemblyContext, type, out _);
    }

    private static MemberInjector CreateInjector(AssemblyBuildingContext assemblyContext, Type type)
    {
        var functor = GenerateInjector(assemblyContext, type, out var injections)
            .GetMethod("TryInject")!
            .CreateDelegate<Func<object, IInjectionProvider, bool, InjectionTarget?>>();

        return new MemberInjector(type, functor, injections);
    }

    private static Type GenerateInjector(AssemblyBuildingContext assemblyContext, Type type,
        out MultiDictionary<(Type Type, object? Key), MemberInfo> injections)
    {
        if (type.IsPrimitive || type == typeof(string))
            throw new InvalidOperationException($"Cannot inject primitive type or string \"{type.Name}\".");
        if (type.IsGenericTypeDefinition)
            throw new InvalidOperationException($"Cannot inject generic type definition \"{type.Name}\".");

        var typeContext = assemblyContext.DefineClass("MemberInjector_" + type);

        injections = new MultiDictionary<(Type Type, object? Key), MemberInfo>();

        var methodContext = typeContext.Functors.Static("TryInject",
            [
                ParameterDefinition.Value<object>("target"),
                ParameterDefinition.Value<IInjectionProvider>("provider"),
                ParameterDefinition.Value<bool>("onlyNullMembers")
            ],
            ResultDefinition.Value<InjectionTarget?>());

        var code = methodContext.Code;

        // Local variable to store the injection.
        var context = new EmittingContext(type, code);

        code.LoadArgument0();
        if (type.IsValueType)
            code.Emit(OpCodes.Unbox_Any, type);
        code.StoreLocal(context.VariableTarget);

        var labelFailed = code.DefineLabel();

        var options = type.GetCustomAttribute<InjectionOptionsAttribute>();

        foreach (var member in type
                     .GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                     .Where(candidate => IsInjectionTarget(candidate, options)))
        {
            var attribute = member.GetCustomAttribute<InjectionAttribute>();
            var required = member.IsDefined(typeof(RequiredMemberAttribute));

            var labelContinue = code.DefineLabel();

            Type injectionType;

            var labelInjection = code.DefineLabel();

            // Argument `onlyNullMembers`
            code.LoadArgument2();
            code.GotoIfFalse(labelInjection);

            // Check if the member is null.
            switch (member)
            {
                case FieldInfo field:
                    EmitExamineIfFieldIsNull(context, field);
                    break;
                case PropertyInfo property:
                    EmitExamineIfPropertyIsNull(context, property);
                    break;
                default:
                    throw new Exception($"Unsupported injecting member type {member.MemberType}.");
            }

            // Skip the injection if the member is not null.
            code.GotoIfFalse(labelContinue);

            code.MarkLabel(labelInjection);

            switch (member)
            {
                case FieldInfo field:
                    injectionType = field.FieldType;
                    EmitGettingInjection(context, injectionType, attribute?.Key, field);
                    code.If(branch =>
                        {
                            branch.LoadLocalAddress(context.VariableNullableInjection);
                            branch.NullableHasValue<InjectionItem>();
                        },
                        whenFalse: required
                            ? branch => branch.Goto(labelFailed)
                            : branch => branch.Goto(labelContinue));
                    EmitInjectField(context, field);
                    break;
                case PropertyInfo property:
                    injectionType = property.PropertyType;
                    EmitGettingInjection(context, injectionType, attribute?.Key, property);
                    code.If(branch =>
                        {
                            branch.LoadLocalAddress(context.VariableNullableInjection);
                            branch.NullableHasValue<InjectionItem>();
                        },
                        whenFalse: required
                            ? branch => branch.Goto(labelFailed)
                            : branch => branch.Goto(labelContinue));
                    EmitInjectProperty(context, property);
                    break;
                default:
                    throw new Exception($"Unsupported injecting member type {member.MemberType}.");
            }

            injections.Add((injectionType, attribute?.Key), member);

            code.MarkLabel(labelContinue);
        }

        var labelReturn = code.DefineLabel();
        
        // If `this` is a boxed value type, then the modified result in the local variable should be copied back.
        if (type.IsValueType)
        {
            code.LoadArgument0();
            code.Call(typeof(Unsafe).GetMethod("Unbox")!.MakeGenericMethod(type));
            code.LoadLocal(context.VariableTarget);
            code.Emit(OpCodes.Stobj, type);
        }

        // Construct a null missing requester.
        code.LoadLocalAddress(context.VariableMissingRequester);
        code.Emit(OpCodes.Initobj, typeof(InjectionTarget?));
        code.LoadLocal(context.VariableMissingRequester);

        code.Goto(labelReturn);

        code.MarkLabel(labelFailed);

        // Construct a missing requester with the last requester.
        code.LoadLocal(context.VariableLatestTarget);
        code.NewObject(typeof(InjectionTarget?).GetConstructor([typeof(InjectionTarget)])!);

        code.MarkLabel(labelReturn);
        code.MethodReturn();

        typeContext.Build();
        
        return typeContext.BuildingType;
    }

    private static bool IsInjectionTarget(MemberInfo member, InjectionOptionsAttribute? options)
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

    private static void EmitGettingInjection(in EmittingContext context, Type type, object? key,
        MemberInfo requester)
    {
        var code = context.Code;

        // Load injection source.
        code.Emit(OpCodes.Ldarg_1);
        code.EmitTypeInfo(type);
        code.LoadBoxedLiteral(key);

        // Load injection target.
        code.LoadArgument0();
        code.LoadNull();
        code.LoadNull();
        switch (requester)
        {
            case FieldInfo field:
                code.EmitFieldInfo(field);
                break;
            case PropertyInfo property:
                code.EmitPropertyInfo(property);
                break;
            default:
                throw new Exception("Unsupported requester type.");
        }

        code.NewObject(typeof(InjectionTarget).GetConstructor(
            [typeof(object), typeof(Type), typeof(ParameterInfo), typeof(MemberInfo)])!);
        code.StoreLocal(context.VariableLatestTarget);

        code.LoadLocal(context.VariableLatestTarget);

        // Query the container for specific injection.
        code.Emit(OpCodes.Callvirt,
            typeof(IInjectionProvider).GetMethod(nameof(IInjectionProvider.GetInjection))!);

        // Store the injection to the variable.
        code.StoreLocal(context.VariableNullableInjection);
    }

    private static void EmitExamineIfFieldIsNull(in EmittingContext context, FieldInfo field)
    {
        var code = context.Code;

        if (!field.FieldType.IsValueType)
        {
            context.EmitLoadTarget();
            code.LoadField(field);
            code.LoadNull();
            code.Emit(OpCodes.Ceq);
        }
        else if (field.FieldType.IsGenericType && 
                 field.FieldType.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            context.EmitLoadTarget();
            code.LoadFieldAddress(field);
            
            code.NullableHasValue(field.FieldType.GetGenericArguments()[0]);
            code.LoadLiteral(false);
            code.Emit(OpCodes.Ceq);
        }
        else
        {
            code.LoadLiteral(true);
        }
    }

    private static void EmitExamineIfPropertyIsNull(in EmittingContext context, PropertyInfo property)
    {
        var code = context.Code;

        if (property.GetMethod == null)
        {
            code.LoadLiteral(true);
            return;
        }

        if (!property.PropertyType.IsValueType)
        {
            context.EmitLoadTarget();
            code.LoadProperty(property);
            code.LoadNull();
            code.Emit(OpCodes.Ceq);
        }
        else if (property.PropertyType.IsGenericType &&
                 property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            context.EmitLoadTarget();
            code.LoadProperty(property);
            code.ToAddress(property.PropertyType);
            code.NullableHasValue(property.PropertyType.GetGenericArguments()[0]);

            code.LoadLiteral(false);
            code.Emit(OpCodes.Ceq);
        }
        else
        {
            code.LoadLiteral(true);
        }
    }
    
    private static void EmitInjectField(in EmittingContext context, FieldInfo field)
    {
        var code = context.Code;
        
        context.EmitLoadTarget();
        
        code.LoadLocalAddress(context.VariableNullableInjection);
        code.NullableGetValue<InjectionItem>();
        code.ToAddress<InjectionItem>();
        code.LoadField(typeof(InjectionItem).GetField("Item1")!);
        
        if (field.FieldType.IsValueType)
            code.Emit(OpCodes.Unbox_Any, field.FieldType);
        code.Emit(OpCodes.Stfld, field);
    }

    private static void EmitInjectProperty(in EmittingContext context, PropertyInfo property)
    {
        var code = context.Code;
        
        context.EmitLoadTarget();
        
        code.LoadLocalAddress(context.VariableNullableInjection);
        code.NullableGetValue<InjectionItem>();
        code.ToAddress<InjectionItem>();
        code.LoadField(typeof(InjectionItem).GetField("Item1")!);
        
        if (property.PropertyType.IsValueType)
            code.Emit(OpCodes.Unbox_Any, property.PropertyType);
        code.Emit(property.SetMethod!.IsVirtual ? OpCodes.Callvirt : OpCodes.Call,
            property.SetMethod!);
    }

    private readonly struct EmittingContext(Type type, ILGenerator code)
    {
        public ILGenerator Code { get; } = code;
        public LocalBuilder VariableTarget { get; } = code.DeclareLocal(type);
        public LocalBuilder VariableNullableInjection { get; }
            = code.DeclareLocal(typeof(InjectionItem?));
        public LocalBuilder VariableLatestTarget { get; }
            = code.DeclareLocal(typeof(InjectionTarget));
        public LocalBuilder VariableMissingRequester { get; }
            = code.DeclareLocal(typeof(InjectionTarget?));

        public void EmitLoadTarget()
        {
            if (type.IsValueType)
                Code.LoadLocalAddress(VariableTarget);
            else
                Code.LoadLocal(VariableTarget);
        }
    }
}