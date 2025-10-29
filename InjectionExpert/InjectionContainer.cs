using System.Collections;
using System.Runtime.InteropServices;
using InjectionExpert.Entries;
using InjectionExpert.Utilities.Internal;
using Microsoft.Extensions.Logging;

namespace InjectionExpert;

public class InjectionContainer : IInjectionContainer
{
    private readonly Dictionary<Type, EntryGroup> _groups = [];

    [Injection] public ILogger<InjectionContainer>? Logger { protected get; init; }

    public InjectionEntry? GetInjectionEntry(Type type, object? key)
    {
        return !_groups.TryGetValue(type, out var group)
            ? (key is null ? group.UnkeyedItem : group.KeyedItems?.GetValueOrDefault(key))?.Entry
            : null;
    }

    public void AddInjectionEntry(Type type, object? key, InjectionEntry entry)
    {
        ref var group = ref CollectionsMarshal.GetValueRefOrAddDefault(_groups, type, out _);
        if (key is null)
        {
            if (group.UnkeyedItem is null)
            {
                group.UnkeyedItem = new EntryItem(entry);
                if (Logger?.IsEnabled(LogLevel.Information) is true)
                    Logger.LogInformation("Injection Added: ({Type}, Key: {Key}) -> {InjectionEntry}",
                        type, key, entry);
            }
            else
            {
                group.UnkeyedItem.Entry = entry;
                if (Logger?.IsEnabled(LogLevel.Information) is true)
                    Logger.LogInformation("Injection Replaced: ({Type}, Key: {Key}) -> {InjectionEntry}",
                        type, key, entry);
            }

            return;
        }

        group.KeyedItems ??= new Dictionary<object, EntryItem>();
        ref var item = ref CollectionsMarshal.GetValueRefOrAddDefault(
            group.KeyedItems, key, out _);
        if (item is null)
        {
            item = new EntryItem(entry);
            if (Logger?.IsEnabled(LogLevel.Information) is true)
                Logger.LogInformation("Injection Added: ({Type}, Key: {Key}) -> {InjectionEntry}",
                    type, key, entry);
        }
        else
        {
            item.Entry = entry;
            if (Logger?.IsEnabled(LogLevel.Information) is true)
                Logger.LogInformation("Injection Replaced: ({Type}, Key: {Key}) -> {InjectionEntry}",
                    type, key, entry);
        }
    }

    public bool TryAddInjectionEntry(Type type, object? key, InjectionEntry entry)
    {
        ref var group = ref CollectionsMarshal.GetValueRefOrAddDefault(_groups, type, out _);
        if (key is null)
        {
            if (group.UnkeyedItem is not null)
                return false;
            group.UnkeyedItem = new EntryItem(entry);
            if (Logger?.IsEnabled(LogLevel.Information) is true)
                Logger.LogInformation("Injection Added: ({Type}, Key: {Key}) -> {InjectionEntry}",
                    type, key, entry);
            return true;
        }

        group.KeyedItems ??= new Dictionary<object, EntryItem>();
        ref var item = ref CollectionsMarshal.GetValueRefOrAddDefault(
            group.KeyedItems, key, out _);
        if (item is not null)
            return false;
        item = new EntryItem(entry);
        if (Logger?.IsEnabled(LogLevel.Information) is true)
            Logger.LogInformation("Injection Added: ({Type}, Key: {Key}) -> {InjectionEntry}",
                type, key, entry);
        return true;
    }

    public bool RemoveInjectionEntry(Type type, object? key)
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
        if (Logger?.IsEnabled(LogLevel.Information) is true)
            Logger.LogInformation("Injection Removed: ({Type}, Key: {Key})", type, key);
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
            ? new InjectionItem(entry.GetInjection(type, null, target), entry.Lifespan)
            : null;
    }

    public IInjectionProvider.IScope NewScope(InjectionTarget target = default)
        => InjectionScope.New(this, null, target);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerator<(Type Type, object? Key, InjectionEntry Entry)> GetEnumerator()
    {
        foreach (var (type, group) in _groups)
        {
            if (group.UnkeyedItem is { } unkeyedItem)
                yield return (type, null, unkeyedItem.Entry);
            if (group.KeyedItems is null)
                continue;
            foreach (var (key, keyedItem) in group.KeyedItems)
                yield return (type, key, keyedItem.Entry);
        }
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

    private const int MaxRedirectionDepth = 1000;

    private InjectionEntry? SearchEntry(Type type, object? key)
    {
        return InternalSearchItem(type, key, 0)?.Entry;

        EntryItem? InternalSearchItem(Type currentType, object? currentKey, int currentRedirections)
        {
            while (true)
            {
                var currentItem = _groups.TryGetValue(currentType, out var group)
                    ? currentKey is null ? group.UnkeyedItem : group.KeyedItems?.GetValueOrDefault(currentKey)
                    : null;
                switch (currentItem?.Entry)
                {
                    case InjectionRedirectionEntry redirection:
                        if (currentItem.CachedRedirection is { } cachedItem)
                            return cachedItem;
                        if (currentRedirections >= MaxRedirectionDepth)
                        {
                            if (Logger?.IsEnabled(LogLevel.Warning) is true)
                                Logger.LogWarning(
                                    "Maximum redirection depth reached when searching for injection: " +
                                    "{Type} (Key: {Key}). Possible circular redirection detected.",
                                    type, key);
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