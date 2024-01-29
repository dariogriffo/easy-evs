namespace EasyEvs;

using System;
using System.Collections.Generic;
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
    /// <exception cref="ArgumentException">The <see cref="IJsonSerializerOptionsProvider"/> type provided does not implement the <see cref="IJsonSerializerOptionsProvider"/> interface.</exception>
    /// <exception cref="ArgumentException">The <see cref="IStreamResolver"/> type provided does not implement the <see cref="IStreamResolver"/> interface.</exception>
    /// <returns></returns>
    public static IServiceCollection AddEasyEvs(
        this IServiceCollection services,
        EasyEvsDependencyInjectionConfiguration? configuration = default
    )
    {
        Type? jsonSerializerOptionsProvider = configuration?.JsonSerializerOptionsProvider;
        Type? streamResolver = configuration?.StreamResolver;
        Assembly[]? assemblies = configuration?.Assemblies;

        if (
            jsonSerializerOptionsProvider is not null
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
            streamResolver is not null
            && streamResolver!.GetInterfaces().All(x => x != typeof(IStreamResolver))
        )
        {
            throw new ArgumentException(
                "The streamResolver doesn't implement IStreamResolver.",
                nameof(streamResolver)
            );
        }

        services.TryAddSingleton(
            typeof(IJsonSerializerOptionsProvider),
            jsonSerializerOptionsProvider ?? typeof(JsonSerializerOptionsProvider)
        );

        services.TryAddSingleton(
            typeof(IStreamResolver),
            streamResolver
                ?? (
                    (configuration?.DefaultStreamResolver ?? false)
                        ? typeof(AggregateAttributeResolver)
                        : typeof(NoOpStreamResolver)
                )
        );

        services.TryAddSingleton<EventStoreSettings>();
        services.TryAddSingleton<IConnectionRetry, BasicConnectionRetry>();
        services.TryAddSingleton<ISerializer, Serializer>();
        services.TryAddSingleton<IHandlesFactory, HandlesFactory>();
        services.TryAddSingleton<IEventStore, EventStore>();
        services.TryAddSingleton(services);
        HandlersAndEventTypes handlersAndTypes;
        if (assemblies is null || assemblies.Length == 0)
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
        Type pipeline = typeof(T);
        Type attributeType = typeof(PipelineHandlerAttribute);
        foreach (
            Type target in pipeline
                .GetInterfaces()
                .Where(i => i.GetCustomAttributes(true).Any(a => a.GetType() == attributeType))
        )
        {
            ServiceDescriptor descriptor = new(target, pipeline, lifetime);
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
        IEnumerable<Type> handlers = assemblies
            .Distinct()
            .SelectMany(
                x =>
                    x.GetExportedTypes()
                        .Where(y => !y.IsAbstract)
                        .Where(
                            y =>
                                y.GetInterfaces()
                                    .Any(
                                        i =>
                                            i.GetCustomAttributes(true)
                                                .Any(a => a.GetType() == attributeType)
                                    )
                        )
                        .ToList()
            );

        foreach (Type handler in handlers)
        {
            foreach (
                Type target in handler
                    .GetInterfaces()
                    .Where(i => i.GetCustomAttributes(true).Any(a => a.GetType() == attributeType))
            )
            {
                ServiceDescriptor descriptor = new(target, handler, lifetime);
                services.Add(descriptor);
            }
        }
    }
}
