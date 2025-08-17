namespace InjectionExpert;

public static class InjectionContainerExtensions
{
    public static IInjectionContainer AddTransient(this IInjectionContainer container, 
        Type type, Type implementation, object? key = null)
    {
        container.AddInjection(type, implementation, InjectionLifespan.Transient, key);
        return container;
    }

    public static IInjectionContainer AddTransient<TImplementation>(this IInjectionContainer container,
        object? key = null)
    {
        container.AddInjection(typeof(TImplementation), typeof(TImplementation), InjectionLifespan.Transient, key);
        return container;
    }

    public static IInjectionContainer AddTransient<TInjection>(this IInjectionContainer container,
        Func<IInjectionProvider, InjectionTarget, TInjection> factory, object? key = null) where TInjection : class
    {
        container.AddInjection(typeof(TInjection), factory, InjectionLifespan.Transient, key);
        return container;
    }
    
    public static IInjectionContainer AddScoped(this IInjectionContainer container, 
        Type type, Type implementation, object? key = null)
    {
        container.AddInjection(type, implementation, InjectionLifespan.Scoped, key);
        return container;
    }

    public static IInjectionContainer AddScoped<TImplementation>(this IInjectionContainer container,
        object? key = null)
    {
        container.AddInjection(typeof(TImplementation), typeof(TImplementation), 
            InjectionLifespan.Scoped, key);
        return container;
    }

    public static IInjectionContainer AddScoped<TInjection>(this IInjectionContainer container,
        Func<IInjectionProvider, InjectionTarget, TInjection> factory, object? key = null) where TInjection : class
    {
        container.AddInjection(typeof(TInjection), factory, InjectionLifespan.Scoped, key);
        return container;
    }

    public static IInjectionContainer AddSingleton(this IInjectionContainer container, Type type, Type implementation,
        object? key = null)
    {
        container.AddInjection(type, implementation, InjectionLifespan.Singleton, key);
        return container;
    }

    public static IInjectionContainer AddSingleton<TImplementation>(this IInjectionContainer container,
        object? key = null)
    {
        container.AddInjection(typeof(TImplementation), typeof(TImplementation), InjectionLifespan.Singleton, key);
        return container;
    }

    public static IInjectionContainer AddSingleton<TInjection>(this IInjectionContainer container,
        Func<IInjectionProvider, InjectionTarget, TInjection> factory, object? key = null) where TInjection : class
    {
        container.AddInjection(typeof(TInjection), factory, InjectionLifespan.Singleton, key);
        return container;
    }

    public static IInjectionContainer AddConstant<TInjection>(this IInjectionContainer container,
        TInjection value, object? key = null) where TInjection : notnull
    {
        container.AddInjection(typeof(TInjection), value, key);
        return container;
    }
    
    public static IInjectionContainer AddRedirection<TFrom, TTo>(this IInjectionContainer container,
        object? fromKey = null, object? toKey = null)
    {
        container.AddRedirection(typeof(TFrom), fromKey, typeof(TTo), toKey);
        return container;
    }

    public static IInjectionContainer RemoveInjection<TInjection>(this IInjectionContainer container,
        object? key = null)
    {
        container.RemoveInjection(typeof(TInjection), key);
        return container;
    }
}