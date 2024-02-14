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
using Microsoft.Extensions.Options;

/// <summary>
/// The integration point with the <see cref="Microsoft.Extensions.DependencyInjection"/> framework
/// </summary>
public static class ServiceCollectionExtensions
{
    private static HandlersAndEventTypes? _handlersAndTypes;

    /// <summary>
    /// Adds EasyEvs to your app allowing to configuring options
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> where the registration happens.</param>
    /// <param name="actionsConfigurator">The configuration for the Dependency Injection.</param>
    /// <param name="eventStoreSettingsProvider">A function to resolve the <see cref="EventStoreSettings"/></param>
    /// <exception cref="ArgumentException">The <see cref="IJsonSerializerOptionsProvider"/> type provided does not implement the <see cref="IJsonSerializerOptionsProvider"/> interface.</exception>
    /// <exception cref="ArgumentException">The <see cref="IEventsStreamResolver"/> type provided does not implement the <see cref="IEventsStreamResolver"/> interface.</exception>
    /// <returns></returns>
    public static IServiceCollection AddEasyEvs(
        this IServiceCollection services,
        Func<IServiceProvider, EventStoreSettings>? eventStoreSettingsProvider,
        Action<EasyEvsConfiguration>? actionsConfigurator = default
    )
    {
        static void ValidateProvidedType(Type type, Type expectedInterfaceType, string s)
        {
            if (type.GetInterfaces().All(x => x != expectedInterfaceType))
            {
                throw new ArgumentException(s, nameof(type));
            }
        }

        Action<EasyEvsConfiguration> decoratorConfigurator = (config) =>
        {
            actionsConfigurator?.Invoke(config);
            config.JsonSerializerOptionsProviderType ??=
                typeof(DefaultJsonSerializerOptionsProvider);
            config.EventsStreamResolverType ??= typeof(NoEventsStreamResolver);
            config.ReconnectionStrategyType ??= typeof(BasicReconnectionStrategy);
        };

        EasyEvsConfiguration configuration = new();
        decoratorConfigurator.Invoke(configuration);

        Type jsonSerializerOptionsProviderType = configuration.JsonSerializerOptionsProviderType!;
        Type eventsStreamResolverType = configuration.EventsStreamResolverType!;
        Type reconnectionStrategyType = configuration.ReconnectionStrategyType!;

        const string messageTemplate = "The {0} type doesn't implement the interface: {1}.";

        Type expectedInterfaceType = typeof(IJsonSerializerOptionsProvider);
        Type providedType = jsonSerializerOptionsProviderType;
        string message = string.Format(messageTemplate, providedType, expectedInterfaceType);
        ValidateProvidedType(providedType, expectedInterfaceType, message);

        expectedInterfaceType = typeof(IEventsStreamResolver);
        providedType = eventsStreamResolverType;
        message = string.Format(messageTemplate, providedType, expectedInterfaceType);
        ValidateProvidedType(providedType, expectedInterfaceType, message);

        expectedInterfaceType = typeof(IConnectionStrategy);
        providedType = reconnectionStrategyType;
        message = string.Format(messageTemplate, providedType, expectedInterfaceType);
        ValidateProvidedType(providedType, expectedInterfaceType, message);

        services.AddSingleton(configuration.JsonSerializerOptionsProviderType!);
        services.AddSingleton(configuration.ReconnectionStrategyType!);
        services.AddSingleton(configuration.EventsStreamResolverType!);

        services.AddTransient<IConnectionStrategy>(
            sp =>
                (IConnectionStrategy)
                    sp.GetRequiredService(
                        sp.GetRequiredService<
                            IOptions<EasyEvsConfiguration>
                        >().Value.ReconnectionStrategyType!
                    )
        );

        services.AddSingleton<IJsonSerializerOptionsProvider>(sp =>
        {
            EasyEvsConfiguration easyEvsConfiguration = sp.GetRequiredService<
                IOptions<EasyEvsConfiguration>
            >().Value;
            return (IJsonSerializerOptionsProvider)
                sp.GetRequiredService(easyEvsConfiguration.JsonSerializerOptionsProviderType!);
        });

        services.AddSingleton<IEventsStreamResolver>(
            sp =>
                (IEventsStreamResolver)
                    sp.GetRequiredService(
                        sp.GetRequiredService<
                            IOptions<EasyEvsConfiguration>
                        >().Value.EventsStreamResolverType!
                    )
        );

        services.AddSingleton(sp =>
        {
            EventStoreSettings eventStoreSettings = eventStoreSettingsProvider!.Invoke(sp);
            _ =
                eventStoreSettings.ConnectionString
                ?? throw new ArgumentNullException(nameof(eventStoreSettings.ConnectionString));
            return eventStoreSettings;
        });

        services.AddSingleton<ISerializer, Serializer>();
        services.AddSingleton<IHandlesFactory, HandlesFactory>();
        services.AddSingleton<IEventStore, EventStore>();
        services.AddSingleton<IReadEventStore, ReadEventStore>();
        services.AddSingleton<IWriteEventStore, WriteEventStore>();
        services.AddSingleton<IEventStore, EventStore>();
        services.AddSingleton<IPersistentSubscriber, PersistentSubscriber>();
        services.AddSingleton<IInternalPersistentSubscriber, InternalPersistentSubscriber>();
        services.AddSingleton<IConnectionProvider, ConnectionProvider>();
        services.TryAddSingleton(services);
        services.Configure(decoratorConfigurator);

        HandlersAndEventTypes handlersAndTypes;

        Assembly[]? assemblies = configuration.AssembliesToScanForHandlers;

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
            configuration.DefaultHandlesLifetime
        );

        RegisterTargetWithAttribute(
            services,
            assemblies,
            typeof(PostHandlerEventAttribute),
            configuration.DefaultPostActionsLifetime
        );

        RegisterTargetWithAttribute(
            services,
            assemblies,
            typeof(PreActionEventAttribute),
            configuration.DefaultPreActionsLifetime
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
                assembly =>
                    GetNonAbstractClasses(assembly)
                        .Where(c => ImplementsAnInterfaceDecoratedWithAttribute(c, attributeType))
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

        return;

        static IEnumerable<Type> GetNonAbstractClasses(Assembly assembly)
        {
            return assembly.GetExportedTypes().Where(y => !y.IsAbstract);
        }

        static bool ImplementsAnInterfaceDecoratedWithAttribute(Type type, Type attribute)
        {
            return type.GetInterfaces()
                .Any(i => i.GetCustomAttributes(true).Any(a => a.GetType() == attribute));
        }
    }
}
