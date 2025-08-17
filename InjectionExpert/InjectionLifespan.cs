namespace InjectionExpert;

public enum InjectionLifespan
{
    /// <summary>
    /// This injection will be created every time it is requested.
    /// </summary>
    Transient,

    /// <summary>
    /// This injection will be created once and shared across all requests.
    /// </summary>
    Singleton,

    /// <summary>
    /// This injection will be reused within the same scope.
    /// </summary>
    Scoped,
}