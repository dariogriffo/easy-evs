namespace EasyEvs.Internal;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Contracts.Exceptions;
using global::EventStore.Client;
using Grpc.Core;
using Microsoft.Extensions.Logging;

internal sealed class ReadEventStore : IReadEventStore
{
    private readonly ISerializer _serializer;
    private readonly ILogger<ReadEventStore> _logger;
    private readonly IConnectionProvider _connectionProvider;
    private readonly IConnectionStrategy _connectionStrategy;
    private readonly EventStoreSettings _settings;

    public ReadEventStore(
        ISerializer serializer,
        ILogger<ReadEventStore> logger,
        IConnectionProvider connectionProvider,
        IConnectionStrategy connectionStrategy,
        EventStoreSettings settings
    )
    {
        _serializer = serializer;
        _logger = logger;
        _connectionProvider = connectionProvider;
        _settings = settings;
        _connectionStrategy = connectionStrategy;
    }

    public async Task<List<IEvent>> ReadStream(
        string streamName,
        CancellationToken cancellationToken = default
    )
    {
        StreamPosition streamNamePosition = StreamPosition.Start;

        _logger.LogDebug("Reading events on stream {Stream}", streamName);
        List<IEvent> result = new();
        await _connectionStrategy.Execute(DoRead, cancellationToken);
        return result;

        async Task DoRead(CancellationToken c)
        {
            try
            {
                EventStoreClient.ReadStreamResult events =
                    _connectionProvider.ReadClient.ReadStreamAsync(
                        Direction.Forwards,
                        streamName,
                        streamNamePosition,
                        cancellationToken: c
                    );

                var i = 0;

                await foreach (ResolvedEvent @event in events)
                {
                    if (@event.IsResolved && !_settings.ResolveEvents)
                    {
                        continue;
                    }

                    if (i != result.Count)
                    {
                        continue;
                    }

                    IEvent item = _serializer.Deserialize(@event.OriginalEvent);
                    result.Add(item);
                    ++i;
                }

                _logger.LogDebug(
                    "{Count} events found on stream {Stream}",
                    result.Count,
                    streamName
                );
            }
            catch (StreamNotFoundException)
            {
                throw new StreamNotFound(streamName);
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
            {
                await _connectionProvider.WriteClientDisconnected(_connectionProvider.WriteClient);
                throw new ConnectionFailureException();
            }
        }
    }

    public async Task<List<IEvent>> ReadStream(
        string streamName,
        IEvent? lastEventToRead = default,
        CancellationToken cancellationToken = default
    )
    {
        StreamPosition streamNamePosition = StreamPosition.Start;

        _logger.LogDebug("Reading events on stream {Stream}", streamName);

        List<IEvent> result = new();
        await _connectionStrategy.Execute(DoRead, cancellationToken);

        return result;

        async Task DoRead(CancellationToken c)
        {
            try
            {
                EventStoreClient.ReadStreamResult events =
                    _connectionProvider.ReadClient.ReadStreamAsync(
                        Direction.Forwards,
                        streamName,
                        streamNamePosition,
                        cancellationToken: c
                    );

                await foreach (ResolvedEvent @event in events)
                {
                    if (@event.IsResolved && !_settings.ResolveEvents)
                    {
                        continue;
                    }

                    IEvent item = _serializer.Deserialize(@event.OriginalEvent);
                    if (
                        lastEventToRead is null
                        || (
                            item.Timestamp < lastEventToRead.Timestamp
                            || item.Id == lastEventToRead.Id
                        )
                    )
                    {
                        var i = 0;
                        if (i != result.Count)
                        {
                            continue;
                        }

                        result.Add(item);
                        ++i;

                        if (lastEventToRead is not null && item.Id == lastEventToRead.Id)
                        {
                            break;
                        }
                    }
                }

                _logger.LogDebug(
                    "{Count} events found on stream {Stream}",
                    result.Count,
                    streamName
                );
            }
            catch (StreamNotFoundException)
            {
                throw new StreamNotFound(streamName);
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
            {
                await _connectionProvider.WriteClientDisconnected(_connectionProvider.WriteClient);
                throw new ConnectionFailureException();
            }
        }
    }
}
