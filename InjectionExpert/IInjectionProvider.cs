using JetBrains.Annotations;

namespace InjectionExpert;

public interface IInjectionProvider
{
    /// <summary>
    /// Search an injection item for the specified category type.
    /// </summary>
    /// <param name="type">Category type to request.</param>
    /// <param name="key">An optional key to differentiate injection items.</param>
    /// <param name="target">Information about the injection target which requests this injection.</param>
    /// <returns>Injection with the specified category and key, or null if not found.</returns>
    InjectionItem? GetInjectionItem(Type type, object? key = null, InjectionTarget target = default);

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
}