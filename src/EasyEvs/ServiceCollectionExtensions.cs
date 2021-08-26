namespace EasyEvs
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Internal;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEasyEvs(this IServiceCollection services, Type? streamResolver = null, params Assembly[]? assemblies)
        {
            services.AddScoped(typeof(IStreamResolver), streamResolver ?? typeof(NoOpStreamResolver));
            services.AddSingleton<EventStoreSettings>();
            services.AddSingleton<ISerializer, Serializer>();
            services.AddSingleton<IHandlesFactory, HandlesFactory>();
            services.AddSingleton<IConnectionProvider, ConnectionProvider>();
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
                return i.GenericTypeArguments != null && i.GenericTypeArguments.Any(g => g.GetInterfaces().Any(gi => gi == basicType));
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
