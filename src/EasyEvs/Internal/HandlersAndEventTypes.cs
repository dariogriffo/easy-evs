namespace EasyEvs.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.DependencyInjection;

    internal class HandlersAndEventTypes
    {
        internal HandlersAndEventTypes(IServiceCollection services)
        {
            var eventType = typeof(IEvent);
            var handlerType = typeof(IHandlesEvent);
            var preType = typeof(IPreHandlesEventAction);
            var postType = typeof(IPostHandlesEventAction);

            bool Implements(Type i, Type targetType)
            {
                return i.GenericTypeArguments != null &&
                       i.GetInterfaces().Any(gi => gi == targetType) &&
                       i.GenericTypeArguments.Any(g => g.GetInterfaces().Any(gi => gi == eventType));
            }

            RegisteredEventsAndHandlers = services
                .Where(x => Implements(x.ServiceType, handlerType))
                .Where(i => !i.ImplementationType!.IsAbstract)
                .ToDictionary(x => x.ServiceType.GenericTypeArguments.First(), x => x.ServiceType);

            RegisteredPreActions = services
                .Where(x => Implements(x.ServiceType, preType))
                .Where(i => !i.ImplementationType!.IsAbstract)
                .GroupBy(x => x.ServiceType.GenericTypeArguments.First())
                .ToDictionary(x => x.Key, x => x.First().ServiceType);

            RegisteredPostActions = services
                .Where(x => Implements(x.ServiceType, postType))
                .Where(i => !i.ImplementationType!.IsAbstract)
                .GroupBy(x => x.ServiceType.GenericTypeArguments.First())
                .ToDictionary(x => x.Key, x => x.First().ServiceType);
        }

        internal IReadOnlyDictionary<Type, Type> RegisteredEventsAndHandlers { get; }

        internal IReadOnlyDictionary<Type, Type> RegisteredPreActions { get; }

        internal IReadOnlyDictionary<Type, Type> RegisteredPostActions { get; }
    }
}
