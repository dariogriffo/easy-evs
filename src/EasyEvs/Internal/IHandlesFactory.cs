namespace EasyEvs.Internal
{
    using System;
    using System.Collections.Generic;
    using Contracts;
    using Microsoft.Extensions.DependencyInjection;

    internal interface IHandlesFactory
    {
        bool TryGetScopeFor(
            IEvent @event,
            out IServiceScope? scope);

        bool TryGetHandlerFor(
            IEvent @event,
            IServiceScope scope,
            out IHandlesEvent? handler);

        bool TryGetPipelinesFor(
            IEvent @event,
            IServiceScope scope,
            out List<IPipelineHandlesEventAction>? pipelines);

        bool TryGetPreActionsFor(
            IEvent @event,
            IServiceScope scope,
            out List<IPreHandlesEventAction>? preActions);

        bool TryGetPostActionsFor(
            IEvent @event,
            IServiceScope scope,
            out List<IPostHandlesEventAction>? postActions);
    }
}
