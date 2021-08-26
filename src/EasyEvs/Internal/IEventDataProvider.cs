namespace EasyEvs.Internal
{
    using System;

    internal interface IEventDataProvider
    {
        Type EventTypeFrom(global::EventStore.Client.ResolvedEvent @event);
    }
}
