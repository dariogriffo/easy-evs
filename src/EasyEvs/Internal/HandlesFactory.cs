namespace EasyEvs.Internal;

using System;
using System.Collections.Generic;
using System.Linq;
using Contracts;
using Microsoft.Extensions.DependencyInjection;

internal sealed class HandlesFactory : IHandlesFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IReadOnlyDictionary<Type, Type> _handlers;
    private readonly IReadOnlyDictionary<Type, Type> _pre;
    private readonly IReadOnlyDictionary<Type, Type> _post;
    private readonly IReadOnlyDictionary<Type, Type> _pipelines;

    public HandlesFactory(
        IServiceProvider serviceProvider,
        HandlersAndEventTypes handlersAndEventTypes
    )
    {
        _serviceProvider = serviceProvider;
        _handlers = handlersAndEventTypes.RegisteredEventsAndHandlers;
        _pre = handlersAndEventTypes.RegisteredPreActions;
        _post = handlersAndEventTypes.RegisteredPostActions;
        _pipelines = handlersAndEventTypes.RegisteredPipelines;
    }

    public bool TryGetScopeFor(IEvent @event, out IServiceScope? scope)
    {
        scope = default;
        if (!_handlers.TryGetValue(@event.GetType(), out Type? _))
        {
            return false;
        }

        scope = _serviceProvider.CreateScope();
        return true;
    }

    public bool TryGetHandlerFor(IEvent @event, IServiceScope scope, out dynamic? handler)
    {
        handler = default;
        if (!_handlers.TryGetValue(@event.GetType(), out Type? type))
        {
            return false;
        }

        handler = scope.ServiceProvider.GetService(type);
        return true;
    }

    public bool TryGetPipelinesFor(IEvent @event, IServiceScope scope, out List<dynamic>? pipelines)
    {
        pipelines = default;

        if (!_pipelines.TryGetValue(@event.GetType(), out Type? type))
        {
            return false;
        }

        pipelines = scope.ServiceProvider.GetServices(type).Select(x => (x as dynamic)!).ToList();

        return true;
    }

    public bool TryGetPreActionsFor(
        IEvent @event,
        IServiceScope scope,
        out List<dynamic>? preActions
    )
    {
        preActions = default;

        if (!_pre.TryGetValue(@event.GetType(), out Type? type))
        {
            return false;
        }

        preActions = scope.ServiceProvider.GetServices(type).Select(x => (x as dynamic)!).ToList();
        return true;
    }

    public bool TryGetPostActionsFor(
        IEvent @event,
        IServiceScope scope,
        out List<dynamic>? postActions
    )
    {
        postActions = default;

        if (!_post.TryGetValue(@event.GetType(), out Type? type))
        {
            return false;
        }

        postActions = scope.ServiceProvider.GetServices(type).Select(x => (x as dynamic)!).ToList();
        return true;
    }
}
