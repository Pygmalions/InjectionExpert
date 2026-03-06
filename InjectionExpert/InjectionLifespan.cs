namespace InjectionExpert;

public enum InjectionLifespan
{
    /// <summary>
    /// Transient injections will not be cached and reused.
    /// However, if they are <see cref="IDisposable"/> or <see cref="IAsyncDisposable"/>, they will be tracked
    /// by the providers or scopes where they are requested and disposed when their scope ends.
    /// </summary>
    Transient,

    /// <summary>
    /// Singleton injections are managed by the providers, where they are created once and reused for later requests.
    /// </summary>
    Singleton,

    /// <summary>
    /// Scoped injections are managed by the scopes where they are requested and created.
    /// </summary>
    Scoped,
}