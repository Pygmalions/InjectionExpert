namespace InjectionExpert;

public static class InjectionContainerExtensions
{
    /// <summary>
    /// Add the specified implementation type as a transient injection to this container.
    /// </summary>
    /// <param name="container">Injection container to add injection into.</param>
    /// <param name="type">Category type for this injection to register.</param>
    /// <param name="implementation">Type to instantiate for the requests.</param>
    /// <param name="key">Optional key to distinguish this injection.</param>
    /// <returns>The specified injection container.</returns>
    public static IInjectionContainer AddTransient(this IInjectionContainer container, 
        Type type, Type implementation, object? key = null)
    {
        container.AddInjection(type, implementation, InjectionLifespan.Transient, key);
        return container;
    }

    /// <summary>
    /// Add the specified implementation type as a transient injection to this container.
    /// </summary>
    /// <typeparam name="TImplementation">The implementation type to register.</typeparam>
    /// <param name="container">The injection container to add the injection into.</param>
    /// <param name="key">Optional key to distinguish this injection.</param>
    /// <returns>The specified injection container.</returns>
    public static IInjectionContainer AddTransient<TImplementation>(this IInjectionContainer container,
        object? key = null)
    {
        container.AddInjection(typeof(TImplementation), typeof(TImplementation), InjectionLifespan.Transient, key);
        return container;
    }

    /// <summary>
    /// Add a transient injection using a factory method for the specified type.
    /// </summary>
    /// <typeparam name="TInjection">The category type to register.</typeparam>
    /// <param name="container">The injection container to add the injection into.</param>
    /// <param name="factory">A factory function to create instances of <typeparamref name="TInjection"/>.</param>
    /// <param name="key">Optional key to distinguish this injection.</param>
    /// <returns>The specified injection container.</returns>
    public static IInjectionContainer AddTransient<TInjection>(this IInjectionContainer container,
        Func<IInjectionProvider, InjectionTarget, TInjection> factory, object? key = null) where TInjection : class
    {
        container.AddInjection(typeof(TInjection), factory, InjectionLifespan.Transient, key);
        return container;
    }
    
    /// <summary>
    /// Add the specified implementation type as a scoped injection to this container.
    /// </summary>
    /// <param name="container">Injection container to add injection into.</param>
    /// <param name="type">Category type for this injection to register.</param>
    /// <param name="implementation">Type to instantiate for the requests.</param>
    /// <param name="key">Optional key to distinguish this injection.</param>
    /// <returns>The specified injection container.</returns>
    public static IInjectionContainer AddScoped(this IInjectionContainer container, 
        Type type, Type implementation, object? key = null)
    {
        container.AddInjection(type, implementation, InjectionLifespan.Scoped, key);
        return container;
    }

    /// <summary>
    /// Add the specified implementation type as a scoped injection to this container.
    /// </summary>
    /// <typeparam name="TImplementation">The implementation type to register.</typeparam>
    /// <param name="container">The injection container to add the injection into.</param>
    /// <param name="key">Optional key to distinguish this injection.</param>
    /// <returns>The specified injection container.</returns>
    public static IInjectionContainer AddScoped<TImplementation>(this IInjectionContainer container,
        object? key = null)
    {
        container.AddInjection(typeof(TImplementation), typeof(TImplementation), 
            InjectionLifespan.Scoped, key);
        return container;
    }

    /// <summary>
    /// Add a scoped injection using a factory method for the specified type.
    /// </summary>
    /// <typeparam name="TInjection">The category type to register.</typeparam>
    /// <param name="container">The injection container to add the injection into.</param>
    /// <param name="factory">A factory function to create instances of <typeparamref name="TInjection"/>.</param>
    /// <param name="key">Optional key to distinguish this injection.</param>
    /// <returns>The specified injection container.</returns>
    public static IInjectionContainer AddScoped<TInjection>(this IInjectionContainer container,
        Func<IInjectionProvider, InjectionTarget, TInjection> factory, object? key = null) where TInjection : class
    {
        container.AddInjection(typeof(TInjection), factory, InjectionLifespan.Scoped, key);
        return container;
    }

    /// <summary>
    /// Add the specified implementation type as a singleton injection to this container.
    /// </summary>
    /// <param name="container">Injection container to add injection into.</param>
    /// <param name="type">Category type for this injection to register.</param>
    /// <param name="implementation">Type to instantiate for the requests.</param>
    /// <param name="key">Optional key to distinguish this injection.</param>
    /// <returns>The specified injection container.</returns>
    public static IInjectionContainer AddSingleton(this IInjectionContainer container, Type type, Type implementation,
        object? key = null)
    {
        container.AddInjection(type, implementation, InjectionLifespan.Singleton, key);
        return container;
    }

    /// <summary>
    /// Add the specified implementation type as a singleton injection to this container.
    /// </summary>
    /// <typeparam name="TImplementation">The implementation type to register.</typeparam>
    /// <param name="container">The injection container to add the injection into.</param>
    /// <param name="key">Optional key to distinguish this injection.</param>
    /// <returns>The specified injection container.</returns>
    public static IInjectionContainer AddSingleton<TImplementation>(this IInjectionContainer container,
        object? key = null)
    {
        container.AddInjection(typeof(TImplementation), typeof(TImplementation), InjectionLifespan.Singleton, key);
        return container;
    }

    /// <summary>
    /// Add a singleton injection using a factory method for the specified type.
    /// </summary>
    /// <typeparam name="TInjection">The category type to register.</typeparam>
    /// <param name="container">The injection container to add the injection into.</param>
    /// <param name="factory">A factory function to create instances of <typeparamref name="TInjection"/>.</param>
    /// <param name="key">Optional key to distinguish this injection.</param>
    /// <returns>The specified injection container.</returns>
    public static IInjectionContainer AddSingleton<TInjection>(this IInjectionContainer container,
        Func<IInjectionProvider, InjectionTarget, TInjection> factory, object? key = null) where TInjection : class
    {
        container.AddInjection(typeof(TInjection), factory, InjectionLifespan.Singleton, key);
        return container;
    }

    /// <summary>
    /// Add the specified instance as a singleton injection to this container.
    /// </summary>
    /// <typeparam name="TInjection">The category type to register.</typeparam>
    /// <param name="container">The injection container to add the injection into.</param>
    /// <param name="value">The instance to register.</param>
    /// <param name="key">Optional key to distinguish this injection.</param>
    /// <returns>The specified injection container.</returns>
    public static IInjectionContainer AddSingleton<TInjection>(this IInjectionContainer container,
        TInjection value, object? key = null) where TInjection : notnull
    {
        container.AddInjection(typeof(TInjection), value, key);
        return container;
    }
    
    /// <summary>
    /// Add a type redirection so that requests for <typeparamref name="TFrom"/> are redirected to <typeparamref name="TTo"/>.
    /// </summary>
    /// <typeparam name="TFrom">The source type.</typeparam>
    /// <typeparam name="TTo">The target type.</typeparam>
    /// <param name="container">The injection container to add the redirection into.</param>
    /// <param name="fromKey">Optional key for the source type.</param>
    /// <param name="toKey">Optional key for the target type.</param>
    /// <returns>The specified injection container.</returns>
    public static IInjectionContainer AddRedirection<TFrom, TTo>(this IInjectionContainer container,
        object? fromKey = null, object? toKey = null)
    {
        container.AddRedirection(typeof(TFrom), fromKey, typeof(TTo), toKey);
        return container;
    }

    /// <summary>
    /// Remove the injection for the specified type from this container.
    /// </summary>
    /// <typeparam name="TInjection">The category type to remove.</typeparam>
    /// <param name="container">The injection container to remove the injection from.</param>
    /// <param name="key">Optional key to distinguish this injection.</param>
    /// <returns>The specified injection container.</returns>
    public static IInjectionContainer RemoveInjection<TInjection>(this IInjectionContainer container,
        object? key = null)
    {
        container.RemoveInjection(typeof(TInjection), key);
        return container;
    }
}