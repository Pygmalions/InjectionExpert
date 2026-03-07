namespace InjectionExpert.Utilities;

public class InjectionDelegateProvider(IInjectionProvider.InjectionResolver resolver) : IInjectionProvider
{
    public IEnumerable<IInjectionProvider.InjectionResolver> Resolvers
    {
        get { yield return resolver; }
    }

    public InjectionEntry? GetEntry(Type type, object? key = null)
        => null;

    public bool HasEntry(Type type, object? key = null)
        => false;

    public object? GetInjection(Type type, object? key = null, InjectionTarget target = default)
        => resolver(this, type, key, target);

    public IInjectionScope NewScope() => new InjectionScope(this);
}

public static class InjectionDelegateProviderExtensions
{
    public static IInjectionProvider.InjectionResolver FromFunctor(IInjectionProvider.InjectionResolver resolver)
        => new(resolver);
}