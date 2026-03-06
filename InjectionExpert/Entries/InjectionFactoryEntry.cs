using System.Diagnostics;

namespace InjectionExpert.Entries;

public abstract class InjectionFactoryEntry : InjectionEntry
{
    public delegate TInjection FactoryDelegate<out TInjection>(
        IInjectionProvider provider, Type type, object? key, InjectionTarget target);
}

[DebuggerDisplay("Factory={Factory}")]
public class InjectionFactoryEntry<TInjection>(
    InjectionLifespan lifespan,
    InjectionFactoryEntry.FactoryDelegate<TInjection> factory)
    : InjectionFactoryEntry
{
    public override InjectionLifespan Lifespan => lifespan;

    public FactoryDelegate<TInjection> Factory { get; } = factory;

    public override bool IsAssignableTo(Type type)
        => typeof(TInjection).IsAssignableTo(type);

    public override object GetInjection(IInjectionProvider provider, Type type, object? key, InjectionTarget target)
        => Factory(provider, type, key, target)!;

    public override string ToString() => $"(Factory: {Factory})";
}