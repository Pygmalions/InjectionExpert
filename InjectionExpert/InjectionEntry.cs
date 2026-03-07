namespace InjectionExpert;

public abstract class InjectionEntry
{
    /// <summary>
    /// Lifespan of the injection.
    /// </summary>
    public abstract InjectionLifespan Lifespan { get; }
    
    /// <summary>
    /// Determine whether the injection of this entry is assignable to the specified type.
    /// </summary>
    /// <param name="type">Category type to check.</param>
    /// <returns>
    /// True if the injection of this entry is assignable to the specified category type; false otherwise.
    /// </returns>
    public abstract bool IsAssignableTo(Type type);

    /// <summary>
    /// Retrieve an injection instance based on the specified type, key, and target.
    /// </summary>
    /// <param name="provider">The injection scope provider used to resolve dependencies.</param>
    /// <param name="type">The type of the object to be injected.</param>
    /// <param name="key">An optional key to distinguish between multiple registrations of the same type.</param>
    /// <param name="target">The target location where the injection will be applied.</param>
    /// <returns>An object instance that matches the requested injection criteria.</returns>
    public abstract object GetInjection(IInjectionProvider provider, Type type, object? key, InjectionTarget target);
}