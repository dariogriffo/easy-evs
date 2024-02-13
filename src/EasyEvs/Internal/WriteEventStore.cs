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

internal sealed class WriteEventStore(
    ISerializer serializer,
    IEventsStreamResolver eventsStreamResolver,
    ILogger<WriteEventStore> logger,
    IConnectionProvider connectionProvider
) : IWriteEventStore
{
    public async Task Append<T>(
        string streamName,
        [NotNull] T @event,
        CancellationToken cancellationToken = default
    )
        where T : IEvent
    {
        logger.LogDebug(
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
        logger.LogDebug(
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
            logger.LogDebug(
                "Appending event with id {EventId} of type {EventType} to stream {Stream}",
                @event.Id,
                @event.GetType(),
                streamName
            );
        }

        StreamState expectedState = StreamState.NoStream;
        EventData[] data = events.Select(serializer.Serialize).ToArray();
        await AppendWithRetryStrategy(streamName, data, expectedState, cancellationToken);
        foreach (T @event in events)
        {
            logger.LogDebug("Event with Id {EventId} added", @event.Id);
        }
    }

    public async Task Append<T>(
        string streamName,
        T[] events,
        CancellationToken cancellationToken = default
    )
        where T : IEvent
    {
        logger.LogDebug("Appending {Count} events to stream {Stream}", events.Length, streamName);
        EventData[] data = events.Select(serializer.Serialize).ToArray();
        StreamState expectedState = StreamState.Any;
        await AppendWithRetryStrategy(streamName, data, expectedState, cancellationToken);
        foreach (T @event in events)
        {
            logger.LogDebug("Event with Id {EventId} added", @event.Id);
        }
    }

    public Task Append<T>([NotNull] T @event, CancellationToken cancellationToken = default)
        where T : IEvent
    {
        string streamName = eventsStreamResolver.StreamForEvent(@event);
        return Append(streamName, @event, cancellationToken);
    }

    private async Task SaveInEventStore(
        string streamName,
        IEvent @event,
        StreamState expectedState,
        CancellationToken cancellationToken
    )
    {
        EventData[] data = [serializer.Serialize(@event)];
        await AppendWithRetryStrategy(streamName, data, expectedState, cancellationToken);
        logger.LogDebug("Event with Id {EventId} appended", @event.Id);
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
            await connectionProvider.WriteClient.AppendToStreamAsync(
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
