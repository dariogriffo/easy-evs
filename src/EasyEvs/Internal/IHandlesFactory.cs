namespace EasyEvs.Internal
{
    using System;
    using System.Collections.Generic;
    using Contracts;

    internal interface IHandlesFactory
    {
        bool TryGetHandlerFor(
            IEvent @event,
            out IHandlesEvent? handler,
            out IDisposable? scope,
            out List<IPreHandlesEventAction>? preActions,
            out List<IPostHandlesEventAction>? postActions,
            out List<IPipelineHandlesEventAction>? pipelines);
    }
}
