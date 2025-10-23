using System.Collections.Concurrent;
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
public class InjectionFunctorProvider(
    InjectionFunctorProvider.ProviderDelegate provider, bool cachingSingletons) 
    : IInjectionProvider
{
    /// <summary>
    /// Cache for keyed singleton injections.
    /// </summary>
    private ConcurrentKeyedDictionary<Type, object, object>? _cachedKeyedSingletons;

    /// <summary>
    /// Cache for unkeyed singleton injections.
    /// </summary>
    private ConcurrentDictionary<Type, object>? _cachedUnkeyedSingletons;
    
    public delegate InjectionItem? ProviderDelegate(Type type, object? key, InjectionTarget target);

    public InjectionItem? GetInjectionItem(Type type, object? key, InjectionTarget target)
    {
        if (!cachingSingletons)
            return provider(type, key, target);

        if (key is null)
        {
            if (_cachedUnkeyedSingletons?.TryGetValue(type, out var value) == true)
                return new InjectionItem(value, InjectionLifespan.Singleton);
        }
        else
        {
            if (_cachedKeyedSingletons?.TryGetValue(type, key, out var value) == true)
                return new InjectionItem(value, InjectionLifespan.Singleton);
        }
        var entry = provider(type, key, target);
        if (entry == null) 
            return null;
        if (entry.Value.Lifespan != InjectionLifespan.Singleton) 
            return entry;
        // Cache the singleton instance.
        if (key is null)
        {
            _cachedUnkeyedSingletons ??= new ConcurrentDictionary<Type, object>();
            _cachedUnkeyedSingletons[type] = entry.Value.Instance;
        }
        else
        {
            _cachedKeyedSingletons ??= new ConcurrentKeyedDictionary<Type, object, object>();
            _cachedKeyedSingletons.SetValue(type, key, entry.Value.Instance);
        }
        return entry;
    }

    public IInjectionProvider.IScope NewScope(InjectionTarget target) =>
        InjectionScope.New(this, null, target);
}

public static class InjectionFunctorProviderExtensions
{
    extension(IInjectionProvider)
    {
        /// <summary>
        /// Create a new injection provider from a functor.
        /// </summary>
        /// <param name="provider">Factory functor.</param>
        /// <param name="cachingSingletons">
        /// If true, the provider will cache singleton injections,
        /// and the cached instances will be return for subsequent requests;
        /// otherwise, the factory will be called every time the injection is requested.
        /// </param>
        /// <returns>Injection provider created that wraps the specified functor.</returns>
        public static InjectionFunctorProvider FromFunctor(
            InjectionFunctorProvider.ProviderDelegate provider, bool cachingSingletons = false)
            => new(provider, cachingSingletons);
    }
}