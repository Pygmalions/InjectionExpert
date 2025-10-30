using InjectionExpert.Entries;

namespace InjectionExpert;

public static class InjectionContainerExtensions
{
    extension<TContainer>(TContainer container) where TContainer : IInjectionContainer
    {
        /// <summary>
        /// Add the specified instance as a constant injection to this container.
        /// </summary>
        /// <param name="type">Type that the entry is associated with.</param>
        /// <param name="instance">Instance to add.</param>
        /// <param name="key">Optional key of the entry.</param>
        public TContainer AddSingleton(Type type, object instance, object? key = null)
        {
            container.AddInjectionEntry(type, key, new InjectionConstantEntry(instance));
            return container;
        }

        /// <summary>
        /// Add the specified instance as a constant injection to this container.
        /// </summary>
        /// <typeparam name="TInjection">Type that the entry is associated with.</typeparam>
        /// <param name="instance">Instance to add.</param>
        /// <param name="key">Optional key of the entry.</param>
        public TContainer AddSingleton<TInjection>(TInjection instance, object? key = null)
        {
            container.AddInjectionEntry(typeof(TInjection), key, new InjectionConstantEntry(instance!));
            return container;
        }

        /// <summary>
        /// Add a redirection from one type/key to another type/key.
        /// </summary>
        /// <param name="fromType">Type to redirect from.</param>
        /// <param name="fromKey">Key to redirect from.</param>
        /// <param name="toType">Type to redirect to.</param>
        /// <param name="toKey">Key to redirect to.</param>
        public TContainer AddRedirection(Type fromType, object? fromKey, Type toType, object? toKey)
        {
            container.AddInjectionEntry(fromType, fromKey,
                new InjectionRedirectionEntry(toType, toKey));
            return container;
        }

        /// <summary>
        /// Add a redirection from one type/key to another type/key.
        /// </summary>
        /// <typeparam name="TFrom">Type to redirect from.</typeparam>
        /// <typeparam name="TTo">Type to redirect to.</typeparam>
        /// <param name="fromKey">Key to redirect from.</param>
        /// <param name="toKey">Key to redirect to.</param>
        public TContainer AddRedirection<TFrom, TTo>(object? fromKey = null, object? toKey = null)
            => container.AddRedirection(typeof(TFrom), fromKey, typeof(TTo), toKey);

        /// <summary>
        /// Add the specified implementation type to this container.
        /// </summary>
        /// <param name="lifespan">Lifespan of this injection.</param>
        /// <param name="type">Type that the entry is associated with.</param>
        /// <param name="implementation">Type to instantiate for requests.</param>
        /// <param name="key">Optional key of the entry.</param>
        public TContainer AddInjection(InjectionLifespan lifespan,
            Type type, Type implementation, object? key = null)
        {
            if (!type.IsGenericTypeDefinition)
                container.AddInjectionEntry(type, key,
                    new InjectionTypeEntry(lifespan, implementation));
            else
                container.AddInjectionEntry(type, key,
                    new InjectionTypeDefinitionEntry(lifespan, type, implementation));
            return container;
        }

        /// <summary>
        /// Add the specified implementation type as a transient injection to this container.
        /// </summary>
        /// <param name="type">Type that the entry is associated with.</param>
        /// <param name="implementation">Type to instantiate for requests.</param>
        /// <param name="key">Optional key of the entry.</param>
        public TContainer AddTransient(Type type, Type implementation, object? key = null)
            => container.AddInjection(InjectionLifespan.Transient, type, implementation, key);

        /// <summary>
        /// Add the specified implementation type as a transient injection to this container.
        /// </summary>
        /// <typeparam name="TImplementation">
        /// Type to instantiate for requests. The injection is also added under this type.
        /// </typeparam>
        /// <param name="key">Optional key of the entry.</param>
        public TContainer AddTransient<TImplementation>(object? key = null)
            => container.AddInjection(InjectionLifespan.Transient,
                typeof(TImplementation), typeof(TImplementation), key);

        /// <summary>
        /// Add the specified implementation type as a transient injection to this container.
        /// </summary>
        /// <typeparam name="TCategory">Category type that this injection is associated with.</typeparam>
        /// <typeparam name="TImplementation">Type to instantiate for requests. 
        /// </typeparam>
        /// <param name="key">Optional key of the entry.</param>
        public TContainer AddTransient<TCategory, TImplementation>(object? key = null)
            => container.AddInjection(InjectionLifespan.Transient,
                typeof(TCategory), typeof(TImplementation), key);

        /// <summary>
        /// Add the specified implementation type as a scoped injection to this container.
        /// </summary>
        /// <param name="type">Type that the entry is associated with.</param>
        /// <param name="implementation">Type to instantiate for requests.</param>
        /// <param name="key">Optional key of the entry.</param>
        public TContainer AddScoped(Type type, Type implementation, object? key = null)
            => container.AddInjection(InjectionLifespan.Scoped, type, implementation, key);

        /// <summary>
        /// Add the specified implementation type as a scoped injection to this container.
        /// </summary>
        /// <typeparam name="TImplementation">
        /// Type to instantiate for requests. The injection is also added under this type.
        /// </typeparam>
        /// <param name="key">Optional key of the entry.</param>
        public TContainer AddScoped<TImplementation>(object? key = null)
            => container.AddInjection(InjectionLifespan.Scoped,
                typeof(TImplementation), typeof(TImplementation), key);

        /// <summary>
        /// Add the specified implementation type as a scoped injection to this container.
        /// </summary>
        /// <typeparam name="TCategory">Category type that this injection is associated with.</typeparam>
        /// <typeparam name="TImplementation">Type to instantiate for requests.</typeparam>
        /// <param name="key">Optional key of the entry.</param>
        public TContainer AddScoped<TCategory, TImplementation>(object? key = null)
            => container.AddInjection(InjectionLifespan.Scoped,
                typeof(TCategory), typeof(TImplementation), key);

        /// <summary>
        /// Add the specified implementation type as a singleton injection to this container.
        /// </summary>
        /// <param name="type">Type that the entry is associated with.</param>
        /// <param name="implementation">Type to instantiate for requests.</param>
        /// <param name="key">Optional key of the entry.</param>
        public TContainer AddSingleton(Type type, Type implementation, object? key = null)
            => container.AddInjection(InjectionLifespan.Singleton, type, implementation, key);

        /// <summary>
        /// Add the specified implementation type as a singleton injection to this container.
        /// </summary>
        /// <typeparam name="TImplementation">
        /// Type to instantiate for requests. The injection is also added under this type.
        /// </typeparam>
        /// <param name="key">Optional key of the entry.</param>
        public TContainer AddSingleton<TImplementation>(object? key = null)
            => container.AddInjection(InjectionLifespan.Singleton,
                typeof(TImplementation), typeof(TImplementation), key);

        /// <summary>
        /// Add the specified implementation type as a singleton injection to this container.
        /// </summary>
        /// <typeparam name="TCategory">Category type that this injection is associated with.</typeparam>
        /// <typeparam name="TImplementation">Type to instantiate for requests.</typeparam>
        /// <param name="key">Optional key of the entry.</param>
        public TContainer AddSingleton<TCategory, TImplementation>(object? key = null)
            => container.AddInjection(InjectionLifespan.Singleton,
                typeof(TCategory), typeof(TImplementation), key);

        /// <summary>
        /// Add the specified factory to this container.
        /// </summary>
        /// <param name="lifespan">Lifespan of this injection.</param>
        /// <param name="type">Type that the entry is associated with.</param>
        /// <param name="factory">Factory delegate that creates instances for requests.</param>
        /// <param name="key">Optional key of the entry.</param>
        public TContainer AddInjection(InjectionLifespan lifespan,
            Type type, InjectionFactoryEntry.FactoryDelegate<object> factory, object? key = null)
        {
            container.AddInjectionEntry(type, key,
                new InjectionFactoryEntry<object>(lifespan, factory));
            return container;
        }

        /// <summary>
        /// Add the specified factory to this container.
        /// </summary>
        /// <typeparam name="TInjection">Type that the entry is associated with.</typeparam>
        /// <param name="lifespan">Lifespan of this injection.</param>
        /// <param name="factory">Factory delegate that creates instances for requests.</param>
        /// <param name="key">Optional key of the entry.</param>
        public TContainer AddInjection<TInjection>(InjectionLifespan lifespan,
            InjectionFactoryEntry.FactoryDelegate<TInjection> factory, object? key = null)
        {
            container.AddInjectionEntry(typeof(TInjection), key,
                new InjectionFactoryEntry<TInjection>(lifespan, factory));
            return container;
        }

        /// <summary>
        /// Add the specified factory as a transient injection to this container.
        /// </summary>
        /// <param name="type">Type that the entry is associated with.</param>
        /// <param name="factory">Factory delegate that creates instances for requests.</param>
        /// <param name="key">Optional key of the entry.</param>
        public TContainer AddTransient(Type type, InjectionFactoryEntry.FactoryDelegate<object> factory,
            object? key = null)
            => container.AddInjection(InjectionLifespan.Transient, type, factory, key);

        /// <summary>
        /// Add the specified factory as a transient injection to this container.
        /// </summary>
        /// <typeparam name="TInjection">Type that the entry is associated with.</typeparam>
        /// <param name="factory">Factory delegate that creates instances for requests.</param>
        /// <param name="key">Optional key of the entry.</param>
        public TContainer AddTransient<TInjection>(InjectionFactoryEntry.FactoryDelegate<TInjection> factory,
            object? key = null)
            => container.AddInjection(InjectionLifespan.Transient, factory, key);

        /// <summary>
        /// Add the specified factory as a scoped injection to this container.
        /// </summary>
        /// <param name="type">Type that the entry is associated with.</param>
        /// <param name="factory">Factory delegate that creates instances for requests.</param>
        /// <param name="key">Optional key of the entry.</param>
        public TContainer AddScoped(Type type, InjectionFactoryEntry.FactoryDelegate<object> factory,
            object? key = null)
            => container.AddInjection(InjectionLifespan.Scoped, type, factory, key);

        /// <summary>
        /// Add the specified factory as a scoped injection to this container.
        /// </summary>
        /// <typeparam name="TInjection">Type that the entry is associated with.</typeparam>
        /// <param name="factory">Factory delegate that creates instances for requests.</param>
        /// <param name="key">Optional key of the entry.</param>
        public TContainer AddScoped<TInjection>(InjectionFactoryEntry.FactoryDelegate<TInjection> factory,
            object? key = null)
            => container.AddInjection(InjectionLifespan.Scoped, factory, key);

        /// <summary>
        /// Add the specified factory as a singleton injection to this container.
        /// </summary>
        /// <param name="type">Type that the entry is associated with.</param>
        /// <param name="factory">Factory delegate that creates instances for requests.</param>
        /// <param name="key">Optional key of the entry.</param>
        public TContainer AddSingleton(Type type, InjectionFactoryEntry.FactoryDelegate<object> factory,
            object? key = null)
            => container.AddInjection(InjectionLifespan.Singleton, type, factory, key);

        /// <summary>
        /// Add the specified factory as a singleton injection to this container.
        /// </summary>
        /// <typeparam name="TInjection">Type that the entry is associated with.</typeparam>
        /// <param name="factory">Factory delegate that creates instances for requests.</param>
        /// <param name="key">Optional key of the entry.</param>
        public TContainer AddSingleton<TInjection>(InjectionFactoryEntry.FactoryDelegate<TInjection> factory,
            object? key = null)
            => container.AddInjection(InjectionLifespan.Singleton, factory, key);
    }

    extension(IInjectionContainer container)
    {
        /// <summary>
        /// Add the specified instance as a constant injection to this container.
        /// </summary>
        /// <param name="type">Type that the entry is associated with.</param>
        /// <param name="instance">Instance to add.</param>
        /// <param name="key">Optional key of the entry.</param>
        public bool TryAddSingleton(Type type, object instance, object? key = null)
            => container.TryAddInjectionEntry(type, key, new InjectionConstantEntry(instance));

        /// <summary>
        /// Add the specified instance as a constant injection to this container.
        /// </summary>
        /// <param name="type">Type that the entry is associated with.</param>
        /// <param name="instance">Instance to add.</param>
        /// <param name="key">Optional key of the entry.</param>
        public bool TryAddSingleton<TInjection>(Type type, TInjection instance, object? key = null)
            => container.TryAddInjectionEntry(type, key, new InjectionConstantEntry(instance!));

        /// <summary>
        /// Add a redirection from one type/key to another type/key.
        /// </summary>
        /// <param name="fromType">Type to redirect from.</param>
        /// <param name="fromKey">Key to redirect from.</param>
        /// <param name="toType">Type to redirect to.</param>
        /// <param name="toKey">Key to redirect to.</param>
        public bool TryAddRedirection(Type fromType, object? fromKey, Type toType, object? toKey)
            => container.TryAddInjectionEntry(fromType, fromKey,
                new InjectionRedirectionEntry(toType, toKey));

        /// <summary>
        /// Add a redirection from one type/key to another type/key.
        /// </summary>
        /// <typeparam name="TFrom">Type to redirect from.</typeparam>
        /// <typeparam name="TTo">Type to redirect to.</typeparam>
        /// <param name="fromKey">Key to redirect from.</param>
        /// <param name="toKey">Key to redirect to.</param>
        public bool TryAddRedirection<TFrom, TTo>(object? fromKey = null, object? toKey = null)
            => container.TryAddRedirection(typeof(TFrom), fromKey, typeof(TTo), toKey);

        /// <summary>
        /// Add the specified implementation type to this container.
        /// </summary>
        /// <param name="lifespan">Lifespan of this injection.</param>
        /// <param name="type">Type that the entry is associated with.</param>
        /// <param name="implementation">Type to instantiate for requests.</param>
        /// <param name="key">Optional key of the entry.</param>
        public bool TryAddInjection(InjectionLifespan lifespan,
            Type type, Type implementation, object? key = null)
        {
            if (!type.IsGenericTypeDefinition)
                return container.TryAddInjectionEntry(type, key,
                    new InjectionTypeEntry(lifespan, implementation));

            return container.TryAddInjectionEntry(type, key,
                new InjectionTypeDefinitionEntry(lifespan, type, implementation));
        }

        /// <summary>
        /// Add the specified implementation type as a transient injection to this container.
        /// </summary>
        /// <param name="type">Type that the entry is associated with.</param>
        /// <param name="implementation">Type to instantiate for requests.</param>
        /// <param name="key">Optional key of the entry.</param>
        public bool TryAddTransient(Type type, Type implementation, object? key = null)
            => container.TryAddInjection(InjectionLifespan.Transient, type, implementation, key);

        /// <summary>
        /// Add the specified implementation type as a transient injection to this container.
        /// </summary>
        /// <typeparam name="TImplementation">
        /// Type to instantiate for requests. The injection is also added under this type.
        /// </typeparam>
        /// <param name="key">Optional key of the entry.</param>
        public bool TryAddTransient<TImplementation>(object? key = null)
            => container.TryAddInjection(InjectionLifespan.Transient,
                typeof(TImplementation), typeof(TImplementation), key);

        /// <summary>
        /// Add the specified implementation type as a transient injection to this container.
        /// </summary>
        /// <typeparam name="TCategory">Category type that this injection is associated with.</typeparam>
        /// <typeparam name="TImplementation">Type to instantiate for requests.</typeparam>
        /// <param name="key">Optional key of the entry.</param>
        public bool TryAddTransient<TCategory, TImplementation>(object? key = null)
            => container.TryAddInjection(InjectionLifespan.Transient,
                typeof(TCategory), typeof(TImplementation), key);

        /// <summary>
        /// Add the specified implementation type as a scoped injection to this container.
        /// </summary>
        /// <param name="type">Type that the entry is associated with.</param>
        /// <param name="implementation">Type to instantiate for requests.</param>
        /// <param name="key">Optional key of the entry.</param>
        public bool TryAddScoped(Type type, Type implementation, object? key = null)
            => container.TryAddInjection(InjectionLifespan.Scoped, type, implementation, key);

        /// <summary>
        /// Add the specified implementation type as a scoped injection to this container.
        /// </summary>
        /// <typeparam name="TImplementation">
        /// Type to instantiate for requests. The injection is also added under this type.
        /// </typeparam>
        /// <param name="key">Optional key of the entry.</param>
        public bool TryAddScoped<TImplementation>(object? key = null)
            => container.TryAddInjection(InjectionLifespan.Scoped,
                typeof(TImplementation), typeof(TImplementation), key);

        /// <summary>
        /// Add the specified implementation type as a scoped injection to this container.
        /// </summary>
        /// <typeparam name="TCategory">Category type that this injection is associated with.</typeparam>
        /// <typeparam name="TImplementation">Type to instantiate for requests.</typeparam>
        /// <param name="key">Optional key of the entry.</param>
        public bool TryAddScoped<TCategory, TImplementation>(object? key = null)
            => container.TryAddInjection(InjectionLifespan.Scoped,
                typeof(TCategory), typeof(TImplementation), key);

        /// <summary>
        /// Add the specified implementation type as a singleton injection to this container.
        /// </summary>
        /// <param name="type">Type that the entry is associated with.</param>
        /// <param name="implementation">Type to instantiate for requests.</param>
        /// <param name="key">Optional key of the entry.</param>
        public bool TryAddSingleton(Type type, Type implementation, object? key = null)
            => container.TryAddInjection(InjectionLifespan.Singleton, type, implementation, key);

        /// <summary>
        /// Add the specified implementation type as a singleton injection to this container.
        /// </summary>
        /// <typeparam name="TImplementation">
        /// Type to instantiate for requests. The injection is also added under this type.
        /// </typeparam>
        /// <param name="key">Optional key of the entry.</param>
        public bool TryAddSingleton<TImplementation>(object? key = null)
            => container.TryAddInjection(InjectionLifespan.Singleton,
                typeof(TImplementation), typeof(TImplementation), key);

        /// <summary>
        /// Add the specified implementation type as a singleton injection to this container.
        /// </summary>
        /// <typeparam name="TCategory">Category type that this injection is associated with.</typeparam>
        /// <typeparam name="TImplementation">Type to instantiate for requests.</typeparam>
        /// <param name="key">Optional key of the entry.</param>
        public bool TryAddSingleton<TCategory, TImplementation>(object? key = null)
            => container.TryAddInjection(InjectionLifespan.Singleton,
                typeof(TCategory), typeof(TImplementation), key);

        /// <summary>
        /// Add the specified factory to this container.
        /// </summary>
        /// <param name="lifespan">Lifespan of this injection.</param>
        /// <param name="type">Type that the entry is associated with.</param>
        /// <param name="factory">Factory delegate that creates instances for requests.</param>
        /// <param name="key">Optional key of the entry.</param>
        public bool TryAddInjection(InjectionLifespan lifespan,
            Type type, InjectionFactoryEntry.FactoryDelegate<object> factory, object? key = null)
            => container.TryAddInjectionEntry(type, key,
                new InjectionFactoryEntry<object>(lifespan, factory));

        /// <summary>
        /// Add the specified factory to this container.
        /// </summary>
        /// <typeparam name="TInjection">Type that the entry is associated with.</typeparam>
        /// <param name="lifespan">Lifespan of this injection.</param>
        /// <param name="factory">Factory delegate that creates instances for requests.</param>
        /// <param name="key">Optional key of the entry.</param>
        public bool TryAddInjection<TInjection>(InjectionLifespan lifespan,
            InjectionFactoryEntry.FactoryDelegate<TInjection> factory, object? key = null)
            => container.TryAddInjectionEntry(typeof(TInjection), key,
                new InjectionFactoryEntry<TInjection>(lifespan, factory));

        /// <summary>
        /// Add the specified factory as a transient injection to this container.
        /// </summary>
        /// <param name="type">Type that the entry is associated with.</param>
        /// <param name="factory">Factory delegate that creates instances for requests.</param>
        /// <param name="key">Optional key of the entry.</param>
        public bool TryAddTransient(Type type, InjectionFactoryEntry.FactoryDelegate<object> factory,
            object? key = null)
            => container.TryAddInjection(InjectionLifespan.Transient, type, factory, key);

        /// <summary>
        /// Add the specified factory as a transient injection to this container.
        /// </summary>
        /// <typeparam name="TInjection">Type that the entry is associated with.</typeparam>
        /// <param name="factory">Factory delegate that creates instances for requests.</param>
        /// <param name="key">Optional key of the entry.</param>
        public bool TryAddTransient<TInjection>(InjectionFactoryEntry.FactoryDelegate<TInjection> factory,
            object? key = null)
            => container.TryAddInjection(InjectionLifespan.Transient, factory, key);

        /// <summary>
        /// Add the specified factory as a scoped injection to this container.
        /// </summary>
        /// <param name="type">Type that the entry is associated with.</param>
        /// <param name="factory">Factory delegate that creates instances for requests.</param>
        /// <param name="key">Optional key of the entry.</param>
        public bool TryAddScoped(Type type, InjectionFactoryEntry.FactoryDelegate<object> factory,
            object? key = null)
            => container.TryAddInjection(InjectionLifespan.Scoped, type, factory, key);

        /// <summary>
        /// Add the specified factory as a scoped injection to this container.
        /// </summary>
        /// <typeparam name="TInjection">Type that the entry is associated with.</typeparam>
        /// <param name="factory">Factory delegate that creates instances for requests.</param>
        /// <param name="key">Optional key of the entry.</param>
        public bool TryAddScoped<TInjection>(InjectionFactoryEntry.FactoryDelegate<TInjection> factory,
            object? key = null)
            => container.TryAddInjection(InjectionLifespan.Scoped, factory, key);

        /// <summary>
        /// Add the specified factory as a singleton injection to this container.
        /// </summary>
        /// <param name="type">Type that the entry is associated with.</param>
        /// <param name="factory">Factory delegate that creates instances for requests.</param>
        /// <param name="key">Optional key of the entry.</param>
        public bool TryAddSingleton(Type type, InjectionFactoryEntry.FactoryDelegate<object> factory,
            object? key = null)
            => container.TryAddInjection(InjectionLifespan.Singleton, type, factory, key);

        /// <summary>
        /// Add the specified factory as a singleton injection to this container.
        /// </summary>
        /// <typeparam name="TInjection">Type that the entry is associated with.</typeparam>
        /// <param name="factory">Factory delegate that creates instances for requests.</param>
        /// <param name="key">Optional key of the entry.</param>
        public bool TryAddSingleton<TInjection>(InjectionFactoryEntry.FactoryDelegate<TInjection> factory,
            object? key = null)
            => container.TryAddInjection(InjectionLifespan.Singleton, factory, key);
    }
}