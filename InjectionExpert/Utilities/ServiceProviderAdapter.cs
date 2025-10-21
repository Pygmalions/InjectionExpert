using Microsoft.Extensions.DependencyInjection;

namespace InjectionExpert.Utilities;

public class ServiceProviderAdapter(IInjectionProvider provider) : IKeyedServiceProvider
{
    public object? GetService(Type serviceType) 
        => provider.GetInjection(serviceType);

    public object? GetKeyedService(Type serviceType, object? serviceKey)
        => provider.GetInjection(serviceType, serviceKey);

    public object GetRequiredKeyedService(Type serviceType, object? serviceKey)
        => provider.RequireInjection(serviceType, serviceKey);
}