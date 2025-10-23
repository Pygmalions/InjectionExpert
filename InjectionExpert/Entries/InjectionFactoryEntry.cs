using System.Diagnostics;

namespace InjectionExpert.Entries;

[DebuggerDisplay("Factory={Factory}")]
public class InjectionFactoryEntry(
    IInjectionProvider provider,
    InjectionLifespan lifespan,
    IInjectionContainer.FactoryDelegate factory)
    : InjectionEntry(lifespan)
{
    private object? _cache;

    public IInjectionContainer.FactoryDelegate Factory { get; } = factory;

    public override object GetInjection(Type type, InjectionTarget target)
    {
        if (Lifespan == InjectionLifespan.Singleton)
            return _cache ??= Factory(provider, type, target);
        return Factory(provider, type, target);
    }

    public override bool InvalidateCache()
    {
        if (_cache == null)
            return false;
        _cache = null;
        return true;
    }
}