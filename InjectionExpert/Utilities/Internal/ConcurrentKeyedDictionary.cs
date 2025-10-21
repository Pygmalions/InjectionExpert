using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace InjectionExpert.Utilities.Internal;

internal class ConcurrentKeyedDictionary<TPrimaryKey, TSecondaryKey, TValue> : 
    IEnumerable<(TPrimaryKey PrimaryKey, TSecondaryKey SecondaryKey, TValue Value)>
    where TPrimaryKey : notnull
    where TSecondaryKey : notnull
    where TValue : notnull
{
    private readonly ConcurrentDictionary<TPrimaryKey, ConcurrentDictionary<TSecondaryKey, TValue>> _dictionaries =
        new();

    /// <summary>
    /// Access this dictionary with the specified primary and secondary keys.
    /// </summary>
    /// <param name="keys">Tuple of primary and secondary keys.</param>
    /// <exception cref="KeyNotFoundException">
    /// Thrown if the value with the specified primary and secondary keys is not found.
    /// </exception>
    public TValue this[(TPrimaryKey Primary, TSecondaryKey Secondary) keys]
    {
        get
        {
            if (_dictionaries.GetValueOrDefault(keys.Primary)?.TryGetValue(keys.Secondary, out var value) == true)
                return value;
            throw new KeyNotFoundException($"Key '{keys}' is not found in the dictionary.");
        }
        set =>
            _dictionaries.GetOrAdd(keys.Primary, _ => new ConcurrentDictionary<TSecondaryKey, TValue>())
                [keys.Secondary] = value;
    }

    /// <summary>
    /// Adds a value to the set associated with the specified key.
    /// </summary>
    /// <param name="primaryKey">Primary key for the value.</param>
    /// <param name="secondaryKey">Secondary key for the value.</param>
    /// <param name="value">Value to add.</param>
    /// <returns>True if the value is added, false if the same value already exists.</returns>
    public void SetValue(TPrimaryKey primaryKey, TSecondaryKey secondaryKey, TValue value)
        => _dictionaries.GetOrAdd(primaryKey, _ => new ConcurrentDictionary<TSecondaryKey, TValue>())
            [secondaryKey] = value;

    /// <summary>
    /// Remove all objects with the specified primary key,
    /// not matter what their secondary key is.
    /// </summary>
    /// <param name="key">Primary key of the values to remove.</param>
    /// <returns>True if any value is removed, false if the value is not found.</returns>
    public bool Remove(TPrimaryKey key)
        => _dictionaries.TryRemove(key, out var collection) && collection.IsEmpty;

    /// <summary>
    /// Remove all objects with the specified primary key,
    /// not matter what their secondary key is.
    /// </summary>
    /// <param name="key">Primary key of the values to remove.</param>
    /// <param name="dictionary">Dictionary of values with the specified primary key.</param>
    /// <returns>True if any value is removed, false if the value is not found.</returns>
    public bool Remove(TPrimaryKey key,
        [NotNullWhen(true)] out ConcurrentDictionary<TSecondaryKey, TValue>? dictionary)
        => _dictionaries.TryRemove(key, out dictionary) && dictionary.IsEmpty;


    /// <summary>
    /// Remove a value with the specified primary and secondary key.
    /// </summary>
    /// <param name="primaryKey">Primary key of the value to remove.</param>
    /// <param name="secondaryKey">Secondary key of the value to remove.</param>
    /// <returns>True if the value is removed, false if the value is not found.</returns>
    public bool Remove(TPrimaryKey primaryKey, TSecondaryKey secondaryKey)
        => _dictionaries.TryGetValue(primaryKey, out var dictionary) && dictionary.Remove(secondaryKey, out _);

    /// <summary>
    /// Remove a value with the specified primary and secondary key.
    /// </summary>
    /// <param name="primaryKey">Primary key of the value to remove.</param>
    /// <param name="secondaryKey">Secondary key of the value to remove.</param>
    /// <param name="value">The removed value the specified primary and secondary keys.</param>
    /// <returns>True if the value is removed, false if the value is not found.</returns>
    public bool Remove(TPrimaryKey primaryKey, TSecondaryKey secondaryKey,
        [NotNullWhen(true)] out TValue? value)
    {
        if (_dictionaries.TryGetValue(primaryKey, out var dictionary) && dictionary.TryRemove(secondaryKey, out value))
            return true;
        value = default;
        return false;
    }

    /// <summary>
    /// Check if this dictionary contains any value with the specified primary key.
    /// </summary>
    /// <param name="key">Primary key to check.</param>
    /// <returns>
    /// True if this dictionary contains at least one value with the specified primary key, otherwise false.
    /// </returns>
    public bool ContainsKey(TPrimaryKey key)
        => _dictionaries.TryGetValue(key, out var dictionary) && !dictionary.IsEmpty;

    /// <summary>
    /// Check if this dictionary contains any value with the specified primary and secondary keys.
    /// </summary>
    /// <param name="primaryKey">Primary key to check.</param>
    /// <param name="secondaryKey">Secondary key to check.</param>
    /// <returns>
    /// True if this dictionary contains at least one value ith the specified primary and secondary keys,
    /// otherwise false.
    /// </returns>
    public bool ContainsKey(TPrimaryKey primaryKey, TSecondaryKey secondaryKey)
        => _dictionaries.GetValueOrDefault(primaryKey)?.ContainsKey(secondaryKey) == true;

    /// <summary>
    /// Clear the whole dictionary.
    /// </summary>
    public void Clear()
        => _dictionaries.Clear();

    /// <summary>
    /// Get the value with the specified primary and secondary keys or default if not found.
    /// </summary>
    /// <param name="primaryKey">Primary key of the value.</param>
    /// <param name="secondaryKey">Secondary key of the value.</param>
    /// <returns>The value with the specified primary and secondary keys, or default if it is not found.</returns>
    public TValue? GetValueOrDefault(TPrimaryKey primaryKey, TSecondaryKey secondaryKey)
        => _dictionaries.GetValueOrDefault(primaryKey)?.TryGetValue(secondaryKey, out var value) == true
            ? value
            : default;

    /// <summary>
    /// Try to get the value with the specified primary and secondary keys.
    /// </summary>
    /// <param name="primaryKey">Primary key of the value.</param>
    /// <param name="secondaryKey">Secondary key of the value.</param>
    /// <param name="value">
    /// The value with the specified primary and secondary keys,
    /// or null if this method returns false.
    /// </param>
    /// <returns>True if the value is found, otherwise false.</returns>
    public bool TryGetValue(TPrimaryKey primaryKey, TSecondaryKey secondaryKey,
        [MaybeNullWhen(false)] out TValue value)
    {
        if (_dictionaries.GetValueOrDefault(primaryKey)?.TryGetValue(secondaryKey, out value) == true)
            return true;
        value = default;
        return false;
    }

    /// <summary>
    /// Try to get the values with the specified primary key.
    /// </summary>
    /// <param name="primaryKey">Primary key of the value.</param>
    /// <param name="dictionary">Dictionary of values with the specified primary key.</param>
    /// <returns>True if the value is found, otherwise false.</returns>
    public bool TryGetValues(TPrimaryKey primaryKey,
        [NotNullWhen(true)] out IReadOnlyDictionary<TSecondaryKey, TValue>? dictionary)
    {
        if (_dictionaries.TryGetValue(primaryKey, out var collection))
        {
            dictionary = collection;
            return true;
        }

        dictionary = null;
        return false;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerator<(TPrimaryKey PrimaryKey, TSecondaryKey SecondaryKey, TValue Value)> GetEnumerator()
    {
        foreach (var (primaryKey, secondaryDictionary) in _dictionaries)
        {
            foreach (var (secondaryKey, value) in secondaryDictionary)
            {
                yield return (primaryKey, secondaryKey, value);
            }
        }
    }
}