namespace InjectionExpert.Utilities;

/// <summary>
/// This class wraps a method delegate into an injection provider.
/// </summary>
/// <param name="factory">Functor to provide injections.</param>
public readonly struct FunctorInjectionProvider(
    FunctorInjectionProvider.FactoryDelegate factory) : IInjectionProvider
{
    public delegate (object Injection, InjectionLifespan Lifespan)? 
        FactoryDelegate(Type type, object? key, InjectionTarget target);
    
    public (object Injection, InjectionLifespan Lifespan)? GetInjection(Type type, object? key, InjectionTarget target)
        => factory(type, key, target);

    public IInjectionProvider.IScope NewScope(InjectionTarget target) =>
        InjectionScope.New(this, null, target);
    }