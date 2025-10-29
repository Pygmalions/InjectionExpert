using Microsoft.Extensions.Logging;

namespace InjectionExpert.Utilities;

public static class InjectionContainerLoggingExtensions
{
    extension<TContainer>(TContainer container) where TContainer : IInjectionContainer
    {
        public TContainer AddLogging<TLoggerFactory>(TLoggerFactory factory) where TLoggerFactory : ILoggerFactory
        {
            // Register the factory.
            container.AddSingleton(factory);
            container.AddRedirection(
                typeof(ILoggerFactory), null,
                typeof(TLoggerFactory), null);
            
            // Register loggers.
            container.AddInjection(InjectionLifespan.Transient,
                typeof(Logger<>), (provider, type, key, target) =>
                    Activator.CreateInstance(
                        typeof(Logger<>).MakeGenericType(type.GetGenericArguments()), 
                        provider.RequireInjection<ILoggerFactory>(null, target))!);
            container.AddRedirection(
                typeof(ILogger<>), null,
                typeof(Logger<>), null);
            container.AddInjection(InjectionLifespan.Transient,
                typeof(ILogger), (provider, type, key, target) =>
                    provider.RequireInjection<ILoggerFactory>(null, target)
                        .CreateLogger(target.OwnerType?.FullName ?? "Unnamed"));
            return container;
        }
    }
}