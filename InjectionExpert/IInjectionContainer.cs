using System.ComponentModel;

namespace InjectionExpert;

public interface IInjectionContainer : IInjectionProvider
{
    /// <summary>
    /// Factory delegate to create instances for injection.
    /// </summary>
    /// <param name="provider">Injection provider that this delegate is registered in.</param>
    /// <param name="type">
    /// Requested type of the injection, NOT the category type;
    /// when the category type is a generic type definition, requested type is different from category type 
    /// </param>
    public delegate object FactoryDelegate(IInjectionProvider provider, Type type, InjectionTarget target);

    /// <summary>
    /// Add an implementation type for the specified category type in the container.
    /// </summary>
    /// <param name="lifespan">Lifespan of the injected instances.</param>
    /// <param name="type">Category type to register.</param>
    /// <param name="implementation">Implementation type to instantiate for the injection requests.</param>
    /// <param name="key">Optional key to distinguish this injection.</param>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    void AddInjection(InjectionLifespan lifespan, Type type, Type implementation, object? key = null);

    /// <summary>
    /// Registers a factory method for creating instances of the specified category type in the container.
    /// </summary>
    /// <param name="lifespan">Lifespan of the injected instances.</param>
    /// <param name="type">Category type to register.</param>
    /// <param name="factory">Factory method that creates instances.</param>
    /// <param name="key">Optional key to distinguish this injection.</param>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    void AddInjection(InjectionLifespan lifespan, Type type, FactoryDelegate factory, object? key = null);

    /// <summary>
    /// Registers a specific instance for the given category type in the container.
    /// </summary>
    /// <param name="type">Category type to register.</param>
    /// <param name="value">Instance to register.</param>
    /// <param name="key">Optional key to distinguish this injection.</param>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    void AddInjection(Type type, object value, object? key = null);

    /// <summary>
    /// Add a redirection entry into this container.
    /// Requests to the source entry will be redirected to the destination entry,
    /// even if the destination entry does not exist.
    /// </summary>
    /// <remarks>
    /// This redirection will always be in effect, even when destination entry does not exist;
    /// in this situation, the requests to the source entry will always get null.
    /// This method will replace the existing source entry with this redirection entry.
    /// </remarks>
    /// <param name="fromType">Type to redirect from.</param>
    /// <param name="fromKey">Key to redirect from.</param>
    /// <param name="toType">Type to redirect to.</param>
    /// <param name="toKey">Key to redirect to.</param>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    void AddRedirection(Type fromType, object? fromKey, Type toType, object? toKey);

    /// <summary>
    /// Remove an injection entry from the container.
    /// </summary>
    /// <param name="type">Type of the injection to remove.</param>
    /// <param name="key">Optional key for the injection.</param>
    /// <returns>True if the resource is removed from this container, false if it is not found.</returns>
    bool RemoveInjection(Type type, object? key = null);
}