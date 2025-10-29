using Microsoft.Extensions.DependencyInjection;

namespace InjectionExpert.Utilities.Adapters;

public class FromServiceProviderAdapter(IServiceProvider provider) : IInjectionProvider
{
    private readonly IKeyedServiceProvider? _keyedProvider = provider as IKeyedServiceProvider;
    
    public InjectionItem? GetInjectionItem(Type type, object? key, InjectionTarget target)
    {
        var service = _keyedProvider is not null
            ? _keyedProvider.GetKeyedService(type, key)
            : provider.GetService(type);
        if (service is null)
            return null;
        return InjectionItem.Transient(service);
    }

    public IInjectionProvider.IScope NewScope(InjectionTarget target = default)
        => new FromServiceScopeAdapter(provider.CreateScope(), null, target);
}