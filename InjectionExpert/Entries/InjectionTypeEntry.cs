using System.Diagnostics;

namespace InjectionExpert.Entries;

[DebuggerDisplay("Type={Implementation}")]
public class InjectionTypeEntry(IInjectionProvider provider, InjectionLifespan lifespan, Type implementation)
    : InjectionEntry(lifespan)
{
    private object? _cache;
    
    public Type Implementation { get; } = implementation;

    public override object GetInjection(Type type, InjectionTarget target)
    {
        if (Lifespan == InjectionLifespan.Singleton)
            return _cache ??= provider.NewObject(Implementation);
        return provider.NewObject(Implementation.MakeGenericType(Implementation));
    }

    public override bool InvalidateCache()
    {
        if (_cache == null)
            return false;
        _cache = null;
        return true;
    }
}