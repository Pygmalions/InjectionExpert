using InjectionExpert.Utilities.Internal;

namespace InjectionExpert.Utilities;

/// <summary>
/// This class wraps a provider delegate into an injection provider.
/// </summary>
/// <param name="provider">Functor to provide injections.</param>
/// <param name="cachingSingletons">
/// If true, this provider will cache singleton instances,
/// and return the cached instance for subsequent requests.
/// If false, the factory will be called for every request.
/// </param>
public class FunctorInjectionProvider(
    FunctorInjectionProvider.ProviderDelegate provider, bool cachingSingletons = true) 
    : IInjectionProvider
{
    /// <summary>
    /// Caches for singleton injections.
    /// </summary>
    private ConcurrentKeyedDictionary<Type, object, object>? _singletons;
    
    public delegate InjectionItem? ProviderDelegate(Type type, object? key, InjectionTarget target);

    public InjectionItem? GetInjection(Type type, object? key, InjectionTarget target)
    {
        if (!cachingSingletons)
            return provider(type, key, target);
        
        if (_singletons != null 
            && _singletons.TryGetValue(type, key ?? InjectionContainer.NullKey.Value, out var value))
            return (value, InjectionLifespan.Singleton);
        var entry = provider(type, key, target);
        if (entry == null) 
            return null;
        if (entry.Value.Lifespan != InjectionLifespan.Singleton) 
            return entry;
        // Cache the singleton instance.
        _singletons ??= new ConcurrentKeyedDictionary<Type, object, object>();
        _singletons.SetValue(type, key ?? InjectionContainer.NullKey.Value, entry.Value.Instance);
        return entry;
    }

    public IInjectionProvider.IScope NewScope(InjectionTarget target) =>
        InjectionScope.New(this, null, target);
}