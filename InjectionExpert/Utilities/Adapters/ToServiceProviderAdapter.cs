using Microsoft.Extensions.DependencyInjection;

namespace InjectionExpert.Utilities.Adapters;

public class ToServiceProviderAdapter(IInjectionProvider provider) : IKeyedServiceProvider
{
    public object? GetService(Type serviceType) 
        => provider.GetInjection(serviceType);

    public object? GetKeyedService(Type serviceType, object? serviceKey)
        => provider.GetInjection(serviceType, serviceKey);

    public object GetRequiredKeyedService(Type serviceType, object? serviceKey)
        => provider.RequireInjection(serviceType, serviceKey);
}