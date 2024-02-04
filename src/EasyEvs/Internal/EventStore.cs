namespace EasyEvs.Internal;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Contracts;
using global::EventStore.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

internal sealed class EventStore : IEventStore, IAsyncDisposable
{
    private readonly ISerializer _serializer;
    private readonly IEventsStreamResolver _eventsStreamResolver;
    private readonly IHandlesFactory _handlesFactory;
    private readonly ConcurrentBag<IDisposable> _disposables = new();
    private readonly ILogger<EventStore> _logger;
    private readonly IRetryStrategy _retryStrategy;
    private readonly EventStoreSettings _settings;
    private Lazy<EventStorePersistentSubscriptionsClient> _persistent;
    private Lazy<EventStoreClient> _write;
    private Lazy<EventStoreClient> _read;

    public EventStore(
        ISerializer serializer,
        IEventsStreamResolver eventsStreamResolver,
        IHandlesFactory handlesFactory,
        ILogger<EventStore> logger,
        IRetryStrategy retryStrategy,
        EventStoreSettings settings
    )
    {
        _serializer = serializer;
        _eventsStreamResolver = eventsStreamResolver;
        _logger = logger;
        _retryStrategy = retryStrategy;
        _settings = settings;
        _handlesFactory = handlesFactory;

        _persistent = new Lazy<EventStorePersistentSubscriptionsClient>(
            PersistentConnectionFactory
        );
        _write = new Lazy<EventStoreClient>(ClientFactory);
        _read = new Lazy<EventStoreClient>(ClientFactory);
    }

    public async Task Append<T>(
        string stream,
        [NotNull] T @event,
        CancellationToken cancellationToken = default
    )
        where T : IEvent
    {
        _logger.LogDebug(
            "Appending event with id {EventId} of type {EventType} to stream {Stream}",
            @event.Id,
            @event.GetType(),
            stream
        );

        EventData data = _serializer.Serialize(@event);
        StreamState expectedState = StreamState.Any;
        await SaveInEventStore(stream, data, expectedState, cancellationToken);

        _logger.LogDebug("Event with Id {EventId} appended", @event.Id);
    }

    public async Task Store<T>(
        string stream,
        [NotNull] T @event,
        CancellationToken cancellationToken = default
    )
        where T : IEvent
    {
        _logger.LogDebug(
            "Saving event with id {EventId} of type {EventType} to stream {Stream}",
            @event.Id,
            @event.GetType(),
            stream
        );

        EventData data = _serializer.Serialize(@event);

        StreamState expectedState = StreamState.Any;
        await SaveInEventStore(stream, data, expectedState, cancellationToken);

        _logger.LogDebug("Event with Id {EventId} saved", @event.Id);
    }

    public Task Store<T>(string stream, T[] events, CancellationToken cancellationToken = default)
        where T : IEvent
    {
        _logger.LogDebug("Adding {Count} events to new stream {Stream}", events.Length, stream);
        EventData[] data = events.Select(_serializer.Serialize).ToArray();
        StreamState expectedState = StreamState.NoStream;
        return SaveInEventStore(stream, data, expectedState, cancellationToken);
    }

    public Task Append<T>(string stream, T[] events, CancellationToken cancellationToken = default)
        where T : IEvent
    {
        _logger.LogDebug("Appending {Count} events to stream {Stream}", events.Length, stream);
        EventData[] data = events.Select(_serializer.Serialize).ToArray();
        StreamState expectedState = StreamState.Any;
        return SaveInEventStore(stream, data, expectedState, cancellationToken);
    }

    public Task Append<T>([NotNull] T @event, CancellationToken cancellationToken = default)
        where T : IEvent
    {
        string stream = _eventsStreamResolver.StreamForEvent(@event);
        return Append(stream, @event, cancellationToken);
    }

    public async Task<List<IEvent>> ReadStream(
        string stream,
        CancellationToken cancellationToken = default
    )
    {
        StreamPosition streamPosition = StreamPosition.Start;

        _logger.LogDebug("Reading events on stream {Stream}", stream);

        List<IEvent> result = new();
        await _retryStrategy.Read(async () =>
        {
            EventStoreClient client = _read.Value;
            EventStoreClient.ReadStreamResult events = client.ReadStreamAsync(
                Direction.Forwards,
                stream,
                streamPosition,
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

            _logger.LogDebug("{Count} events found on stream {Stream}", result.Count, stream);
        });
        return result;
    }

    public Task SubscribeToStream(string stream, CancellationToken cancellationToken)
    {
        return InnerSubscribe(stream, _settings.TreatMissingHandlersAsErrors, cancellationToken);
    }

    public Task SubscribeToStream(SubscribeCommand command, CancellationToken cancellationToken)
    {
        return InnerSubscribe(
            command.Stream,
            command.TreatMissingHandlersAsErrors,
            cancellationToken
        );
    }

    public async ValueTask DisposeAsync()
    {
        ValueTask t0 = _persistent.IsValueCreated
            ? _persistent.Value.DisposeAsync()
            : ValueTask.CompletedTask;

        ValueTask t1 = _write.IsValueCreated
            ? _write.Value.DisposeAsync()
            : ValueTask.CompletedTask;

        ValueTask t2 = _read.IsValueCreated ? _read.Value.DisposeAsync() : ValueTask.CompletedTask;

        foreach (IDisposable disposable in _disposables)
        {
            disposable.Dispose();
        }

        await t0;
        await t1;
        await t2;
    }

    private async Task InnerSubscribe(
        string stream,
        bool treatMissingHandlersAsErrors,
        CancellationToken cancellationToken
    )
    {
        await _retryStrategy.Subscribe(async () =>
        {
            string group = _settings.SubscriptionGroup!;
            EventStorePersistentSubscriptionsClient connection = _persistent.Value;
            try
            {
                PersistentSubscriptionSettings settings = new(_settings.ResolveEvents);

                await connection.CreateAsync(
                    stream,
                    @group,
                    settings,
                    cancellationToken: cancellationToken
                );

                _logger.LogDebug(
                    "Created subscription for stream {Stream} with group {Group}",
                    stream,
                    group
                );
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("AlreadyExists"))
            {
                // Nothing to do here
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while creating subscription to stream {Stream} with group {Group}",
                    stream,
                    group
                );
                _logger.LogInformation(
                    "Not subscribed to {Stream} with group: {Group}",
                    stream,
                    group
                );
                throw;
            }

            _logger.LogDebug("Subscribing to stream {Stream} with group {Group}", stream, group);

            PersistentSubscription sub = await connection.SubscribeAsync(
                stream,
                group,
                OnEventAppeared(treatMissingHandlersAsErrors),
                OnSubscriptionDropped(stream),
                bufferSize: _settings.SubscriptionBufferSize,
                autoAck: false,
                cancellationToken: cancellationToken
            );

            _logger.LogDebug(
                "Subscribed to stream {Stream} with group {Group} id {SubscriptionId}",
                stream,
                group,
                sub.SubscriptionId
            );

            _disposables.Add(sub);
        });
    }

    private Func<
        PersistentSubscription,
        ResolvedEvent,
        int?,
        CancellationToken,
        Task
    > OnEventAppeared(bool treatMissingHandlersAsErrors)
    {
        return async (subscription, resolvedEvent, retryCount, c) =>
        {
            if (@resolvedEvent.IsResolved && !_settings.ResolveEvents)
            {
                await subscription.Ack(resolvedEvent);
            }

            try
            {
                IEvent @event = _serializer.Deserialize(resolvedEvent.OriginalEvent);
                _logger.LogDebug("Event with id {EventId} arrived", @event.Id);

                if (!_handlesFactory.TryGetScopeFor(@event, out IServiceScope? scope))
                {
                    if (treatMissingHandlersAsErrors)
                    {
                        _logger.LogWarning(
                            "Handler for event of type {EventType} not found",
                            @event.GetType()
                        );
                        await subscription.Nack(
                            PersistentSubscriptionNakEventAction.Park,
                            $"Handler for event of type {@event.GetType()} not found",
                            resolvedEvent
                        );
                    }
                    else
                    {
                        await subscription.Ack(resolvedEvent);
                    }

                    return;
                }

                ConsumerContext context = new(Trace.CorrelationManager.ActivityId, retryCount);

                dynamic dynamicEvent = @event;

                async Task<OperationResult> ExecuteActionsAndHandler()
                {
                    if (
                        _handlesFactory.TryGetPreActionsFor(
                            @event,
                            scope!,
                            out List<dynamic>? preActions
                        )
                    )
                    {
                        foreach (var action in preActions!)
                        {
                            Task preTask = action.Execute(dynamicEvent, context, c);
                            await preTask;
                        }
                    }

                    _handlesFactory.TryGetHandlerFor(@event, scope!, out dynamic? handler);

                    Task<OperationResult> task = (handler!).Handle(dynamicEvent, context, c);
                    OperationResult operationResult = await task;

                    if (
                        _handlesFactory.TryGetPostActionsFor(
                            @event,
                            scope!,
                            out List<dynamic>? postActions
                        )
                    )
                    {
                        foreach (var action in postActions!)
                        {
                            Task<OperationResult> postTask = action.Execute(
                                dynamicEvent,
                                context,
                                operationResult,
                                c
                            );
                            operationResult = await postTask;
                        }
                    }

                    return operationResult;
                }

                OperationResult result = OperationResult.Ok;
                if (
                    _handlesFactory.TryGetPipelinesFor(@event, scope!, out List<dynamic>? pipelines)
                )
                {
                    Func<Task<OperationResult>>[] reversed = new Func<Task<OperationResult>>[
                        pipelines!.Count + 1
                    ];
                    pipelines.Reverse();
                    int length = pipelines.Count;
                    reversed[length] = ExecuteActionsAndHandler;

                    //Let's build the execution tree
                    for (int i = 0; i < length; ++i)
                    {
                        dynamic current = pipelines[i];
                        Func<Task<OperationResult>> next = reversed[length - i];
                        reversed[length - i - 1] = () =>
                            current.Execute(dynamicEvent, context, next, c);
                    }

                    Func<Task<OperationResult>> func = reversed[0];
                    result = await func();
                }
                else
                {
                    result = await ExecuteActionsAndHandler();
                }

                scope!.Dispose();
                _logger.LogDebug(
                    "Event with id: {EventId} handled with result {Result:G}",
                    @event.Id,
                    result
                );
                await (
                    result switch
                    {
                        OperationResult.Park
                            => subscription.Nack(
                                PersistentSubscriptionNakEventAction.Park,
                                string.Empty,
                                resolvedEvent
                            ),
                        OperationResult.Retry
                            => subscription.Nack(
                                PersistentSubscriptionNakEventAction.Retry,
                                string.Empty,
                                resolvedEvent
                            ),
                        _ => subscription.Ack(resolvedEvent),
                    }
                );
            }
            catch (Exception ex)
            {
                await subscription.Nack(
                    PersistentSubscriptionNakEventAction.Park,
                    ex.Message,
                    resolvedEvent
                );
                _logger.LogError(
                    ex,
                    "Error processing event {ResolvedEvent} retryCount {RetryCount}",
                    resolvedEvent,
                    retryCount
                );
            }
        };
    }

    private Action<
        PersistentSubscription,
        SubscriptionDroppedReason,
        Exception?
    > OnSubscriptionDropped(string stream)
    {
        return (subscription, reason, exception) =>
        {
            if (reason == SubscriptionDroppedReason.Disposed)
            {
                return;
            }

            if (_settings.ReconnectOnSubscriptionDropped)
            {
                _logger.LogWarning(
                    exception,
                    "Dropped subscription to stream {Stream} with id {SubscriptionId}. Reason {Reason}",
                    stream,
                    subscription.SubscriptionId,
                    reason
                );
                SubscribeToStream(stream, CancellationToken.None).GetAwaiter().GetResult();
            }
            else
            {
                _logger.LogError(
                    exception,
                    "Dropped subscription to stream {Stream} with id {SubscriptionId}. Reason {Reason}",
                    stream,
                    subscription.SubscriptionId,
                    reason
                );
            }
        };
    }

    private EventStorePersistentSubscriptionsClient PersistentConnectionFactory()
    {
        return new EventStorePersistentSubscriptionsClient(
            EventStoreClientSettings.Create(_settings.ConnectionString)
        );
    }

    private EventStoreClient ClientFactory()
    {
        return new EventStoreClient(EventStoreClientSettings.Create(_settings.ConnectionString));
    }

    private async Task ReconnectWrite(Exception _)
    {
        try
        {
            await _write.Value.DisposeAsync();
        }
        catch (Exception)
        {
            // ignored
        }

        _write = new Lazy<EventStoreClient>(ClientFactory);
    }

    private async Task ReconnectSubscription(Exception ex)
    {
        try
        {
            await _persistent.Value.DisposeAsync();
        }
        catch (Exception)
        {
            // ignored
        }

        _persistent = new Lazy<EventStorePersistentSubscriptionsClient>(
            PersistentConnectionFactory
        );
    }

    private async Task ReconnectRead(Exception ex)
    {
        try
        {
            await _read.Value.DisposeAsync();
        }
        catch (Exception)
        {
            // ignored
        }

        _read = new Lazy<EventStoreClient>(ClientFactory);
    }

    private async Task SaveInEventStore(
        string stream,
        EventData data,
        StreamState expectedState,
        CancellationToken cancellationToken
    )
    {
        await _retryStrategy.Write(async () =>
        {
            try
            {
                await _write.Value.AppendToStreamAsync(
                    stream,
                    expectedState,
                    new[] { data },
                    cancellationToken: cancellationToken
                );
            }
            catch (WrongExpectedVersionException ex)
                when (ex.ExpectedStreamRevision == StreamRevision.None)
            {
                throw new StreamAlreadyExists(stream);
            }
        });
    }

    private async Task SaveInEventStore(
        string stream,
        EventData[] data,
        StreamState expectedState,
        CancellationToken cancellationToken
    )
    {
        await _retryStrategy.Write(async () =>
        {
            try
            {
                await _write.Value.AppendToStreamAsync(
                    stream,
                    expectedState,
                    data,
                    cancellationToken: cancellationToken
                );
            }
            catch (WrongExpectedVersionException ex)
                when (ex.ExpectedStreamRevision == StreamRevision.None)
            {
                throw new StreamAlreadyExists(stream);
            }
        });
    }
}
