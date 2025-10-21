using Microsoft.Extensions.Logging;

namespace InjectionExpert.Utilities;

/// <summary>
/// This provider allows chaining multiple injection providers together.
/// It queries each provider in order until one returns a valid injection.
/// If none of the providers can provide the injection, it returns null.
/// </summary>
/// <param name="providers">
/// Providers to chained together.
/// This enumerable sequence will be enumerated for each injection request.
/// </param>
public class InjectionChainedProvider(IEnumerable<IInjectionProvider?> providers) : IInjectionProvider
{
    public InjectionItem? GetInjectionItem(Type type, object? key, InjectionTarget target)
    {
        foreach (var provider in providers)
        {
            var entry = provider?.GetInjectionItem(type, key, target);
            if (entry != null)
                return entry;
        }
        return null;
    }

    public IInjectionProvider.IScope NewScope(InjectionTarget target) =>
        InjectionScope.New(this, null, target);
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