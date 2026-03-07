using System.Diagnostics;
using InjectionExpert.Utilities;
using InjectionExpert.Utilities.Internal;

namespace InjectionExpert.Entries;

[DebuggerDisplay("Generic={Implementation}")]
public class InjectionGenericEntry(InjectionLifespan lifespan, Type implementation) : InjectionEntry
{
    public override InjectionLifespan Lifespan => lifespan;

    /// <summary>
    /// Implementation type definition.
    /// </summary>
    public Type Implementation { get; } = implementation;

    public override bool IsAssignableTo(Type type)
    {
        if (type.IsInterface)
            return Implementation.TryMatchInterface(type, out _);
        if (!type.IsGenericType)
            return false;
        if (!type.IsGenericTypeDefinition)
            type = type.GetGenericTypeDefinition();
        return Implementation.TryMatchGenericBaseType(type, out _);
    }

    public override object GetInjection(IInjectionProvider provider, Type type, object? key, InjectionTarget target)
    {
        if (type.GetGenericTypeDefinition() == Implementation)
            return provider.NewObject(type); 
        if (!Implementation.TryMatchGenericBaseType(type.GetGenericTypeDefinition(), 
                out var matchedDefinition))
            throw new ArgumentException(
                $"Request type '{type}' cannot match the generic implementation type '{Implementation}'.");
        var parameters = GenericParameterExtractor.ExtractArguments(type, matchedDefinition);
        return provider.NewObject(Implementation.MakeGenericType(parameters));
    }

    public override string ToString() => $"(Generic Definition: {Implementation})";
}