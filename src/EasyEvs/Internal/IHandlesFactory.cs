namespace EasyEvs.Internal;

using System.Collections.Generic;
using Contracts;
using Microsoft.Extensions.DependencyInjection;

internal interface IHandlesFactory
{
    bool TryGetScopeFor(IEvent @event, out IServiceScope? scope);

    bool TryGetHandlerFor(IEvent @event, IServiceScope scope, out dynamic? handler);

    bool TryGetPipelinesFor(IEvent @event, IServiceScope scope, out List<dynamic>? pipelines);

    bool TryGetPreActionsFor(IEvent @event, IServiceScope scope, out List<dynamic>? preActions);

    bool TryGetPostActionsFor(IEvent @event, IServiceScope scope, out List<dynamic>? postActions);
}
