namespace InjectionExpert;

/// <summary>
/// Represents a container that manages dependency injection entries and provides registration,
/// retrieval, and removal capabilities for injection configurations.
/// </summary>
public interface IInjectionContainer : IInjectionProvider
{
    /// <summary>
    /// Gets a collection of all registered injection entries in this container.
    /// Each entry contains the associated type, optional key, and the injection configuration.
    /// </summary>
    IEnumerable<(Type Type, object? Key, InjectionEntry Entry)> Entries { get; }

    /// <summary>
    /// Adds an injection entry for the specified type and key to the container.
    /// </summary>
    /// <param name="type">The type that the injection entry is associated with.</param>
    /// <param name="key">An optional key to distinguish between multiple registrations of the same type.</param>
    /// <param name="entry">The injection entry to add to the container.</param>
    void AddEntry(Type type, object? key, InjectionEntry entry);

    /// <summary>
    /// Attempts to add an injection entry for the specified type and key to the container
    /// if no entry with the same type and key combination already exists.
    /// </summary>
    /// <param name="type">The type that the injection entry is associated with.</param>
    /// <param name="key">An optional key to distinguish between multiple registrations of the same type.</param>
    /// <param name="entry">The injection entry to add to the container.</param>
    /// <returns>
    /// True if the entry was successfully added;
    /// false if an entry with the same type and key already exists.
    /// </returns>
    bool TryAddEntry(Type type, object? key, InjectionEntry entry);

    /// <summary>
    /// Removes the injection entry associated with the specified type and key from the container.
    /// </summary>
    /// <param name="type">The type that the injection entry is associated with.</param>
    /// <param name="key">An optional key to distinguish between multiple registrations of the same type.</param>
    /// <returns>
    /// True if the entry was successfully removed; false if no matching entry was found.
    /// </returns>
    bool RemoveEntry(Type type, object? key = null);

    /// <summary>
    /// Clear all injection entries from this container.
    /// </summary>
    void ClearEntries();
}