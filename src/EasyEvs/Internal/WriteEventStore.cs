namespace EasyEvs.Internal;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Contracts.Exceptions;
using global::EventStore.Client;
using Microsoft.Extensions.Logging;

internal sealed class WriteEventStore : IWriteEventStore
{
    private readonly ISerializer _serializer;
    private readonly IEventsStreamResolver _eventsStreamResolver;
    private readonly ILogger<WriteEventStore> _logger;
    private readonly IConnectionProvider _connectionProvider;

    public WriteEventStore(
        ISerializer serializer,
        IEventsStreamResolver eventsStreamResolver,
        ILogger<WriteEventStore> logger,
        IConnectionProvider connectionProvider
    )
    {
        _serializer = serializer;
        _eventsStreamResolver = eventsStreamResolver;
        _logger = logger;
        _connectionProvider = connectionProvider;
    }

    public async Task Append<T>(
        string streamName,
        [NotNull] T @event,
        CancellationToken cancellationToken = default
    )
        where T : IEvent
    {
        _logger.LogDebug(
            "Appending event with id {EventId} of type {EventType} to stream {Stream}",
            @event.Id,
            @event.GetType(),
            streamName
        );

        StreamState expectedState = StreamState.Any;
        try
        {
            await SaveInEventStore(streamName, @event, expectedState, cancellationToken);
        }
        catch (Exception ex) when (ex is not StreamAlreadyExists)
        {
            throw new ErrorAppendingEventToStream(ex, @event, streamName);
        }
    }

    public Task Store<T>(
        string streamName,
        [NotNull] T @event,
        CancellationToken cancellationToken = default
    )
        where T : IEvent
    {
        _logger.LogDebug(
            "Appending event with id {EventId} of type {EventType} to stream {Stream}",
            @event.Id,
            @event.GetType(),
            streamName
        );

        StreamState expectedState = StreamState.Any;
        return SaveInEventStore(streamName, @event, expectedState, cancellationToken);
    }

    public async Task Store<T>(
        string streamName,
        T[] events,
        CancellationToken cancellationToken = default
    )
        where T : IEvent
    {
        foreach (T @event in events)
        {
            _logger.LogDebug(
                "Appending event with id {EventId} of type {EventType} to stream {Stream}",
                @event.Id,
                @event.GetType(),
                streamName
            );
        }

        StreamState expectedState = StreamState.NoStream;
        EventData[] data = events.Select(_serializer.Serialize).ToArray();
        await AppendWithRetryStrategy(streamName, data, expectedState, cancellationToken);
        foreach (T @event in events)
        {
            _logger.LogDebug("Event with Id {EventId} added", @event.Id);
        }
    }

    public async Task Append<T>(
        string streamName,
        T[] events,
        CancellationToken cancellationToken = default
    )
        where T : IEvent
    {
        _logger.LogDebug("Appending {Count} events to stream {Stream}", events.Length, streamName);
        EventData[] data = events.Select(_serializer.Serialize).ToArray();
        StreamState expectedState = StreamState.Any;
        await AppendWithRetryStrategy(streamName, data, expectedState, cancellationToken);
        foreach (T @event in events)
        {
            _logger.LogDebug("Event with Id {EventId} added", @event.Id);
        }
    }

    public Task Append<T>([NotNull] T @event, CancellationToken cancellationToken = default)
        where T : IEvent
    {
        string streamName = _eventsStreamResolver.StreamForEvent(@event);
        return Append(streamName, @event, cancellationToken);
    }

    private async Task SaveInEventStore(
        string streamName,
        IEvent @event,
        StreamState expectedState,
        CancellationToken cancellationToken
    )
    {
        EventData[] data = [_serializer.Serialize(@event)];
        await AppendWithRetryStrategy(streamName, data, expectedState, cancellationToken);
        _logger.LogDebug("Event with Id {EventId} appended", @event.Id);
    }

    private async Task AppendWithRetryStrategy(
        string streamName,
        EventData[] data,
        StreamState expectedState,
        CancellationToken cancellationToken
    )
    {
        try
        {
            await _connectionProvider.WriteClient.AppendToStreamAsync(
                streamName,
                expectedState,
                data,
                cancellationToken: cancellationToken
            );
        }
        catch (WrongExpectedVersionException ex)
            when (expectedState == StreamState.NoStream
                && ex.ExpectedStreamRevision == StreamRevision.None
            )
        {
            throw new StreamAlreadyExists(streamName);
        }
    }
}
