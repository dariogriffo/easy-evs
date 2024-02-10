namespace EasyEvs;

using System;
using Contracts;
using Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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

        EasyEvsAggregatesConfiguration configuration = new();
        
        Action<EasyEvsAggregatesConfiguration> decoratorConfigurator = (config) =>
        {
            actionsConfigurator?.Invoke(config);
            config.AggregateStreamResolver ??= typeof(AggregateAttributeResolver);
        };
        
        decoratorConfigurator.Invoke(configuration);
        services.Configure(decoratorConfigurator);

        services.AddSingleton(configuration.AggregateStreamResolver!);
        
        services.AddSingleton<IAggregateStreamResolver>(
            sp =>
                (IAggregateStreamResolver)
                sp.GetRequiredService(
                    sp.GetRequiredService<
                        IOptions<EasyEvsAggregatesConfiguration>
                    >().Value.AggregateStreamResolver!
                )
        );
        
        services.AddSingleton<IAggregateStore, AggregatesStore>();

        return services;
    }
}
