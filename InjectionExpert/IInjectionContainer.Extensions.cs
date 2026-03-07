using InjectionExpert.Entries;

namespace InjectionExpert;

public static class InjectionContainerExtensions
{
    extension(IInjectionContainer container)
    {
        /// <summary>
        /// Remove the injection entry for the specified type and key.
        /// </summary>
        /// <typeparam name="TInjection">Type that the entry is associated with.</typeparam>
        /// <param name="key">Optional key of the entry.</param>
        /// <returns>True if the entry is removed, or false if the entry is not found.</returns>
        public bool RemoveEntry<TInjection>(object? key = null)
            => container.RemoveEntry(typeof(TInjection), key);
    }
    
    extension(IInjectionContainer container)
    {
        /// <summary>
        /// Add the specified implementation type to this container.
        /// </summary>
        /// <param name="lifespan">Lifespan of this injection.</param>
        /// <param name="type">Type that the entry is associated with.</param>
        /// <param name="implementation">Type to instantiate for requests.</param>
        /// <param name="key">Optional key of the entry.</param>
        private IInjectionContainer AddTypeEntry(InjectionLifespan lifespan,
            Type type, Type implementation, object? key = null)
        {
            if (!type.ContainsGenericParameters)
                container.AddEntry(type, key,
                    new InjectionTypeEntry(lifespan, implementation));
            else
                container.AddEntry(type, key,
                    new InjectionGenericEntry(lifespan, implementation));
            return container;
        }
        
        /// <summary>
        /// Add the specified implementation type to this container.
        /// </summary>
        /// <param name="lifespan">Lifespan of this injection.</param>
        /// <param name="type">Type that the entry is associated with.</param>
        /// <param name="implementation">Type to instantiate for requests.</param>
        /// <param name="key">Optional key of the entry.</param>
        private bool TryAddTypeEntry(InjectionLifespan lifespan,
            Type type, Type implementation, object? key = null)
        {
            if (!type.IsGenericTypeDefinition)
                return container.TryAddEntry(type, key,
                    new InjectionTypeEntry(lifespan, implementation));

            return container.TryAddEntry(type, key,
                new InjectionGenericEntry(lifespan, implementation));
        }

        /// <summary>
        /// Add the specified implementation type as a transient injection to this container.
        /// </summary>
        /// <param name="type">Type that the entry is associated with.</param>
        /// <param name="implementation">Type to instantiate for requests.</param>
        /// <param name="key">Optional key of the entry.</param>
        public IInjectionContainer AddTransient(Type type, Type implementation, object? key = null)
            => container.AddTypeEntry(InjectionLifespan.Transient, type, implementation, key);

        /// <summary>
        /// Add the specified implementation type as a transient injection to this container.
        /// </summary>
        /// <typeparam name="TImplementation">
        /// Type to instantiate for requests. The injection is also added under this type.
        /// </typeparam>
        /// <param name="key">Optional key of the entry.</param>
        public IInjectionContainer AddTransient<TImplementation>(object? key = null)
            => container.AddTypeEntry(InjectionLifespan.Transient,
                typeof(TImplementation), typeof(TImplementation), key);

        /// <summary>
        /// Add the specified implementation type as a transient injection to this container.
        /// </summary>
        /// <typeparam name="TCategory">Category type that this injection is associated with.</typeparam>
        /// <typeparam name="TImplementation">Type to instantiate for requests. 
        /// </typeparam>
        /// <param name="key">Optional key of the entry.</param>
        public IInjectionContainer AddTransient<TCategory, TImplementation>(object? key = null)
            => container.AddTypeEntry(InjectionLifespan.Transient,
                typeof(TCategory), typeof(TImplementation), key);

        /// <summary>
        /// Add the specified implementation type as a scoped injection to this container.
        /// </summary>
        /// <param name="type">Type that the entry is associated with.</param>
        /// <param name="implementation">Type to instantiate for requests.</param>
        /// <param name="key">Optional key of the entry.</param>
        public IInjectionContainer AddScoped(Type type, Type implementation, object? key = null)
            => container.AddTypeEntry(InjectionLifespan.Scoped, type, implementation, key);

        /// <summary>
        /// Add the specified implementation type as a scoped injection to this container.
        /// </summary>
        /// <typeparam name="TImplementation">
        /// Type to instantiate for requests. The injection is also added under this type.
        /// </typeparam>
        /// <param name="key">Optional key of the entry.</param>
        public IInjectionContainer AddScoped<TImplementation>(object? key = null)
            => container.AddTypeEntry(InjectionLifespan.Scoped,
                typeof(TImplementation), typeof(TImplementation), key);

        /// <summary>
        /// Add the specified implementation type as a scoped injection to this container.
        /// </summary>
        /// <typeparam name="TCategory">Category type that this injection is associated with.</typeparam>
        /// <typeparam name="TImplementation">Type to instantiate for requests.</typeparam>
        /// <param name="key">Optional key of the entry.</param>
        public IInjectionContainer AddScoped<TCategory, TImplementation>(object? key = null)
            => container.AddTypeEntry(InjectionLifespan.Scoped,
                typeof(TCategory), typeof(TImplementation), key);

        /// <summary>
        /// Add the specified implementation type as a singleton injection to this container.
        /// </summary>
        /// <param name="type">Type that the entry is associated with.</param>
        /// <param name="implementation">Type to instantiate for requests.</param>
        /// <param name="key">Optional key of the entry.</param>
        public IInjectionContainer AddSingleton(Type type, Type implementation, object? key = null)
            => container.AddTypeEntry(InjectionLifespan.Singleton, type, implementation, key);

        /// <summary>
        /// Add the specified implementation type as a singleton injection to this container.
        /// </summary>
        /// <typeparam name="TImplementation">
        /// Type to instantiate for requests. The injection is also added under this type.
        /// </typeparam>
        /// <param name="key">Optional key of the entry.</param>
        public IInjectionContainer AddSingleton<TImplementation>(object? key = null)
            => container.AddTypeEntry(InjectionLifespan.Singleton,
                typeof(TImplementation), typeof(TImplementation), key);

        /// <summary>
        /// Add the specified implementation type as a singleton injection to this container.
        /// </summary>
        /// <typeparam name="TCategory">Category type that this injection is associated with.</typeparam>
        /// <typeparam name="TImplementation">Type to instantiate for requests.</typeparam>
        /// <param name="key">Optional key of the entry.</param>
        public IInjectionContainer AddSingleton<TCategory, TImplementation>(object? key = null)
            => container.AddTypeEntry(InjectionLifespan.Singleton,
                typeof(TCategory), typeof(TImplementation), key);

        /// <summary>
        /// Add the specified implementation type as a transient injection to this container.
        /// </summary>
        /// <param name="type">Type that the entry is associated with.</param>
        /// <param name="implementation">Type to instantiate for requests.</param>
        /// <param name="key">Optional key of the entry.</param>
        public bool TryAddTransient(Type type, Type implementation, object? key = null)
            => container.TryAddTypeEntry(InjectionLifespan.Transient, type, implementation, key);

        /// <summary>
        /// Add the specified implementation type as a transient injection to this container.
        /// </summary>
        /// <typeparam name="TImplementation">
        /// Type to instantiate for requests. The injection is also added under this type.
        /// </typeparam>
        /// <param name="key">Optional key of the entry.</param>
        public bool TryAddTransient<TImplementation>(object? key = null)
            => container.TryAddTypeEntry(InjectionLifespan.Transient,
                typeof(TImplementation), typeof(TImplementation), key);

        /// <summary>
        /// Add the specified implementation type as a transient injection to this container.
        /// </summary>
        /// <typeparam name="TCategory">Category type that this injection is associated with.</typeparam>
        /// <typeparam name="TImplementation">Type to instantiate for requests.</typeparam>
        /// <param name="key">Optional key of the entry.</param>
        public bool TryAddTransient<TCategory, TImplementation>(object? key = null)
            => container.TryAddTypeEntry(InjectionLifespan.Transient,
                typeof(TCategory), typeof(TImplementation), key);

        /// <summary>
        /// Add the specified implementation type as a scoped injection to this container.
        /// </summary>
        /// <param name="type">Type that the entry is associated with.</param>
        /// <param name="implementation">Type to instantiate for requests.</param>
        /// <param name="key">Optional key of the entry.</param>
        public bool TryAddScoped(Type type, Type implementation, object? key = null)
            => container.TryAddTypeEntry(InjectionLifespan.Scoped, type, implementation, key);

        /// <summary>
        /// Add the specified implementation type as a scoped injection to this container.
        /// </summary>
        /// <typeparam name="TImplementation">
        /// Type to instantiate for requests. The injection is also added under this type.
        /// </typeparam>
        /// <param name="key">Optional key of the entry.</param>
        public bool TryAddScoped<TImplementation>(object? key = null)
            => container.TryAddTypeEntry(InjectionLifespan.Scoped,
                typeof(TImplementation), typeof(TImplementation), key);

        /// <summary>
        /// Add the specified implementation type as a scoped injection to this container.
        /// </summary>
        /// <typeparam name="TCategory">Category type that this injection is associated with.</typeparam>
        /// <typeparam name="TImplementation">Type to instantiate for requests.</typeparam>
        /// <param name="key">Optional key of the entry.</param>
        public bool TryAddScoped<TCategory, TImplementation>(object? key = null)
            => container.TryAddTypeEntry(InjectionLifespan.Scoped,
                typeof(TCategory), typeof(TImplementation), key);

        /// <summary>
        /// Add the specified implementation type as a singleton injection to this container.
        /// </summary>
        /// <param name="type">Type that the entry is associated with.</param>
        /// <param name="implementation">Type to instantiate for requests.</param>
        /// <param name="key">Optional key of the entry.</param>
        public bool TryAddSingleton(Type type, Type implementation, object? key = null)
            => container.TryAddTypeEntry(InjectionLifespan.Singleton, type, implementation, key);

        /// <summary>
        /// Add the specified implementation type as a singleton injection to this container.
        /// </summary>
        /// <typeparam name="TImplementation">
        /// Type to instantiate for requests. The injection is also added under this type.
        /// </typeparam>
        /// <param name="key">Optional key of the entry.</param>
        public bool TryAddSingleton<TImplementation>(object? key = null)
            => container.TryAddTypeEntry(InjectionLifespan.Singleton,
                typeof(TImplementation), typeof(TImplementation), key);

        /// <summary>
        /// Add the specified implementation type as a singleton injection to this container.
        /// </summary>
        /// <typeparam name="TCategory">Category type that this injection is associated with.</typeparam>
        /// <typeparam name="TImplementation">Type to instantiate for requests.</typeparam>
        /// <param name="key">Optional key of the entry.</param>
        public bool TryAddSingleton<TCategory, TImplementation>(object? key = null)
            => container.TryAddTypeEntry(InjectionLifespan.Singleton,
                typeof(TCategory), typeof(TImplementation), key);
    }

    extension(IInjectionContainer container)
    {
        /// <summary>
        /// Add the specified factory to this container.
        /// </summary>
        /// <param name="lifespan">Lifespan of this injection.</param>
        /// <param name="type">Type that the entry is associated with.</param>
        /// <param name="factory">Factory delegate that creates instances for requests.</param>
        /// <param name="key">Optional key of the entry.</param>
        private bool TryAddFactoryEntry(InjectionLifespan lifespan,
            Type type, InjectionFactoryEntry.FactoryDelegate<object> factory, object? key = null)
            => container.TryAddEntry(type, key,
                new InjectionFactoryEntry<object>(lifespan, factory));
        
        /// <summary>
        /// Add the specified factory to this container.
        /// </summary>
        /// <typeparam name="TInjection">Type that the entry is associated with.</typeparam>
        /// <param name="lifespan">Lifespan of this injection.</param>
        /// <param name="factory">Factory delegate that creates instances for requests.</param>
        /// <param name="key">Optional key of the entry.</param>
        private bool TryAddFactoryEntry<TInjection>(InjectionLifespan lifespan,
            InjectionFactoryEntry.FactoryDelegate<TInjection> factory, object? key = null)
            => container.TryAddEntry(typeof(TInjection), key,
                new InjectionFactoryEntry<TInjection>(lifespan, factory));
        
        /// <summary>
        /// Add the specified factory to this container.
        /// </summary>
        /// <param name="lifespan">Lifespan of this injection.</param>
        /// <param name="type">Type that the entry is associated with.</param>
        /// <param name="factory">Factory delegate that creates instances for requests.</param>
        /// <param name="key">Optional key of the entry.</param>
        public IInjectionContainer AddFactoryEntry(InjectionLifespan lifespan,
            Type type, InjectionFactoryEntry.FactoryDelegate<object> factory, object? key = null)
        {
            container.AddEntry(type, key,
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
        public IInjectionContainer AddFactoryEntry<TInjection>(InjectionLifespan lifespan,
            InjectionFactoryEntry.FactoryDelegate<TInjection> factory, object? key = null)
        {
            container.AddEntry(typeof(TInjection), key,
                new InjectionFactoryEntry<TInjection>(lifespan, factory));
            return container;
        }

        /// <summary>
        /// Add the specified factory as a transient injection to this container.
        /// </summary>
        /// <param name="type">Type that the entry is associated with.</param>
        /// <param name="factory">Factory delegate that creates instances for requests.</param>
        /// <param name="key">Optional key of the entry.</param>
        public IInjectionContainer AddTransient(Type type, InjectionFactoryEntry.FactoryDelegate<object> factory,
            object? key = null)
            => container.AddFactoryEntry(InjectionLifespan.Transient, type, factory, key);

        /// <summary>
        /// Add the specified factory as a transient injection to this container.
        /// </summary>
        /// <typeparam name="TInjection">Type that the entry is associated with.</typeparam>
        /// <param name="factory">Factory delegate that creates instances for requests.</param>
        /// <param name="key">Optional key of the entry.</param>
        public IInjectionContainer AddTransient<TInjection>(InjectionFactoryEntry.FactoryDelegate<TInjection> factory,
            object? key = null)
            => container.AddFactoryEntry(InjectionLifespan.Transient, factory, key);

        /// <summary>
        /// Add the specified factory as a scoped injection to this container.
        /// </summary>
        /// <param name="type">Type that the entry is associated with.</param>
        /// <param name="factory">Factory delegate that creates instances for requests.</param>
        /// <param name="key">Optional key of the entry.</param>
        public IInjectionContainer AddScoped(Type type, InjectionFactoryEntry.FactoryDelegate<object> factory,
            object? key = null)
            => container.AddFactoryEntry(InjectionLifespan.Scoped, type, factory, key);

        /// <summary>
        /// Add the specified factory as a scoped injection to this container.
        /// </summary>
        /// <typeparam name="TInjection">Type that the entry is associated with.</typeparam>
        /// <param name="factory">Factory delegate that creates instances for requests.</param>
        /// <param name="key">Optional key of the entry.</param>
        public IInjectionContainer AddScoped<TInjection>(InjectionFactoryEntry.FactoryDelegate<TInjection> factory,
            object? key = null)
            => container.AddFactoryEntry(InjectionLifespan.Scoped, factory, key);

        /// <summary>
        /// Add the specified factory as a singleton injection to this container.
        /// </summary>
        /// <param name="type">Type that the entry is associated with.</param>
        /// <param name="factory">Factory delegate that creates instances for requests.</param>
        /// <param name="key">Optional key of the entry.</param>
        public IInjectionContainer AddSingleton(Type type, InjectionFactoryEntry.FactoryDelegate<object> factory,
            object? key = null)
            => container.AddFactoryEntry(InjectionLifespan.Singleton, type, factory, key);

        /// <summary>
        /// Add the specified factory as a singleton injection to this container.
        /// </summary>
        /// <typeparam name="TInjection">Type that the entry is associated with.</typeparam>
        /// <param name="factory">Factory delegate that creates instances for requests.</param>
        /// <param name="key">Optional key of the entry.</param>
        public IInjectionContainer AddSingleton<TInjection>(InjectionFactoryEntry.FactoryDelegate<TInjection> factory,
            object? key = null)
            => container.AddFactoryEntry(InjectionLifespan.Singleton, factory, key);

        /// <summary>
        /// Add the specified factory as a transient injection to this container.
        /// </summary>
        /// <param name="type">Type that the entry is associated with.</param>
        /// <param name="factory">Factory delegate that creates instances for requests.</param>
        /// <param name="key">Optional key of the entry.</param>
        public bool TryAddTransient(Type type, InjectionFactoryEntry.FactoryDelegate<object> factory,
            object? key = null)
            => container.TryAddFactoryEntry(InjectionLifespan.Transient, type, factory, key);

        /// <summary>
        /// Add the specified factory as a transient injection to this container.
        /// </summary>
        /// <typeparam name="TInjection">Type that the entry is associated with.</typeparam>
        /// <param name="factory">Factory delegate that creates instances for requests.</param>
        /// <param name="key">Optional key of the entry.</param>
        public bool TryAddTransient<TInjection>(InjectionFactoryEntry.FactoryDelegate<TInjection> factory,
            object? key = null)
            => container.TryAddFactoryEntry(InjectionLifespan.Transient, factory, key);

        /// <summary>
        /// Add the specified factory as a scoped injection to this container.
        /// </summary>
        /// <param name="type">Type that the entry is associated with.</param>
        /// <param name="factory">Factory delegate that creates instances for requests.</param>
        /// <param name="key">Optional key of the entry.</param>
        public bool TryAddScoped(Type type, InjectionFactoryEntry.FactoryDelegate<object> factory,
            object? key = null)
            => container.TryAddFactoryEntry(InjectionLifespan.Scoped, type, factory, key);

        /// <summary>
        /// Add the specified factory as a scoped injection to this container.
        /// </summary>
        /// <typeparam name="TInjection">Type that the entry is associated with.</typeparam>
        /// <param name="factory">Factory delegate that creates instances for requests.</param>
        /// <param name="key">Optional key of the entry.</param>
        public bool TryAddScoped<TInjection>(InjectionFactoryEntry.FactoryDelegate<TInjection> factory,
            object? key = null)
            => container.TryAddFactoryEntry(InjectionLifespan.Scoped, factory, key);

        /// <summary>
        /// Add the specified factory as a singleton injection to this container.
        /// </summary>
        /// <param name="type">Type that the entry is associated with.</param>
        /// <param name="factory">Factory delegate that creates instances for requests.</param>
        /// <param name="key">Optional key of the entry.</param>
        public bool TryAddSingleton(Type type, InjectionFactoryEntry.FactoryDelegate<object> factory,
            object? key = null)
            => container.TryAddFactoryEntry(InjectionLifespan.Singleton, type, factory, key);

        /// <summary>
        /// Add the specified factory as a singleton injection to this container.
        /// </summary>
        /// <typeparam name="TInjection">Type that the entry is associated with.</typeparam>
        /// <param name="factory">Factory delegate that creates instances for requests.</param>
        /// <param name="key">Optional key of the entry.</param>
        public bool TryAddSingleton<TInjection>(InjectionFactoryEntry.FactoryDelegate<TInjection> factory,
            object? key = null)
            => container.TryAddFactoryEntry(InjectionLifespan.Singleton, factory, key);
    }

    extension(IInjectionContainer container)
    {
        /// <summary>
        /// Add the specified instance as a constant injection to this container.
        /// </summary>
        /// <param name="type">Type that the entry is associated with.</param>
        /// <param name="instance">Instance to add.</param>
        /// <param name="key">
        /// Optional key of the entry.
        /// This parameter does not provide a default null
        /// to differentiate from another method where the second parameter is the key.
        /// </param>
        public IInjectionContainer AddSingleton(Type type, object instance, object key)
        {
            container.AddEntry(type, key, new InjectionConstantEntry(instance));
            return container;
        }

        /// <summary>
        /// Add the specified instance as a constant injection to this container.
        /// </summary>
        /// <typeparam name="TInjection">Type that the entry is associated with.</typeparam>
        /// <param name="instance">Instance to add.</param>
        /// <param name="key">Optional key of the entry.</param>
        public IInjectionContainer AddSingleton<TInjection>(TInjection instance, object? key = null)
        {
            container.AddEntry(typeof(TInjection), key, new InjectionConstantEntry(instance!));
            return container;
        }

        /// <summary>
        /// Add the specified instance as a constant injection to this container.
        /// </summary>
        /// <param name="type">Type that the entry is associated with.</param>
        /// <param name="instance">Instance to add.</param>
        /// <param name="key">Optional key of the entry.</param>
        public bool TryAddSingleton(Type type, object instance, object? key = null)
            => container.TryAddEntry(type, key, new InjectionConstantEntry(instance));

        /// <summary>
        /// Add the specified instance as a constant injection to this container.
        /// </summary>
        /// <param name="type">Type that the entry is associated with.</param>
        /// <param name="instance">Instance to add.</param>
        /// <param name="key">Optional key of the entry.</param>
        public bool TryAddSingleton<TInjection>(Type type, TInjection instance, object? key = null)
            => container.TryAddEntry(type, key, new InjectionConstantEntry(instance!));
    }
}