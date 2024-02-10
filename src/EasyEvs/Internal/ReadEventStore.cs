namespace EasyEvs.Internal;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Contracts.Exceptions;
using global::EventStore.Client;
using Microsoft.Extensions.Logging;

internal sealed class ReadEventStore : IReadEventStore
{
    private readonly ISerializer _serializer;
    private readonly ILogger<ReadEventStore> _logger;
    private readonly IConnectionProvider _connectionProvider;
    private readonly EventStoreSettings _settings;

    public ReadEventStore(
        ISerializer serializer,
        ILogger<ReadEventStore> logger,
        IConnectionProvider connectionProvider,
        EventStoreSettings settings
    )
    {
        _serializer = serializer;
        _logger = logger;
        _connectionProvider = connectionProvider;
        _settings = settings;
    }

    public async Task<List<IEvent>> ReadStream(
        string streamName,
        CancellationToken cancellationToken = default
    )
    {
        StreamPosition streamNamePosition = StreamPosition.Start;

        _logger.LogDebug("Reading events on stream {Stream}", streamName);

        List<IEvent> result = new();
        EventStoreClient client = _connectionProvider.ReadClient;
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
                if (@event.IsResolved && !_settings.ResolveEvents)
                {
                    continue;
                }

                result.Add(_serializer.Deserialize(@event.OriginalEvent));
            }

            _logger.LogDebug("{Count} events found on stream {Stream}", result.Count, streamName);
        }
        catch (StreamNotFoundException)
        {
            throw new StreamNotFound(streamName);
        }

        return result;
    }
}
