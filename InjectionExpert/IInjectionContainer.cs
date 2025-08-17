using System.ComponentModel;

namespace InjectionExpert;

public interface IInjectionContainer : IInjectionProvider
{
    public delegate (object Injection, InjectionLifespan Lifespan)? FactoryDelegate(
        IInjectionProvider provider, Type type, object? key, InjectionTarget target);
    
    /// <summary>
    /// Add a factory with the specified name to this container.
    /// Previous factory with the same name will be replaced,
    /// and all singleton caches will be cleared.
    /// </summary>
    /// <param name="name">Name of the factory to add.</param>
    /// <param name="factory">Factory to add.</param>
    void AddFactory(string name, FactoryDelegate factory);
    
    /// <summary>
    /// Remove the factory with the specified name from this container.
    /// All singleton caches will be cleared.
    /// </summary>
    /// <param name="name">Name of the factory to remove.</param>
    /// <returns>True if the factory is found and removed.</returns>
    bool RemoveFactory(string name);
    
    /// <summary>
    /// Add a type to this container.
    /// This implementation type will be instantiated and injected when the corresponding type is requested.
    /// </summary>
    /// <param name="type">Type that this implementation is bound to.</param>
    /// <param name="implementation">Implementation type to add.</param>
    /// <param name="lifespan">Lifespan of instances instantiated from this implementation type.</param>
    /// <param name="key">Optional key for the injection.</param>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    void AddInjection(Type type, Type implementation, InjectionLifespan lifespan,
        object? key = null);

    /// <summary>
    /// Add a factory to this container.
    /// This factory method will be invoked when the corresponding type is requested.
    /// </summary>
    /// <param name="type">Type that this factory is bound to.</param>
    /// <param name="factory">Factory to add.</param>
    /// <param name="lifespan">Lifespan of instances returned by this factory.</param>
    /// <param name="key">Optional key for the injection.</param>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    void AddInjection(Type type, Func<IInjectionProvider, InjectionTarget, object> factory,
        InjectionLifespan lifespan,
        object? key = null);

    /// <summary>
    /// Add a value to this container.
    /// This value will be returned when the corresponding type is requested.
    /// </summary>
    /// <param name="type">Type that this value is bound to.</param>
    /// <param name="value">Value to add.</param>
    /// <param name="key">Optional key for the injection.</param>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    void AddInjection(Type type, object value, object? key = null);

    /// <summary>
    /// Add a redirection entry into this container.
    /// The request to injections of the 'fromType' with 'fromKey'
    /// will be considered as the request to injections of the 'toType' with 'toKey'. <br/>
    /// This redirection can also be removed by <see cref="RemoveInjection"/> method
    /// with 'fromType' and 'fromKey' as arguments.
    /// </summary>
    /// <remarks>
    /// This redirection will always be in effect, even if it is added before the entry to redirect to is added. <br/>
    /// The entry of 'fromType' with 'fromKey' will be replaced by this redirection entry,
    /// this operation is not reversible. <br/>
    /// </remarks>
    /// <param name="fromType">Type to redirect from.</param>
    /// <param name="fromKey">Key to redirect from.</param>
    /// <param name="toType">Type to redirect to.</param>
    /// <param name="toKey">Key to redirect to.</param>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    void AddRedirection(Type fromType, object? fromKey, Type toType, object? toKey);
    
    /// <summary>
    /// Try to remove a resource from the container.
    /// </summary>
    /// <param name="type">Type of the resource to remove.</param>
    /// <param name="key">Optional key for the injection.</param>
    /// <returns>True if the resource is removed from this container, false if it is not found.</returns>
    bool RemoveInjection(Type type, object? key = null);
}