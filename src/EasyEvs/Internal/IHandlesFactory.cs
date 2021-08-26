namespace EasyEvs.Internal
{
    using System;

    internal interface IHandlesFactory
    {
        bool TryGetHandlerFor(IEvent @event, out IHandlesEvent? handler, out IDisposable? scope);
    }
}
