using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace InjectionExpert.Utilities.Internal;

internal class MultiDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, IReadOnlySet<TValue>>>
    where TKey : notnull where TValue : notnull
{
    private readonly Dictionary<TKey, HashSet<TValue>> _dictionary = new();

    public IReadOnlyCollection<TKey> Keys => _dictionary.Keys;

    public HashSet<TValue>? this[TKey key]
    {
        get => _dictionary.GetValueOrDefault(key);
        set
        {
            if (value != null)
                _dictionary[key] = value;
            else
                _dictionary.Remove(key, out _);
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerator<KeyValuePair<TKey, IReadOnlySet<TValue>>> GetEnumerator()
        => _dictionary
            .Select(pair => new KeyValuePair<TKey, IReadOnlySet<TValue>>(pair.Key, pair.Value))
            .GetEnumerator();

    /// <summary>
    /// Adds a value to the set associated with the specified key.
    /// </summary>
    /// <param name="key">Key of the set to add the value to.</param>
    /// <param name="value">Value to add.</param>
    /// <returns>True if the value is added, false if the same value already exists.</returns>
    public bool Add(TKey key, TValue value)
    {
        if (_dictionary.TryGetValue(key, out var set))
            return set.Add(value);
        set = [];
        _dictionary[key] = set;
        return set.Add(value);
    }

    /// <summary>
    /// Remove a value from the set associated with the specified key.
    /// </summary>
    /// <param name="key">Key of the set to remove the value from.</param>
    /// <param name="value">Value to remove.</param>
    /// <returns>True if the value is removed, false if the value is not found.</returns>
    public bool Remove(TKey key, TValue value)
        => _dictionary.TryGetValue(key, out var set) && set.Remove(value);

    /// <summary>
    /// Remove the set associated with the specified key.
    /// </summary>
    /// <param name="key">Key of the set to remove.</param>
    /// <returns>
    /// True if the set associated with the specified key is removed,
    /// false if the set is not found.
    /// </returns>
    public bool Remove(TKey key)
        => _dictionary.Remove(key, out _);

    /// <summary>
    /// Check if the specified value exists in the set associated with the specified key.
    /// </summary>
    /// <param name="key">Key of the set to check the value with.</param>
    /// <param name="value">Value to check.</param>
    /// <returns>True if the value exists in the set associated with the specified key.</returns>
    public bool Contains(TKey key, TValue value)
        => _dictionary.TryGetValue(key, out var set) && set.Contains(value);

    /// <summary>
    /// Check if the specified value exists in any set.
    /// </summary>
    /// <param name="value">Value to check.</param>
    /// <returns>True if the specified value exists in any set.</returns>
    public bool Contains(TValue value)
        => _dictionary.Values.Any(set => set.Contains(value));

    /// <summary>
    /// Check if this dictionary contains the specified key.
    /// </summary>
    /// <param name="key">Key to check.</param>
    /// <returns>True if the set associated with the specified value exists.</returns>
    public bool ContainsKey(TKey key)
        => _dictionary.ContainsKey(key);

    /// <summary>
    /// Clear the whole dictionary.
    /// </summary>
    public void Clear()
        => _dictionary.Clear();

    /// <summary>
    /// Clear the set associated with the specified key.
    /// </summary>
    /// <param name="key">Key of the set to clear.</param>
    public bool Clear(TKey key)
        => _dictionary.Remove(key, out _);

    public bool TryGetValues(TKey key, [NotNullWhen(true)] out HashSet<TValue>? set)
        => _dictionary.TryGetValue(key, out set);
}