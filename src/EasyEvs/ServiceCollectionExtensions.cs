namespace EasyEvs;

using System;
using System.Linq;
using System.Reflection;
using Contracts;
using Contracts.Internal;
using Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

/// <summary>
/// The integration point with the <see cref="Microsoft.Extensions.DependencyInjection"/> framework
/// </summary>
public static class ServiceCollectionExtensions
{
    private static HandlersAndEventTypes? _handlersAndTypes = null!;

    /// <summary>
    /// Add the EventStore to your app.
    /// This is the scenario for event readers.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> where the registration happens.</param>
    /// <param name="configuration">The configuration for the Dependency Injection.</param>
    /// <returns></returns>
    public static IServiceCollection AddEasyEvs(
        this IServiceCollection services,
        EasyEvsDependencyInjectionConfiguration? configuration = default
    )
    {
        var jsonSerializerOptionsProvider = configuration?.JsonSerializerOptionsProvider;
        var streamResolver = configuration?.StreamResolver;
        var assemblies = configuration?.Assemblies;

        if (
            jsonSerializerOptionsProvider != null
            && jsonSerializerOptionsProvider!
                .GetInterfaces()
                .All(x => x != typeof(IJsonSerializerOptionsProvider))
        )
        {
            throw new ArgumentException(
                "The jsonSerializerOptionsProvider doesn't implement IJsonSerializerOptionsProvider.",
                nameof(jsonSerializerOptionsProvider)
            );
        }

        if (
            streamResolver != null
            && streamResolver!.GetInterfaces().All(x => x != typeof(IStreamResolver))
        )
        {
            throw new ArgumentException(
                "The streamResolver doesn't implement IStreamResolver.",
                nameof(streamResolver)
            );
        }

        services.AddSingleton(
            typeof(IJsonSerializerOptionsProvider),
            jsonSerializerOptionsProvider ?? typeof(JsonSerializerOptionsProvider)
        );
        services.AddSingleton(
            typeof(IStreamResolver),
            streamResolver
                ?? (
                    (configuration?.DefaultStreamResolver ?? false)
                        ? typeof(AggregateRootAttributeResolver)
                        : typeof(NoOpStreamResolver)
                )
        );
        services.AddSingleton<EventStoreSettings>();
        services.AddSingleton<IConnectionRetry, BasicConnectionRetry>();
        services.AddSingleton<ISerializer, Serializer>();
        services.AddSingleton<IHandlesFactory, HandlesFactory>();
        services.AddSingleton<IEventStore, EventStore>();
        services.TryAddSingleton(services);
        HandlersAndEventTypes handlersAndTypes;
        if (assemblies == null || assemblies.Length == 0)
        {
            _handlersAndTypes = handlersAndTypes = new HandlersAndEventTypes(services);
            services.AddSingleton(handlersAndTypes);
            return services;
        }

        RegisterTargetWithAttribute(
            services,
            assemblies,
            typeof(HandlesEventAttribute),
            configuration?.DefaultHandlesLifetime ?? ServiceLifetime.Scoped
        );

        RegisterTargetWithAttribute(
            services,
            assemblies,
            typeof(PostHandlerEventAttribute),
            configuration?.DefaultHandlesLifetime ?? ServiceLifetime.Scoped
        );

        RegisterTargetWithAttribute(
            services,
            assemblies,
            typeof(PreActionEventAttribute),
            configuration?.DefaultHandlesLifetime ?? ServiceLifetime.Scoped
        );

        _handlersAndTypes = handlersAndTypes = new HandlersAndEventTypes(services);
        services.AddSingleton(handlersAndTypes);

        return services;
    }

    /// <summary>
    /// Registers a class as <see cref="IPipelineHandlesEventAction{T}"/>
    /// </summary>
    /// <param name="services"></param>
    /// <param name="lifetime"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IServiceCollection WithPipeline<T>(
        this IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Scoped
    )
    {
        var pipeline = typeof(T);
        var attributeType = typeof(PipelineHandlerAttribute);
        foreach (
            var target in pipeline
                .GetInterfaces()
                .Where(i => i.GetCustomAttributes(true).Any(a => a.GetType() == attributeType))
        )
        {
            var descriptor = new ServiceDescriptor(target, pipeline, lifetime);
            services.Add(descriptor);
        }

        _handlersAndTypes!.ScanPipelines(services);
        return services;
    }

    private static void RegisterTargetWithAttribute(
        IServiceCollection services,
        Assembly[] assemblies,
        Type attributeType,
        ServiceLifetime lifetime = ServiceLifetime.Scoped
    )
    {
        var handlers = assemblies
            .Distinct()
            .SelectMany(x =>
                x.GetExportedTypes()
                    .Where(y => !y.IsAbstract)
                    .Where(y =>
                        y.GetInterfaces()
                            .Any(i =>
                                i.GetCustomAttributes(true).Any(a => a.GetType() == attributeType)
                            )
                    )
                    .ToList()
            );

        foreach (var handler in handlers)
        {
            foreach (
                var target in handler
                    .GetInterfaces()
                    .Where(i => i.GetCustomAttributes(true).Any(a => a.GetType() == attributeType))
            )
            {
                var descriptor = new ServiceDescriptor(target, handler, lifetime);
                services.Add(descriptor);
            }
        }
    }
}
