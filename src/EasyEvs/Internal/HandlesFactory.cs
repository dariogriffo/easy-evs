namespace EasyEvs.Internal
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Extensions.DependencyInjection;

    internal class HandlesFactory : IHandlesFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IReadOnlyDictionary<Type, Type> _registeredHandlers;

        public HandlesFactory(IServiceProvider serviceProvider, HandlersAndEventTypes handlersAndEventTypes)
        {
            _serviceProvider = serviceProvider;
            _registeredHandlers = handlersAndEventTypes.RegisteredEventsAndHandlers;
        }

        public bool TryGetHandlerFor(IEvent @event, out IHandlesEvent handler, out IDisposable scope)
        {
            handler = null;
            scope = null;
            if (!_registeredHandlers.TryGetValue(@event.GetType(), out var type))
            {
                return false;
            }

            IServiceScope serviceScope = _serviceProvider.CreateScope();
            handler = serviceScope.ServiceProvider.GetService(type) as IHandlesEvent;
            scope = serviceScope;
            return true;
        }
    }
}
