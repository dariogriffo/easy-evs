namespace EasyEvs.Internal
{
    using System.Collections.Generic;
    using global::EventStore.Client;

    internal interface ISerializer
    {
        (IEvent, IReadOnlyDictionary<string, string>) Deserialize(ResolvedEvent resolvedEvent);

        EventData Serialize<T>(T @event, IReadOnlyDictionary<string, string>? eventMetadata = null) where T : IEvent;
    }
}
