using Microsoft.Extensions.DependencyInjection;

namespace InjectionExpert.Utilities.Adapters;

public class FromServiceScopeAdapter(
    IServiceScope scope, 
    IInjectionProvider.IScope? parent = null, 
    InjectionTarget target = default) : IInjectionProvider.IScope
{
    private readonly FromServiceProviderAdapter _provider = new(scope.ServiceProvider);

    public InjectionItem? GetInjectionItem(Type type, object? key, InjectionTarget target)
        => _provider.GetInjectionItem(type, key, target);

    public IInjectionProvider.IScope NewScope(InjectionTarget target = default)
        => new FromServiceScopeAdapter(scope.ServiceProvider.CreateScope(), this, target);

    public void Dispose() => scope.Dispose();

    public InjectionTarget Target { get; } = target;

    public IInjectionProvider.IScope? Parent { get; } = parent;
}