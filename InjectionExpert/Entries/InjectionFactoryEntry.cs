using System.Diagnostics;

namespace InjectionExpert.Entries;

public abstract class InjectionFactoryEntry(InjectionLifespan lifespan) : InjectionEntry(lifespan)
{
    public delegate TInjection FactoryDelegate<out TInjection>(
        IInjectionProvider provider, Type type, object? key, InjectionTarget target);
    
    public abstract Func<IInjectionProvider, Type, object?, InjectionTarget, object> UntypedFactory { get; }
}

[DebuggerDisplay("Factory={Factory}")]
public class InjectionFactoryEntry<TInjection>(
    IInjectionProvider provider,
    InjectionLifespan lifespan,
    InjectionFactoryEntry.FactoryDelegate<TInjection> factory)
    : InjectionFactoryEntry(lifespan)
{
    private object? _cache;

    public FactoryDelegate<TInjection> Factory { get; } = factory;

    public override Func<IInjectionProvider, Type, object?, InjectionTarget, object> UntypedFactory
        => (argumentProvider, argumentType, argumentKey, argumentTarget) => 
            Factory(argumentProvider, argumentType, argumentKey, argumentTarget)!;

    public override object GetInjection(Type type, object? key, InjectionTarget target)
    {
        if (Lifespan == InjectionLifespan.Singleton)
            return _cache ??= Factory(provider, type, key, target)!;
        return Factory(provider, type, key, target)!;
    }

    public override bool InvalidateCache()
    {
        if (_cache == null)
            return false;
        _cache = null;
        return true;
    }

    public override string ToString() => $"({Lifespan}, Factory: {Factory})";
}