namespace EasyEvs;

using System;
using Contracts;
using Internal;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// The integration point with the <see cref="Microsoft.Extensions.DependencyInjection"/> framework
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds EasyEvs to your app allowing to configuring options
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> where the registration happens.</param>
    /// <param name="actionsConfigurator">The configuration for the Dependency Injection.</param>
    /// <returns></returns>
    public static IServiceCollection AddEasyEvsAggregates(
        this IServiceCollection services,
        Action<EasyEvsAggregatesConfiguration>? actionsConfigurator = default
    )
    {
        static void RegisterProvidedTypeOrFallback(
            IServiceCollection services,
            Type serviceType,
            Type? implementationType,
            Type fallbackType
        )
        {
            services.AddSingleton(serviceType, implementationType ?? fallbackType);
        }

        EasyEvsAggregatesConfiguration configuration = new();
        actionsConfigurator?.Invoke(configuration);

        services.AddSingleton<IAggregateStore, AggregatesStore>();

        RegisterProvidedTypeOrFallback(
            services,
            typeof(IAggregateStreamResolver),
            configuration.AggregateStreamResolver,
            typeof(AggregateAttributeResolver)
        );

        return services;
    }
}
