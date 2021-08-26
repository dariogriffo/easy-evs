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
            var basicType = typeof(IEvent);

            bool ImplementsHandler(Type i)
            {
                return i.GenericTypeArguments != null && i.GenericTypeArguments.Any(g => g.GetInterfaces().Any(gi => gi == basicType));
            }

            RegisteredEventsAndHandlers = services
                .Where(x => ImplementsHandler(x.ServiceType))
                .Where(x => !x.ImplementationType.IsAbstract)
                .ToDictionary(x => x.ServiceType.GenericTypeArguments.First(), x => x.ServiceType);
        }

        internal IReadOnlyDictionary<Type, Type> RegisteredEventsAndHandlers { get; }
    }
}
