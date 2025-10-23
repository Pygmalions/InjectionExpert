namespace InjectionExpert;

public abstract class InjectionEntry(InjectionLifespan lifespan)
{
    /// <summary>
    /// Lifespan of this entry.
    /// </summary>
    public InjectionLifespan Lifespan { get; } = lifespan;

    /// <summary>
    /// Get the injection from this entry.
    /// </summary>
    /// <param name="type">Actually requested type.</param>
    /// <param name="target">Target that requests the injection.</param>
    /// <returns>Injection value.</returns>
    public abstract object GetInjection(Type type, InjectionTarget target);

    /// <summary>
    /// Invalidate the cache of this entry.
    /// </summary>
    /// <returns>
    /// True if the cache is removed, or false if this entry currently does not have a cache.
    /// This return value can help identify whether the state of the container has truly changed or not.
    /// </returns>
    public virtual bool InvalidateCache() => false;
}