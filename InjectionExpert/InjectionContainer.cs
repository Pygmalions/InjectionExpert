using System.Collections;
using System.Collections.Concurrent;
using InjectionExpert.Entries;
using InjectionExpert.Utilities.Internal;
using Microsoft.Extensions.Logging;

namespace InjectionExpert;

public class InjectionContainer : IInjectionContainer
{
    private readonly ConcurrentKeyedDictionary<Type, object, InjectionEntry> _keyedEntries = new();

    private readonly ConcurrentDictionary<Type, InjectionEntry> _unkeyedEntries = [];

    [Injection] public ILogger<InjectionContainer>? Logger { protected get; init; }

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

            var entry = new InjectionTypeDefinitionEntry(this, lifespan, genericCategory, implementation);

            if (key is null)
                _unkeyedEntries[type] = entry;
            else
                _keyedEntries.SetValue(type, key, entry);
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

            var entry = new InjectionTypeEntry(this, lifespan, implementation);

            if (key is null)
                _unkeyedEntries[type] = entry;
            else
                _keyedEntries.SetValue(type, key, entry);
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
        var entry = new InjectionFactoryEntry(this, lifespan, factory);
        if (key is null)
            _unkeyedEntries[type] = entry;
        else
            _keyedEntries.SetValue(type, key, entry);
    }

    public void AddInjection(Type type, object value, object? key = null)
    {
        Logger?.LogInformation(
            "Injection Added - Constant: {Type} -> {Value} (Key: {Key})",
            type, value, key);
        var entry = new InjectionConstantEntry(value);
        if (key is null)
            _unkeyedEntries[type] = entry;
        else
            _keyedEntries.SetValue(type, key, entry);
    }

    public void AddRedirection(Type fromType, object? fromKey, Type toType, object? toKey)
    {
        Logger?.LogInformation(
            "Injection Added - Redirection: ({FromType}, Key: {Key}) -> ({ToType}, Key: {ToKey})",
            fromType, fromKey, toType, toKey);
        var entry = new InjectionRedirectionEntry(this, toType, toKey);
        if (fromKey is null)
            _unkeyedEntries[fromType] = entry;
        else
            _keyedEntries.SetValue(fromType, fromKey, entry);
    }

    public bool RemoveInjection(Type type, object? key = null)
    {
        if (key is null)
        {
            if (!_unkeyedEntries.TryRemove(type, out _))
                return false;
        }
        else
        {
            if (!_keyedEntries.Remove(type, key))
                return false;
        }

        Logger?.LogInformation("Injection Removed: {Type} (Key: {Key})", type, key);
        return true;
    }

    public void Clear()
    {
        _unkeyedEntries.Clear();
        _keyedEntries.Clear();
        Logger?.LogInformation("All entries are removed.");
    }

    public void InvalidateCache()
    {
        foreach (var (_, entry) in _unkeyedEntries)
            entry.InvalidateCache();
        foreach (var (_, _, entry) in _keyedEntries)
            entry.InvalidateCache();
        Logger?.LogInformation("All cached are removed.");
    }

    public IEnumerator<(Type Type, object? Key, InjectionEntry Entry)> GetEnumerator()
    {
        foreach (var (type, entry) in _unkeyedEntries)
            yield return (type, null, entry);
        foreach (var (type, key, entry) in _keyedEntries)
            yield return (type, key, entry);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private InjectionEntry? SearchEntry(Type type, object? key)
    {
        var entry = key is null
            ? _unkeyedEntries.GetValueOrDefault(type)
            : _keyedEntries.GetValueOrDefault(type, key);
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
}