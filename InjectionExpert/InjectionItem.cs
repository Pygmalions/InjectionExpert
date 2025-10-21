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