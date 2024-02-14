namespace EasyEvs.Internal;

using Contracts;
using global::EventStore.Client;

internal interface ISerializer
{
    IEvent Deserialize(EventRecord record);

    EventData Serialize<T>(T @event)
        where T : IEvent;
}
