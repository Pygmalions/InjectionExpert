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
public class ChainedInjectionProvider(IEnumerable<IInjectionProvider?> providers) : IInjectionProvider
{
    public InjectionItem? GetInjection(Type type, object? key, InjectionTarget target)
    {
        foreach (var provider in providers)
        {
            var entry = provider?.GetInjection(type, key, target);
            if (entry != null)
                return entry;
        }
        return null;
    }

    public IInjectionProvider.IScope NewScope(InjectionTarget target) =>
        InjectionScope.New(this, null, target);
}