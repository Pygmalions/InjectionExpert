using InjectionExpert.Utilities;
using JetBrains.Annotations;

namespace InjectionExpert;

public interface IInjectionProvider
{
    /// <summary>
    /// Get an injection for the specified category type.
    /// </summary>
    /// <param name="type">Category type to request.</param>
    /// <param name="key">An optional key to differentiate injection items.</param>
    /// <param name="target">Information about the injection target which requests this injection.</param>
    /// <returns>Injection with the specified category and key, or null if not found.</returns>
    InjectionItem? GetInjection(Type type, object? key, InjectionTarget target);

    /// <summary>
    /// Scope can manage injections with <see cref="InjectionLifespan.Scoped"/> lifespan.
    /// </summary>
    public interface IScope : IInjectionProvider, IDisposable
    {
        /// <summary>
        /// Injection target for this scope.
        /// </summary>
        public InjectionTarget Target { get; }

        /// <summary>
        /// Parent scope of this scope.
        /// </summary>
        public IScope? Parent { get; }
    }

    /// <summary>
    /// Create a new injection scope for this provider.
    /// </summary>
    /// <param name="target">Injection target for this scope.</param>
    /// <returns>Provider for the new injection scope.</returns>
    [MustDisposeResource]
    IScope NewScope(InjectionTarget target = default);

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
    public static FunctorInjectionProvider FromFunctor(
        FunctorInjectionProvider.ProviderDelegate provider, bool cachingSingletons = true)
        => new(provider, cachingSingletons);

    /// <summary>
    /// Create a new injection provider from a sequence of providers.
    /// The providers will be queried in order until one of them returns a non-null injection.
    /// </summary>
    /// <param name="providers">
    /// Providers to concatenate in sequence;
    /// This sequence will be enumerated every time an injection is requested.
    /// </param>
    /// <returns>Injection provider that queries the specified sequence of providers.</returns>
    public static ChainedInjectionProvider FromMultiple(params IEnumerable<IInjectionProvider?> providers)
        => new(providers);
}