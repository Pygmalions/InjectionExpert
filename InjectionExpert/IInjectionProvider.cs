using InjectionExpert.Utilities;
using JetBrains.Annotations;

namespace InjectionExpert;

public interface IInjectionProvider
{
    /// <summary>
    /// Get a resource of the specified category for this provider.
    /// </summary>
    /// <param name="type">Category type of the resource.</param>
    /// <param name="key">Optional key for the requested injection.</param>
    /// <param name="target">Optional information about the target requested this injection.</param>
    /// <returns>Requested resource, or null if not found.</returns>
    (object Injection, InjectionLifespan Lifespan)? GetInjection(Type type, object? key, InjectionTarget target);
    
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

    public static FunctorInjectionProvider FromFunctor(FunctorInjectionProvider.FactoryDelegate factory)
        => new (factory);
    
    public static ChainedInjectionProvider FromMultiple(params IEnumerable<IInjectionProvider> providers)
        => new (providers);
}