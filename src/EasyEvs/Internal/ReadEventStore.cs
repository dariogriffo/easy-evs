namespace EasyEvs.Internal;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Contracts.Exceptions;
using global::EventStore.Client;
using Microsoft.Extensions.Logging;

internal sealed class ReadEventStore(
    ISerializer serializer,
    ILogger<ReadEventStore> logger,
    IConnectionProvider connectionProvider,
    EventStoreSettings settings
) : IReadEventStore
{
    public async Task<List<IEvent>> ReadStream(
        string streamName,
        IEvent? lastEventToRead = default,
        CancellationToken cancellationToken = default
    )
    {
        StreamPosition streamNamePosition = StreamPosition.Start;

        logger.LogDebug("Reading events on stream {Stream}", streamName);

        List<IEvent> result = new();
        EventStoreClient client = connectionProvider.ReadClient;
        try
        {
            EventStoreClient.ReadStreamResult events = client.ReadStreamAsync(
                Direction.Forwards,
                streamName,
                streamNamePosition,
                cancellationToken: cancellationToken
            );

            await foreach (ResolvedEvent @event in events)
            {
                if (@event.IsResolved && !settings.ResolveEvents)
                {
                    continue;
                }

                IEvent item = serializer.Deserialize(@event.OriginalEvent);
                if (
                    lastEventToRead is null
                    || (item.Timestamp < lastEventToRead.Timestamp || item.Id == lastEventToRead.Id)
                )
                {
                    result.Add(item);
                    if (lastEventToRead is not null && item.Id == lastEventToRead.Id)
                    {
                        break;
                    }
                }
            }

            logger.LogDebug("{Count} events found on stream {Stream}", result.Count, streamName);
        }
        catch (StreamNotFoundException)
        {
            throw new StreamNotFound(streamName);
        }

        return result;
    }
}
