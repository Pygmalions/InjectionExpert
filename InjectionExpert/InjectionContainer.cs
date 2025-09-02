using InjectionExpert.Utilities.Internal;

namespace InjectionExpert;

public partial class InjectionContainer : IInjectionContainer
{
    private readonly ConcurrentKeyedDictionary<Type, object, InjectionEntry> _entries = new();

    private InjectionEntry? SearchEntry(Type type, object? key)
    {
        var entry = _entries.GetValueOrDefault(type, key ?? NullKey.Value);
        return entry switch
        {
            RedirectionEntry redirection =>
                // Redirection entries are handled here to optimize the lookup performance.
                SearchEntry(redirection.TargetType, redirection.TargetKey),
            null when type is { IsGenericType: true, IsGenericTypeDefinition: false } =>
                // Try to increase searching granularity by consider nested generic arguments as a generic arguments.
                SearchEntry(type.EraseDeepestGenericArguments(), key),
            _ => entry
        };
    }

    /// <summary>
    /// Get an injection from this container.
    /// </summary>
    /// <param name="type">Type of the injection.</param>
    /// <param name="key">The key for the requested injection.</param>
    /// <param name="target">This parameter is ignored.</param>
    /// <returns>Injection resource, or null if not found.</returns>
    public InjectionItem? GetInjection(Type type, object? key, InjectionTarget target)
    {
        var entry = SearchEntry(type, key);
        return entry != null
            ? new InjectionItem(entry.GetValue(type, target), entry.Lifespan)
            : null;
    }

    public IInjectionProvider.IScope NewScope(InjectionTarget target = default)
        => InjectionScope.New(this, null, target);

    public void AddInjection(Type type, Type implementation, InjectionLifespan lifespan, object? key = null)
    {
        key ??= NullKey.Value;

        if (implementation.IsGenericTypeDefinition)
        {
            Type? genericCategory;

            if (type.IsInterface)
                implementation.TryMatchInterface(type, out genericCategory);
            else
                implementation.TryMatchGenericBaseType(type, out genericCategory);

            if (genericCategory == null)
                throw new ArgumentException(
                    $"Cannot add injection: implementation type " +
                    $"'{implementation}' is not be assignable to category type '{type}'.");

            _entries.SetValue(type, key,
                new TypeDefinitionEntry(this, lifespan, genericCategory, implementation));
        }
        else
        {
            if (!implementation.IsAssignableTo(type))
                throw new ArgumentException(
                    $"Cannot add injection: implementation type " +
                    $"'{implementation}' is not be assignable to type '{type}'.");

            _entries.SetValue(type, key, new TypeEntry(this, lifespan, implementation));
        }
    }

    public void AddInjection(Type type, Func<IInjectionProvider, InjectionTarget, object> factory,
        InjectionLifespan lifespan,
        object? key = null)
        => _entries.SetValue(type, key ?? NullKey.Value, new FactoryEntry(this, lifespan, factory));

    public void AddInjection(Type type, object value, object? key = null)
        => _entries.SetValue(type, key ?? NullKey.Value, new ConstantEntry(value));

    public void AddRedirection(Type fromType, object? fromKey, Type toType, object? toKey)
        => _entries.SetValue(fromType, fromKey ?? NullKey.Value, new RedirectionEntry(this, toType, toKey));

    public bool RemoveInjection(Type type, object? key = null)
        => _entries.Remove(type, key ?? NullKey.Value);

    /// <summary>
    /// The default key for injections when optional key is null.
    /// </summary>
    public sealed class NullKey
    {
        public static readonly NullKey Value = new();

        private NullKey()
        {
        }
    }
}