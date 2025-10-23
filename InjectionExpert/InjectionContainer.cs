using System.Collections;
using InjectionExpert.Entries;
using InjectionExpert.Utilities.Internal;
using Microsoft.Extensions.Logging;

namespace InjectionExpert;

public class InjectionContainer : IInjectionContainer
{
    private readonly ConcurrentKeyedDictionary<Type, object, InjectionEntry> _entries = new();

    [Injection] public ILogger<InjectionContainer>? Logger { get; init; }

    /// <summary>
    /// Get an injection from this container.
    /// </summary>
    /// <param name="type">Type of the injection.</param>
    /// <param name="key">The key for the requested injection.</param>
    /// <param name="target">This parameter is ignored.</param>
    /// <returns>Injection resource, or null if not found.</returns>
    public InjectionItem? GetInjectionItem(Type type, object? key, InjectionTarget target)
    {
        var entry = SearchEntry(type, key);
        return entry != null
            ? new InjectionItem(entry.GetInjection(type, target), entry.Lifespan)
            : null;
    }

    public IInjectionProvider.IScope NewScope(InjectionTarget target = default)
        => InjectionScope.New(this, null, target);

    public void AddInjection(InjectionLifespan lifespan, Type type, Type implementation, object? key = null)
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
                new InjectionTypeDefinitionEntry(this, lifespan, genericCategory, implementation));
            Logger?.LogInformation(
                "Injection Added - Generic: {Type} -> {Implementation} (Lifespan: {Lifespan}, Key: {Key})",
                type, implementation, lifespan, key);
        }
        else
        {
            if (!implementation.IsAssignableTo(type))
                throw new ArgumentException(
                    $"Cannot add injection: implementation type " +
                    $"'{implementation}' is not be assignable to type '{type}'.");

            _entries.SetValue(type, key, new InjectionTypeEntry(this, lifespan, implementation));
            Logger?.LogInformation(
                "Injection Added: {Type} -> {Implementation} (Lifespan: {Lifespan}, Key: {Key})",
                type, implementation, lifespan, key);
        }
    }

    public void AddInjection(InjectionLifespan lifespan,
        Type type, IInjectionContainer.FactoryDelegate factory,
        object? key = null)
    {
        Logger?.LogInformation(
            "Injection Added - Factory: {Type} -> {Factory} (Lifespan: {Lifespan}, Key: {Key})",
            type, factory.Method.DeclaringType, lifespan, key);
        _entries.SetValue(type, key ?? NullKey.Value, new InjectionFactoryEntry(this, lifespan, factory));
    }

    public void AddInjection(Type type, object value, object? key = null)
    {
        Logger?.LogInformation(
            "Injection Added - Constant: {Type} -> {Value} (Key: {Key})",
            type, value, key);
        _entries.SetValue(type, key ?? NullKey.Value, new InjectionConstantEntry(value));
    }

    public void AddRedirection(Type fromType, object? fromKey, Type toType, object? toKey)
    {
        Logger?.LogInformation(
            "Injection Added - Redirection: ({FromType}, Key: {Key}) -> ({ToType}, Key: {ToKey})",
            fromType, fromKey, toType, toKey);
        _entries.SetValue(fromType, fromKey ?? NullKey.Value, new InjectionRedirectionEntry(this, toType, toKey));
    }

    public bool RemoveInjection(Type type, object? key = null)
    {
        if (!_entries.Remove(type, key ?? NullKey.Value))
            return false;
        Logger?.LogInformation("Injection Removed: {Type} (Key: {Key})", type, key);
        return true;
    }

    public void Clear()
    {
        _entries.Clear();
        Logger?.LogInformation("All entries are removed.");
    }

    public void InvalidateCache()
    {
        foreach (var (_, _, entry) in _entries)
            entry.InvalidateCache();
        Logger?.LogInformation("All cached are removed.");
    }

    public IEnumerator<(Type Type, object? Key, InjectionEntry Entry)> GetEnumerator()
        => _entries
            .Select(pair =>
                (pair.PrimaryKey, pair.SecondaryKey is NullKey ? null : pair.SecondaryKey, pair.Value))
            .GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private InjectionEntry? SearchEntry(Type type, object? key)
    {
        var entry = _entries.GetValueOrDefault(type, key ?? NullKey.Value);
        return entry switch
        {
            InjectionRedirectionEntry redirection =>
                // Redirection entries are handled here to optimize the lookup performance.
                SearchEntry(redirection.TargetType, redirection.TargetKey),
            null when type is { IsGenericType: true, IsGenericTypeDefinition: false } =>
                // Try to increase searching granularity by consider nested generic arguments as a generic arguments.
                SearchEntry(type.EraseDeepestGenericArguments(), key),
            _ => entry
        };
    }

    /// <summary>
    /// The default key for injections when optional key is null.
    /// </summary>
    public sealed class NullKey
    {
        public static readonly NullKey Value = new();

        private NullKey()
        {
        }

        public override string ToString() => "<Null>";
    }
}