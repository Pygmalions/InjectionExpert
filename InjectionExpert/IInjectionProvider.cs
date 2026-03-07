namespace InjectionExpert;

/// <summary>
/// Represents a provider for managing and resolving dependency injections.
/// </summary>
/// <remarks>
/// This interface serves as the primary abstraction for retrieving injection entries
/// configured for specific types. It allows differentiating between keyed and unkeyed
/// injections and enables flexibility in dependency resolution mechanisms. Typically,
/// implementations of this interface facilitate customization of injection strategies
/// and support scenarios like integration with service providers or chained providers.
/// </remarks>
public interface IInjectionProvider
{
    /// <summary>
    /// Represents a delegate responsible for resolving dependency injections based on the
    /// provided type, key, and target information.
    /// </summary>
    public delegate (object Instance, bool ShouldCache)? InjectionResolver(
        IInjectionProvider provider, Type type, object? key, InjectionTarget target);
    
    /// <summary>
    /// Injection resolvers that are used to resolve injections when no matching entry is found.
    /// </summary>
    IEnumerable<InjectionResolver> Resolvers { get; }
    
    /// <summary>
    /// Retrieves the injection entry for the specified type, key, and target.
    /// </summary>
    /// <param name="type">The type of the injection entry to retrieve.</param>
    /// <param name="key">An optional key to distinguish between multiple registrations of the same type.</param>
    /// <returns>An <see cref="InjectionEntry"/> instance if found; otherwise, null.</returns>
    InjectionEntry? GetEntry(Type type, object? key = null);
    
    /// <summary>
    /// Determines whether an injection of the specified type is registered in this scope.
    /// </summary>
    /// <param name="type">The type of injection to check for.</param>
    /// <param name="key">Optional key for keyed injections. If null, checks for unkeyed injections.</param>
    /// <returns>true if an injection of the specified type exists; otherwise, false.</returns>
    bool HasEntry(Type type, object? key = null);
    
    /// <summary>
    /// Retrieves an injection instance for the specified type, key, and target.
    /// </summary>
    /// <param name="type">The type for which to retrieve the injection instance.</param>
    /// <param name="key">An optional key used to differentiate between multiple registrations of the same type.</param>
    /// <param name="target">The target location where the injection instance will be applied.</param>
    /// <returns>The resolved injection instance if found; otherwise, null.</returns>
    object? GetInjection(Type type, object? key = null, InjectionTarget target = default);
    
    /// <summary>
    /// Creates a new injection scope.
    /// </summary>
    /// <returns>
    /// An instance of <see cref="IInjectionScope"/> representing the newly created scope.
    /// </returns>
    IInjectionScope NewScope();
    
    /// <summary>
    /// Represents a null key; this class is used to unify the management of keyed injections and unkeyed injections.
    /// </summary>
    internal record NullKey
    {
        private NullKey()
        {
        }

        public static NullKey Instance { get; } = new();
    }
}