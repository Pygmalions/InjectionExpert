namespace InjectionExpert.Utilities;

/// <summary>
/// This provider allows chaining multiple injection providers together.
/// It queries each provider in order until one returns a valid injection.
/// If none of the providers can provide the injection, it returns null.
/// </summary>
/// <param name="providers">
/// Providers to chain together.
/// This enumerable sequence will be enumerated for each injection request.
/// </param>
public class InjectionChainedProvider(IEnumerable<IInjectionProvider?> providers) : IInjectionProvider
{
    public InjectionEntry? GetEntry(Type type, object? key = null)
        => providers.Select(provider => provider?.GetEntry(type, key)).FirstOrDefault(entry => entry != null);

    public bool HasEntry(Type type, object? key = null)
        => providers.Any(provider => provider?.HasEntry(type, key) == true);

    public object? GetInjection(Type type, object? key = null, InjectionTarget target = default)
        => providers
            .Select(provider => provider?.GetInjection(type, key, target))
            .FirstOrDefault(injection => injection != null);

    public IInjectionScope NewScope() => new InjectionScope(this);
}

public static class InjectionChainedProviderExtensions
{
    extension(IInjectionProvider)
    {
        /// <summary>
        /// Create a new injection provider from a sequence of providers.
        /// The providers will be queried in order until one of them returns a non-null injection.
        /// </summary>
        /// <param name="providers">
        /// Providers to concatenate in sequence;
        /// This sequence will be enumerated every time an injection is requested.
        /// </param>
        /// <returns>Injection provider that queries the specified sequence of providers.</returns>
        public static InjectionChainedProvider FromMultiple(params IEnumerable<IInjectionProvider?> providers)
            => new(providers);
    }
}