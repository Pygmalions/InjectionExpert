using System.Diagnostics;

namespace InjectionExpert.Entries;

[DebuggerDisplay("Type={Implementation}")]
public class InjectionTypeEntry : InjectionEntry
{
    public InjectionTypeEntry(InjectionLifespan lifespan, Type implementation)
    {
        Lifespan = lifespan;
        Implementation = implementation;
        if (implementation.IsGenericTypeDefinition)
            throw new ArgumentException(
                $"Implementation type '{implementation.Name}' is an generic type definition.",
                nameof(implementation));
    }

    public override InjectionLifespan Lifespan { get; }

    public Type Implementation { get; }

    public override bool IsAssignableTo(Type type)
        => Implementation.IsAssignableTo(type);

    public override object GetInjection(IInjectionProvider provider, Type type, object? key, InjectionTarget target)
        => provider.NewObject(Implementation);


    public override string ToString() => $"(Type: {Implementation})";
}