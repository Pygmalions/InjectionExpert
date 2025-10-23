namespace InjectionExpert;

/// <summary>
/// Injection item, containing the instance and its lifespan.
/// </summary>
/// <param name="Instance">Injection instance.</param>
/// <param name="Lifespan">Lifespan of this injection.</param>
public readonly record struct InjectionItem(object Instance, InjectionLifespan Lifespan)
{
    public static implicit operator ValueTuple<object, InjectionLifespan>(InjectionItem item)
        => (item.Instance, item.Lifespan);

    public static implicit operator InjectionItem(ValueTuple<object, InjectionLifespan> item)
        => new(item.Item1, item.Item2);

    public void Deconstruct(out object instance, out InjectionLifespan lifespan)
    {
        instance = Instance;
        lifespan = Lifespan;
    }
}

public static class InjectionItemFactoryExtensions
{
    extension(InjectionItem)
    {
        /// <summary>
        /// Create an injection item with a <see cref="InjectionLifespan.Transient"/> lifespan.
        /// </summary>
        public static InjectionItem Transient(object instance)
            => new(instance, InjectionLifespan.Transient);
        
        /// <summary>
        /// Create an injection item with a <see cref="InjectionLifespan.Singleton"/> lifespan.
        /// </summary>
        public static InjectionItem Singleton(object instance)
            => new(instance, InjectionLifespan.Singleton);
        
        /// <summary>
        /// Create an injection item with a <see cref="InjectionLifespan.Scoped"/> lifespan.
        /// </summary>
        public static InjectionItem Scoped(object instance)
            => new(instance, InjectionLifespan.Scoped);
    }
}