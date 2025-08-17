using System.Collections.Concurrent;
using InjectionExpert.Utilities.Internal;

namespace InjectionExpert;

public partial class InjectionContainer : IInjectionContainer
{
    private readonly ConcurrentKeyedDictionary<Type, object, InjectionEntry> _entries = new();
    
    private readonly ConcurrentDictionary<string, FactoryEntry> _factories = new();
    
    private InjectionEntry? SearchEntry(Type type, object? key)
    {
        var entry = _entries.GetValueOrDefault(type, key ?? NullKey.Value);
        return entry switch
        {
            RedirectionEntry redirection => SearchEntry(redirection.TargetType, redirection.TargetKey),
            null when type is { IsGenericType: true, IsGenericTypeDefinition: false } => 
                SearchEntry(type.EraseDeepestGenericArguments(), key),
            _ => entry
        };
    }

    public void AddFactory(string name, IInjectionContainer.FactoryDelegate factory)
    {
        _factories[name] = new FactoryEntry
        {
            Container = this,
            Factory = factory
        };
    }

    public bool RemoveFactory(string name)
    {
        return _factories.Remove(name, out _);
    }

    /// <summary>
    /// Get an injection from this container.
    /// </summary>
    /// <param name="type">Type of the injection.</param>
    /// <param name="key">The key for the requested injection.</param>
    /// <param name="target">This parameter is ignored.</param>
    /// <returns>Injection resource, or null if not found.</returns>
    public (object Injection, InjectionLifespan Lifespan)? GetInjection(
        Type type, object? key, InjectionTarget target)
    {
        var entry = SearchEntry(type, key);
        if (entry != null) 
            return (entry.GetValue(type, target), entry.Lifespan);
        foreach (var (_, factory) in _factories)
        {
            var item = factory.Get(type, key, target);
            if (item != null)
                return item;
        }
        return null;
    }

    public IInjectionProvider.IScope NewScope(InjectionTarget target = default)
        => InjectionScope.New(this, null, target);

    public void AddInjection(Type type, Type implementation, InjectionLifespan lifespan, object? key = null)
    {
        key ??= NullKey.Value;

        if (implementation.IsGenericTypeDefinition)
        {
            Type? matchedCategory;

            if (type.IsInterface)
                implementation.TryMatchInterface(type, out matchedCategory);
            else 
                implementation.TryMatchGenericBaseType(type, out matchedCategory);
            
            if (matchedCategory == null)
                throw new ArgumentException(
                    $"Cannot add injection: implementation type " +
                    $"'{implementation}' is not be assignable to category type '{type}'.");
            
            _entries.SetValue(type, key, new TypeDefinitionInjectionEntry(implementation, matchedCategory)
            {
                Container = this,
                Lifespan = lifespan,
            });
        }
        else
        {
            if (!implementation.IsAssignableTo(type))
                throw new ArgumentException(
                    $"Cannot add injection: implementation type " +
                    $"'{implementation}' is not be assignable to type '{type}'.");
            
            _entries.SetValue(type, key, new TypeInjectionEntry(implementation)
            {
                Container = this,
                Lifespan = lifespan,
            });
        }
    }

    public void AddInjection(Type type, Func<IInjectionProvider, InjectionTarget, object> factory,
        InjectionLifespan lifespan,
        object? key = null)
        => _entries.SetValue(type, key ?? NullKey.Value, new FactoryInjectionEntry(factory)
        {
            Container = this,
            Lifespan = lifespan
        });

    public void AddInjection(Type type, object value, object? key = null)
        => _entries.SetValue(type, key ?? NullKey.Value, new ConstantInjectionEntry(value)
        {
            Container = this,
            Lifespan = InjectionLifespan.Transient
        });

    public void AddRedirection(Type fromType, object? fromKey, Type toType, object? toKey)
        => _entries.SetValue(fromType, fromKey ?? NullKey.Value, new RedirectionEntry(toType, toKey)
        {
            Container = this,
            Lifespan = InjectionLifespan.Transient
        });

    public bool RemoveInjection(Type type, object? key = null)
    {
        return _entries.Remove(type, key ?? NullKey.Value);
    }

    /// <summary>
    /// The default key for injections when optional key is null.
    /// </summary>
    public sealed record NullKey
    {
        public static readonly NullKey Value = new();

        private NullKey()
        {
        }
    }
}