namespace InjectionExpert;

public interface IInjectionContainer : IInjectionProvider, IEnumerable<(Type Type, object? Key, InjectionEntry Entry)>
{
    /// <summary>
    /// Get the injection entry for the specified type and key.
    /// </summary>
    /// <param name="type">Type that the entry is associated with.</param>
    /// <param name="key">Optional key of the entry.</param>
    /// <returns>Entry with the specified type and key, or null if not found. </returns>
    InjectionEntry? GetInjectionEntry(Type type, object? key);

    /// <summary>
    /// Add an injection entry for the specified type and key.
    /// </summary>
    /// <param name="type">Type that the entry is associated with.</param>
    /// <param name="key">Optional key of the entry.</param>
    /// <param name="entry">Injection entry to add.</param>
    void AddInjection(Type type, object? key, InjectionEntry entry);

    /// <summary>
    /// Try to add an injection entry for the specified type and key,
    /// if no entry with the same type and key exists.
    /// </summary>
    /// <param name="type">Type that the entry is associated with.</param>
    /// <param name="key">Optional key of the entry.</param>
    /// <param name="entry">Injection entry to add.</param>
    /// <returns>True if the entry is added, or false if an entry with the same type and key exists.</returns>
    bool TryAddInjection(Type type, object? key, InjectionEntry entry);

    /// <summary>
    /// Remove the injection entry for the specified type and key.
    /// </summary>
    /// <param name="type">Type that the entry is associated with.</param>
    /// <param name="key">Optional key of the entry.</param>
    /// <returns>True if the entry is removed, or false if the entry is not found.</returns>
    bool RemoveInjection(Type type, object? key = null);

    /// <summary>
    /// Clear all cached non-constant singleton values.
    /// </summary>
    void InvalidateCache();

    /// <summary>
    /// Clear all injection entries.
    /// </summary>
    void Clear();
}