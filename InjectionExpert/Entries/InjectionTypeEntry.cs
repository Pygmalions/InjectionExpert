using System.Diagnostics;

namespace InjectionExpert.Entries;

[DebuggerDisplay("Type={Implementation}")]
public class InjectionTypeEntry : InjectionEntry
{
    private object? _cache;
    
    public Type Implementation { get; }

    public InjectionTypeEntry(InjectionLifespan lifespan, Type implementation) : base(lifespan)
    {
        if (implementation.IsGenericTypeDefinition)
            throw new ArgumentException(
                $"Implementation type {implementation.Name} is an generic type definition.", 
                nameof(implementation));
        Implementation = implementation;
    }

    public override object GetInjection(IInjectionProvider provider, Type type, object? key, InjectionTarget target)
        => GetInjection(provider);

    public object GetInjection(IInjectionProvider provider)
    {
        if (Lifespan == InjectionLifespan.Singleton)
            return _cache ??= provider.NewObject(Implementation);
        return provider.NewObject(Implementation);
    }

    public override bool InvalidateCache()
    {
        if (_cache == null)
            return false;
        _cache = null;
        return true;
    }

    public override string ToString() => $"({Lifespan}, Type: {Implementation})";
}