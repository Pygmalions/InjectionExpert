using InjectionExpert.Utilities.Internal;

namespace InjectionExpert;

public class InjectionContainer : IInjectionContainer, IAsyncDisposable
{
    private readonly KeyedDictionary<Type, object, InjectionEntry> _concreteEntries = new();

    private readonly KeyedDictionary<Type, object, InjectionEntry> _genericEntries = new();

    /// <summary>
    /// Root scope of the injection container.
    /// </summary>
    private readonly InjectionScope _root;

    private readonly HashSet<IInjectionProvider.InjectionResolver> _resolvers = [];
    
    public IEnumerable<IInjectionProvider.InjectionResolver> Resolvers => _resolvers;
    
    public InjectionContainer()
    {
        _root = new InjectionScope(this);
    }

    public IInjectionScope NewScope() => new InjectionScope(this);

    public InjectionEntry? GetEntry(Type type, object? key = null)
    {
        key ??= IInjectionProvider.NullKey.Instance;
        if (!type.IsGenericTypeDefinition && _concreteEntries.TryGetValue(type, key, out var entry) ||
            type.IsGenericType && _genericEntries.TryGetValue(type.GetGenericTypeDefinition(), key, out entry))
            return entry;
        return null;
    }

    public bool HasEntry(Type type, object? key = null)
    {
        key ??= IInjectionProvider.NullKey.Instance;
        return !type.IsGenericTypeDefinition && _concreteEntries.ContainsKey(type, key) ||
               type.IsGenericType && _genericEntries.ContainsKey(type.GetGenericTypeDefinition(), key);
    }

    public object? GetInjection(Type type, object? key = null, InjectionTarget target = default)
        => _root.GetInjection(type, key, target);

    public IEnumerable<(Type Type, object? Key, InjectionEntry Entry)> Entries
        => _concreteEntries.Concat(_genericEntries)
            .Select(tuple => (tuple.PrimaryKey,
                tuple.SecondaryKey is IInjectionProvider.NullKey ? null : tuple.SecondaryKey,
                tuple.Value));

    public void AddEntry(Type type, object? key, InjectionEntry entry)
    {
        if (!entry.IsAssignableTo(type))
            throw new ArgumentException(
                $"The injection in entry is not assignable to category type '{type}'.", nameof(entry));
        (type.IsGenericTypeDefinition ? _genericEntries : _concreteEntries)
            .SetValue(type, key ?? IInjectionProvider.NullKey.Instance, entry);
    }

    public bool TryAddEntry(Type type, object? key, InjectionEntry entry)
        => (type.IsGenericTypeDefinition ? _genericEntries : _concreteEntries)
            .TrySetValue(type, key ?? IInjectionProvider.NullKey.Instance, entry);

    public bool RemoveEntry(Type type, object? key = null)
        => (type.IsGenericTypeDefinition ? _genericEntries : _concreteEntries)
            .Remove(type, key ?? IInjectionProvider.NullKey.Instance);

    public void ClearEntries()
    {
        _concreteEntries.Clear();
        _genericEntries.Clear();
    }

    public void AddResolver(IInjectionProvider.InjectionResolver resolver)
        => _resolvers.Add(resolver);

    public void RemoveResolver(IInjectionProvider.InjectionResolver resolver)
        => _resolvers.Remove(resolver);

    public void ClearResolvers()
        => _resolvers.Clear();

    public async ValueTask DisposeAsync()
    {
        await _root.DisposeAsync();
    }
}