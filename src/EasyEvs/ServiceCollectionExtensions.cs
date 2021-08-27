namespace EasyEvs
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Internal;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    /// <summary>
    /// The integration point with the <see cref="Microsoft.Extensions.DependencyInjection"/> framework
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add the EventStore to your app.
        /// If no <paramref name="streamResolver"/> is passed, there will be no way to store events.
        /// This is the scenario for event readers.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> where the registration happens.</param>
        /// <param name="jsonSerializerOptionsProvider">The optional <see cref="IJsonSerializerOptionsProvider"/> type implementation.</param>
        /// <param name="streamResolver">The optional <see cref="IStreamResolver"/> type implementation.</param>
        /// <param name="assemblies">An array of <see cref="Assembly"/> where the handlers of events are to be scanned.</param>
        /// <returns></returns>
        public static IServiceCollection AddEasyEvs(
            this IServiceCollection services,
            Type? jsonSerializerOptionsProvider = null,
            Type? streamResolver = null,
            params Assembly[]? assemblies)
        {
            if (jsonSerializerOptionsProvider != null &&
                jsonSerializerOptionsProvider!.GetInterfaces().All(x => x != typeof(IJsonSerializerOptionsProvider)))
            {
                throw new ArgumentException(
                    "The jsonSerializerOptionsProvider doesn't implement IJsonSerializerOptionsProvider.",
                    nameof(jsonSerializerOptionsProvider));
            }

            if (streamResolver != null &&
                streamResolver!.GetInterfaces().All(x => x != typeof(IStreamResolver)))
            {
                throw new ArgumentException(
                    "The streamResolver doesn't implement IStreamResolver.",
                    nameof(streamResolver));
            }

            services.AddSingleton(typeof(IJsonSerializerOptionsProvider), jsonSerializerOptionsProvider ?? typeof(JsonSerializerOptionsProvider));
            services.AddScoped(typeof(IStreamResolver), streamResolver ?? typeof(NoOpStreamResolver));
            services.AddSingleton<EventStoreSettings>();
            services.AddSingleton<ISerializer, Serializer>();
            services.AddSingleton<IHandlesFactory, HandlesFactory>();
            services.AddSingleton<IEventStore, EventStore>();
            services.TryAddSingleton(services);
            HandlersAndEventTypes handlersAndTypes;
            if (assemblies == null || assemblies.Length == 0)
            {
                handlersAndTypes = new HandlersAndEventTypes(services);
                services.AddSingleton(handlersAndTypes);
                return services;
            }

            var basicType = typeof(IEvent);

            bool ImplementsHandler(Type i)
            {
                return i.GenericTypeArguments.Any(g => g.GetInterfaces().Any(gi => gi == basicType));
            }

            var handlers =
                assemblies
                    .Distinct()
                    .SelectMany(x =>
                        x.GetExportedTypes()
                            .Where(y => !y.IsAbstract)
                            .Where(y => y.GetInterfaces().Any(ImplementsHandler))
                            .ToList());

            foreach (var handler in handlers)
            {
                foreach (var target in handler.GetInterfaces().Where(ImplementsHandler))
                {
                    services.TryAddScoped(target, handler);
                }
            }

            handlersAndTypes = new HandlersAndEventTypes(services);
            services.AddSingleton(handlersAndTypes);

            return services;
        }
    }
}
