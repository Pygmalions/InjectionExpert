namespace InjectionExpert.Utilities;

public readonly struct ChainedInjectionProvider(IEnumerable<IInjectionProvider> providers) : IInjectionProvider
{
    public (object, InjectionLifespan)? GetInjection(Type type, object? key, InjectionTarget target)
    {
        foreach (var provider in providers)
        {
            var entry = provider.GetInjection(type, key, target);
            if (entry != null)
                return entry;
        }
        return null;
    }

    public IInjectionProvider.IScope NewScope(InjectionTarget target) =>
        InjectionScope.New(this, null, target);
}