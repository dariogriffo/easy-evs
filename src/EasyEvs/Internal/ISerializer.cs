﻿namespace EasyEvs.Internal
{
    using System.Collections.Generic;
    using Contracts;
    using global::EventStore.Client;

    internal interface ISerializer
    {
        IEvent Deserialize(ResolvedEvent resolvedEvent);

        EventData Serialize<T>(T @event) where T : IEvent;
    }
}
