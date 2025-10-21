namespace InjectionExpert;

public static class InjectionContainerExtensions
{
    /// <summary>
    /// Factory delegate to get instances for injection.
    /// </summary>
    /// <typeparam name="TTarget">Type of instances that this factory creates.</typeparam>
    /// <param name="provider">Injection provider that this delegate is registered in.</param>
    /// <param name="type">
    /// Requested type of the injection, NOT the category type;
    /// when the category type is a generic type definition, requested type is different from category type 
    /// </param>
    public delegate TTarget TypedFactoryDelegate<out TTarget>(
        IInjectionProvider provider, Type type, InjectionTarget target);

    /// <summary>
    /// Wrap a typed factory delegate to an untyped factory delegate.
    /// </summary>
    /// <param name="factory">Factory delegate to wrap.</param>
    /// <typeparam name="TTarget">Type of the specified factory delegate.</typeparam>
    /// <returns>Untyped factory delegate that returns an object instance.</returns>
    public static IInjectionContainer.FactoryDelegate WrapToUntypedFactory<TTarget>(TypedFactoryDelegate<TTarget> factory)
        => (provider, type, target) => factory(provider, type, target)!;
    
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
        container.AddInjection(InjectionLifespan.Transient, type, implementation, key);
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
        container.AddInjection(InjectionLifespan.Transient, typeof(TImplementation), typeof(TImplementation), key);
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
        TypedFactoryDelegate<TInjection> factory, object? key = null)
        where TInjection : class
    {
        container.AddInjection(InjectionLifespan.Transient, typeof(TInjection), WrapToUntypedFactory(factory), key);
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
        container.AddInjection(InjectionLifespan.Scoped, type, implementation, key);
        return container;
    }

    /// <summary>
    /// Add the specified implementation type as a scoped injection to this container.
    /// </summary>
    /// <typeparam name="TImplementation">The implementation type to register.</typeparam>
    /// <param name="container">The injection container to add the injection into.</param>
    /// <param name="key">Optional key to distinguish this injection.</param>
    /// <returns>The specified injection container.</returns>
    public static IInjectionContainer AddScoped<TImplementation>(
        this IInjectionContainer container, object? key = null)
    {
        container.AddInjection(InjectionLifespan.Scoped, typeof(TImplementation), typeof(TImplementation), key);
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
        TypedFactoryDelegate<TInjection> factory, object? key = null)
        where TInjection : class
    {
        container.AddInjection(InjectionLifespan.Scoped, typeof(TInjection), WrapToUntypedFactory(factory), key);
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
    public static IInjectionContainer AddSingleton(this IInjectionContainer container,
        Type type, Type implementation, object? key = null)
    {
        container.AddInjection(InjectionLifespan.Singleton, type, implementation, key);
        return container;
    }

    /// <summary>
    /// Add the specified implementation type as a singleton injection to this container.
    /// </summary>
    /// <typeparam name="TImplementation">The implementation type to register.</typeparam>
    /// <param name="container">The injection container to add the injection into.</param>
    /// <param name="key">Optional key to distinguish this injection.</param>
    /// <returns>The specified injection container.</returns>
    public static IInjectionContainer AddSingleton<TImplementation>(
        this IInjectionContainer container, object? key = null)
    {
        container.AddInjection(InjectionLifespan.Singleton, typeof(TImplementation), typeof(TImplementation), key);
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
        TypedFactoryDelegate<TInjection> factory, object? key = null) 
        where TInjection : class
    {
        container.AddInjection(InjectionLifespan.Singleton, typeof(TInjection), WrapToUntypedFactory(factory), key);
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
        TInjection value, object? key = null)
    {
        container.AddInjection(typeof(TInjection), value!, key);
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
    public static IInjectionContainer RemoveInjection<TInjection>(
        this IInjectionContainer container, object? key = null)
    {
        container.RemoveInjection(typeof(TInjection), key);
        return container;
    }
}