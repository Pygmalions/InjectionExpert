using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using InjectionExpert.Entries;
using InjectionExpert.Utilities.Internal;
using Microsoft.Extensions.Logging;

namespace InjectionExpert;

public class InjectionContainer : IInjectionContainer
{
    private readonly Dictionary<Type, EntryGroup> _groups = [];

    [Injection] public ILogger<InjectionContainer>? Logger { protected get; init; }

    /// <summary>
    /// Set the entry for the specified type and key.
    /// </summary>
    /// <returns>
    /// True if a previous entry is overwritten; false if a new entry is added.
    /// </returns>
    private bool SetEntryItem(Type type, object? key, InjectionEntry entry)
    {
        ref var group = ref CollectionsMarshal.GetValueRefOrAddDefault(_groups, type, out _);
        if (key is null)
        {
            if (group.UnkeyedItem is null)
            {
                group.UnkeyedItem = new EntryItem(entry);
                return false;
            }

            group.UnkeyedItem.Entry = entry;
            return true;
        }

        group.KeyedItems ??= new Dictionary<object, EntryItem>();
        ref var item = ref CollectionsMarshal.GetValueRefOrAddDefault(
            group.KeyedItems, key, out _);
        if (item is null)
        {
            item = new EntryItem(entry);
            return false;
        }

        item.Entry = entry;
        return true;
    }

    private EntryItem? GetEntryItem(Type type, object? key)
    {
        if (!_groups.TryGetValue(type, out var group))
            return null;
        return key is null ? group.UnkeyedItem : group.KeyedItems?.GetValueOrDefault(key);
    }

    private bool RemoveEntryItem(Type type, object? key)
    {
        if (!_groups.TryGetValue(type, out var group))
            return false;
        if (key is not null)
        {
            if (group.KeyedItems?.Remove(key, out var keyedItem) is not true) 
                return false;
            keyedItem.IsValid = false;
            return true;

        }
        if (group.UnkeyedItem == null)
            return false;
        group.UnkeyedItem.IsValid = false;
        group.UnkeyedItem = null;
        return true;
    }

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

            var overwritten = SetEntryItem(type, key,
                new InjectionTypeDefinitionEntry(this, lifespan, genericCategory, implementation));
            if (Logger?.IsEnabled(LogLevel.Information) is true)
                Logger.LogInformation(
                    "Injection Added - Generic: {Type} -> {Implementation} " +
                    "(Lifespan: {Lifespan}, Key: {Key}, Overwritten: {Overwritten})",
                    type, implementation, lifespan, key, overwritten);
        }
        else
        {
            if (!implementation.IsAssignableTo(type))
                throw new ArgumentException(
                    $"Cannot add injection: implementation type " +
                    $"'{implementation}' is not be assignable to type '{type}'.");

            var overwritten = SetEntryItem(type, key,
                new InjectionTypeEntry(this, lifespan, implementation));
            if (Logger?.IsEnabled(LogLevel.Information) is true)
                Logger.LogInformation(
                    "Injection Added: {Type} -> {Implementation} " +
                    "(Lifespan: {Lifespan}, Key: {Key}, Overwritten: {Overwritten})",
                    type, implementation, lifespan, key, overwritten);
        }
    }

    public void AddInjection(InjectionLifespan lifespan,
        Type type, IInjectionContainer.FactoryDelegate factory,
        object? key = null)
    {
        var overwritten = SetEntryItem(type, key, new InjectionFactoryEntry(this, lifespan, factory));
        if (Logger?.IsEnabled(LogLevel.Information) is true)
            Logger.LogInformation(
                "Injection Added - Factory: {Type} -> {Factory} " +
                "(Lifespan: {Lifespan}, Key: {Key}, Overwritten: {Overwritten})",
                type, factory.Method, lifespan, key, overwritten);
    }

    public void AddInjection(Type type, object value, object? key = null)
    {
        var overwritten = SetEntryItem(type, key, new InjectionConstantEntry(value));
        if (Logger?.IsEnabled(LogLevel.Information) is true)
            Logger.LogInformation(
                "Injection Added - Constant: {Type} -> {Value} " +
                "(Key: {Key}, Overwritten: {Overwritten})",
                type, value, key, overwritten);
    }

    public void AddRedirection(Type fromType, object? fromKey, Type toType, object? toKey)
    {
        var overwritten = SetEntryItem(fromType, fromKey,
            new InjectionRedirectionEntry(this, toType, toKey));
        if (Logger?.IsEnabled(LogLevel.Information) is true)
            Logger.LogInformation(
                "Injection Added - Redirection: ({FromType}, Key: {Key}) -> ({ToType}, Key: {ToKey}), " +
                "Overwritten: {Overwritten}",
                fromType, fromKey, toType, toKey, overwritten);
    }

    public bool RemoveInjection(Type type, object? key = null)
    {
        var removed = RemoveEntryItem(type, key);
        if (removed && Logger?.IsEnabled(LogLevel.Information) is true)
            Logger.LogInformation("Injection Removed: {Type} (Key: {Key})", type, key);
        return removed;
    }

    public void Clear()
    {
        _groups.Clear();
        Logger?.LogInformation("All entries are removed.");
    }

    public void InvalidateCache()
    {
        foreach (var (_, entry) in _groups)
        {
            entry.UnkeyedItem?.Entry.InvalidateCache();
            if (entry.KeyedItems is null)
                continue;
            foreach (var (_, keyedEntry) in entry.KeyedItems)
                keyedEntry.Entry.InvalidateCache();
        }

        Logger?.LogInformation("All cache are invalidated.");
    }

    public IEnumerator<(Type Type, object? Key, InjectionEntry Entry)> GetEnumerator()
    {
        foreach (var (type, group) in _groups)
        {
            if (group.UnkeyedItem != null)
                yield return (type, null, group.UnkeyedItem.Entry);
            if (group.KeyedItems is null || group.KeyedItems.Count == 0)
                continue;
            foreach (var (key, entry) in group.KeyedItems)
                yield return (type, key, entry.Entry);
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private const int MaxRedirectionDepth = 1000;

    private InjectionEntry? SearchEntry(Type type, object? key)
    {
        return InternalSearchItem(type, key, 0)?.Entry;

        EntryItem? InternalSearchItem(Type currentType, object? currentKey, int currentRedirections)
        {
            while (true)
            {
                var currentItem = GetEntryItem(currentType, currentKey);
                switch (currentItem?.Entry)
                {
                    case InjectionRedirectionEntry redirection:
                        if (currentItem.CachedRedirection is { } cachedItem) 
                            return cachedItem;
                        if (currentRedirections >= MaxRedirectionDepth)
                        {
                            if (Logger?.IsEnabled(LogLevel.Warning) is true)
                            {
                                Logger.LogWarning(
                                    "Maximum redirection depth reached when searching for injection: " + 
                                    "{Type} (Key: {Key}). Possible circular redirection detected.", 
                                    type, key);
                            }

                            return null;
                        }

                        var redirectedItem = InternalSearchItem(redirection.TargetType, redirection.TargetKey, 
                            currentRedirections + 1);
                        currentItem.CachedRedirection = redirectedItem;
                        return redirectedItem;
                    
                    case null when type is { IsGenericType: true, IsGenericTypeDefinition: false }:
                        currentType = type.EraseDeepestGenericArguments();
                        continue;
                    
                    default:
                        return currentItem;
                }
            }
        }
    }

    private struct EntryGroup()
    {
        public EntryItem? UnkeyedItem = null;

        public Dictionary<object, EntryItem>? KeyedItems = null;
    }

    private class EntryItem(InjectionEntry entry)
    {
        public InjectionEntry Entry
        {
            get;
            set
            {
                field = value;
                // Invalidate cached redirection if the entry is changed.
                CachedRedirection = null;
            }
        } = entry;

        public bool IsValid { get; set; } = true;

        public EntryItem? CachedRedirection
        {
            get
            {
                if (field is null)
                    return null;
                if (field.IsValid)
                    return field;
                // Invalidate cached redirection if it is removed.
                field = null;
                return null;
            }
            set;
        }
    }
}