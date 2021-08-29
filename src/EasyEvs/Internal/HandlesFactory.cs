namespace EasyEvs.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.DependencyInjection;

    internal class HandlesFactory : IHandlesFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IReadOnlyDictionary<Type, Type> _handlers;
        private readonly IReadOnlyDictionary<Type, Type> _pre;
        private readonly IReadOnlyDictionary<Type, Type> _post;

        public HandlesFactory(IServiceProvider serviceProvider, HandlersAndEventTypes handlersAndEventTypes)
        {
            _serviceProvider = serviceProvider;
            _handlers = handlersAndEventTypes.RegisteredEventsAndHandlers;
            _pre = handlersAndEventTypes.RegisteredPreActions;
            _post = handlersAndEventTypes.RegisteredPostActions;

        }

        public bool TryGetHandlerFor(
            IEvent @event,
            out IHandlesEvent? handler,
            out IDisposable? scope,
            out List<IPreHandlesEventAction>? preActions,
            out List<IPostHandlesEventAction>? postActions)
        {
            handler = default;
            scope = default;
            preActions = default;
            postActions = default;
            if (!_handlers.TryGetValue(@event.GetType(), out var type))
            {
                return false;
            }

            IServiceScope serviceScope = _serviceProvider.CreateScope();
            handler = serviceScope.ServiceProvider.GetService(type) as IHandlesEvent;
            scope = serviceScope;

            if (_pre.TryGetValue(@event.GetType(), out type))
            {
                preActions = serviceScope
                    .ServiceProvider.GetServices(type).Select(x => (x as IPreHandlesEventAction)!)
                    .ToList();
            }
            

            if (_post.TryGetValue(@event.GetType(), out type))
            {
                postActions = serviceScope
                    .ServiceProvider.GetServices(type).Select(x => (x as IPostHandlesEventAction)!)
                    .ToList();
            }

            return true;
        }
    }
}

