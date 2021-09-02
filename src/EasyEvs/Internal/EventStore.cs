namespace EasyEvs.Internal
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using global::EventStore.Client;
    using Microsoft.Extensions.Logging;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Contracts;
    using Grpc.Core;


    internal class EventStore : IEventStore, IAsyncDisposable
    {
        private readonly ISerializer _serializer;
        private readonly IStreamResolver _streamResolver;
        private readonly IHandlesFactory _handlesFactory;
        private readonly ConcurrentBag<IDisposable> _disposables = new ConcurrentBag<IDisposable>();
        private readonly ILogger<EventStore> _logger;
        private readonly EventStoreSettings _settings;
        private readonly Lazy<EventStorePersistentSubscriptionsClient> _persistent;
        private readonly Lazy<EventStoreClient> _write;
        private readonly Lazy<EventStoreClient> _read;

        public EventStore(
            ISerializer serializer,
            IStreamResolver streamResolver,
            IHandlesFactory handlesFactory,
            ILogger<EventStore> logger,
            EventStoreSettings settings)
        {
            _serializer = serializer;
            _streamResolver = streamResolver;
            _logger = logger;
            _settings = settings;
            _handlesFactory = handlesFactory;

            EventStorePersistentSubscriptionsClient PersistentConnectionFactory()
            {
                return new EventStorePersistentSubscriptionsClient(EventStoreClientSettings.Create(settings.ConnectionString));
            }

            EventStoreClient ClientFactory()
            {
                return new EventStoreClient(EventStoreClientSettings.Create(settings.ConnectionString));
            }

            _persistent = new Lazy<EventStorePersistentSubscriptionsClient>(PersistentConnectionFactory);
            _write = new Lazy<EventStoreClient>(ClientFactory);
            _read = new Lazy<EventStoreClient>(ClientFactory);
        }

        public async Task Append<T>(
            [NotNull] T @event,
            IReadOnlyDictionary<string, string>? metadata = default,
            CancellationToken cancellationToken = default)
            where T : IEvent
        {
            var stream = _streamResolver.StreamForEvent(@event);
            _logger.LogDebug($"Appending event with id {@event.Id} of type {@event.GetType()} to stream {stream}");

            var data = _serializer.Serialize(@event, metadata);
            await _write.Value
                    .AppendToStreamAsync(
                        stream,
                        StreamState.Any,
                        new[] { data },
                        cancellationToken: cancellationToken);

            _logger.LogDebug($"Event with id {@event.Id} sent to evs");
        }

        public async Task Append<T>(
            [NotNull] T @event,
            long position,
            IReadOnlyDictionary<string, string>? metadata = default,
            CancellationToken cancellationToken = default) where T : IEvent
        {
            if (position <= 0)
            {
                throw new ArgumentException("position MUST be greater than 0", nameof(position));
            }

            var stream = _streamResolver.StreamForEvent(@event);
            _logger.LogDebug($"Appending event with id {@event.Id} of type {@event.GetType()} to stream {stream} at position {position}");

            var data = _serializer.Serialize(@event, metadata);
            await _write.Value
                    .AppendToStreamAsync(
                        stream,
                        StreamRevision.FromInt64(position),
                        new[] { data },
                        cancellationToken: cancellationToken);

            _logger.LogDebug($"Event with id {@event.Id} sent to evs");
        }

        public async Task Save<T>([NotNull] T aggregateRoot, CancellationToken cancellationToken = default) where T : AggregateRoot
        {
            var changes = aggregateRoot.UncommittedChanges;
            var stream = _streamResolver.StreamForAggregateRoot(aggregateRoot);
            var data = changes.Select(@event => _serializer.Serialize(@event)).ToArray();

            _logger.LogDebug($"Saving {data.Count()} events to stream {stream} for aggregate root {aggregateRoot.Id}");

            await _write.Value
                .AppendToStreamAsync(
                    stream,
                    StreamState.Any,
                    data,
                    cancellationToken: cancellationToken);

            _logger.LogDebug($"Aggregate root {aggregateRoot.Id} saved");
        }

        public async Task<List<(IEvent, IReadOnlyDictionary<string, string>)>> ReadStream(
            [NotNull] string stream,
            long? position = null,
            CancellationToken cancellationToken = default)
        {
            var streamPosition =
                position.HasValue && position.Value > 0 ?
                    StreamPosition.FromInt64(position.Value) :
                    StreamPosition.Start;

            _logger.LogDebug($"Reading events on stream {stream} from position {streamPosition}");

            var result = new List<(IEvent, IReadOnlyDictionary<string, string>)>();
            var events =
                _read
                    .Value
                    .ReadStreamAsync(Direction.Forwards, stream, streamPosition, cancellationToken: cancellationToken);

            await foreach (var @event in events)
            {
                result.Add(_serializer.Deserialize(@event));
            }

            _logger.LogDebug($"{result.Count} events found on stream {stream}");
            return result;
        }

        public Task SubscribeToStream([NotNull] string stream, CancellationToken cancellationToken)
        {
            return InnerSubscribe(stream, _settings.TreatMissingHandlersAsErrors, cancellationToken);
        }

        public async Task<T> Get<T>(Guid id, CancellationToken cancellationToken = default) where T : AggregateRoot, new()
        {
            var streamName = _streamResolver.StreamForAggregateRoot<T>(id);

            _logger.LogDebug($"Loading aggregate root with id {id} from stream {streamName}");
            var data = await ReadStream(streamName, null, cancellationToken);
            var result = new T();
            var history = data.Select(x =>
            {
                var enriched = x.Item1 as IEnrichedEvent;
                enriched!.Metadata = x.Item2;
                return enriched;
            }).ToArray();

            result.LoadFromHistory(history);

            _logger.LogDebug($"Aggregate root with id {id} loaded");
            return result;
        }

        public Task SubscribeToStream(SubscribeCommand command, CancellationToken cancellationToken)
        {
            return InnerSubscribe(command.Stream, command.TreatMissingHandlersAsErrors, cancellationToken);
        }

        public async ValueTask DisposeAsync()
        {
            var t0 = _persistent.IsValueCreated
                ? _persistent.Value.DisposeAsync()
                : ValueTask.CompletedTask;

            var t1 = _write.IsValueCreated
                ? _write.Value.DisposeAsync()
                : ValueTask.CompletedTask;

            var t2 = _read.IsValueCreated
                ? _read.Value.DisposeAsync()
                : ValueTask.CompletedTask;

            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }

            await t0; await t1; await t2;

        }

        private async Task InnerSubscribe(string stream, bool treatMissingHandlersAsErrors, CancellationToken cancellationToken)
        {
            var group = _settings.SubscriptionGroup;
            var connection = _persistent.Value;
            try
            {
                var settings = new PersistentSubscriptionSettings(true);

                await connection.CreateAsync(
                    stream,
                    @group,
                    settings,
                    cancellationToken: cancellationToken);

                _logger.LogDebug($"Created subscription for stream {stream} with group {@group}");
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("AlreadyExists"))
            {
                // Nothing to do here
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while creating subscription to stream {stream} with group {@group}");
                _logger.LogInformation($"Not subscribed to {stream} with group: {@group}");
                return;
            }

            _logger.LogDebug($"Subscribing to stream {stream} with group {@group}");


            var sub = await connection.SubscribeAsync(
                stream,
                @group,
                OnEventAppeared(treatMissingHandlersAsErrors),
                OnSubscriptionDropped(stream),
                bufferSize: _settings.SubscriptionBufferSize,
                autoAck: false,
                cancellationToken: cancellationToken);

            _logger.LogDebug($"Subscribed to stream {stream} with group {@group} id {sub.SubscriptionId}");

            _disposables.Add(sub);
        }

        private Func<PersistentSubscription, ResolvedEvent, int?, CancellationToken, Task> OnEventAppeared(bool treatMissingHandlersAsErrors)
        {
            return async (subscription, resolvedEvent, retryCount, c) =>
            {
                try
                {
                    (IEvent @event, IReadOnlyDictionary<string, string> metadata) = _serializer.Deserialize(resolvedEvent);
                    _logger.LogDebug($"Event with id: {@event.Id} arrived");

                    if (!_handlesFactory.TryGetScopeFor(
                        @event,
                        out var scope))
                    {
                        if (treatMissingHandlersAsErrors)
                        {
                            _logger.LogWarning($"Handler for event of type {@event.GetType()} not found");
                            await subscription.Nack(PersistentSubscriptionNakEventAction.Park,
                                $"Handler for event of type {@event.GetType()} not found", resolvedEvent);
                        }
                        else
                        {
                            await subscription.Ack(resolvedEvent);
                        }

                        return;
                    }

                    var context = new ConsumerContext(Trace.CorrelationManager.ActivityId, metadata, retryCount);

                    async Task<OperationResult> ExecuteActionsAndHandler()
                    {

                        if (_handlesFactory.TryGetPreActionsFor(@event, scope!, out var preActions))
                        {
                            foreach (var action in preActions!)
                            {
                                Task preTask = (((dynamic)action!)!).Execute((dynamic)@event, context, c);
                                await preTask;
                            }
                        }

                        _handlesFactory.TryGetHandlerFor(@event, scope!, out var handler);

                        Task<OperationResult> task = (((dynamic)handler!)!).Handle((dynamic)@event, context, c);
                        var operationResult = await task;

                        if (_handlesFactory.TryGetPostActionsFor(@event, scope!, out var postActions))
                        {
                            foreach (var action in postActions!)
                            {
                                Task<OperationResult> postTask = (((dynamic)action!)!).Execute((dynamic)@event, context, operationResult, c);
                                operationResult = await postTask;
                            }
                        }

                        return operationResult;
                    }

                    OperationResult result = OperationResult.Ok;
                    if (_handlesFactory.TryGetPipelinesFor(@event, scope!, out var pipelines))
                    {
                        Func<Task<OperationResult>>[] reversed = new Func<Task<OperationResult>>[pipelines!.Count + 1];
                        pipelines.Reverse();
                        var length = pipelines.Count;
                        reversed[length] = ExecuteActionsAndHandler;

                        //Let's build the execution tree
                        for (var i = 0; i < length; ++i)
                        {
                            var current = pipelines[i];
                            var next = reversed[length - i];
                            reversed[length - i - 1] = () => (((dynamic)current!)!).Execute((dynamic)@event, context, next, c);
                        }

                        Func<Task<OperationResult>> func = reversed[0];
                        result = await func();
                    }
                    else
                    {
                        result = await ExecuteActionsAndHandler();
                    }

                    scope!.Dispose();
                    _logger.LogDebug($"Event with id: {@event.Id} handled with result {result:G}");
                    await (result switch
                    {
                        OperationResult.Park => subscription.Nack(PersistentSubscriptionNakEventAction.Park, string.Empty, resolvedEvent),
                        OperationResult.Retry => subscription.Nack(PersistentSubscriptionNakEventAction.Retry, string.Empty, resolvedEvent),
                        _ => subscription.Ack(resolvedEvent),
                    });

                }
                catch (Exception ex)
                {
                    await subscription.Nack(PersistentSubscriptionNakEventAction.Park, ex.Message, resolvedEvent);
                    _logger.LogError(ex, $"Error processing event {resolvedEvent} retryCount {retryCount}");
                }
            };
        }

        private Action<PersistentSubscription, SubscriptionDroppedReason, Exception?> OnSubscriptionDropped([NotNull] string stream)
        {
            return (subscription, reason, exception) =>
            {
                if (
                    reason == SubscriptionDroppedReason.Disposed ||
                    // https://github.com/EventStore/EventStore-Client-Dotnet/issues/154
                    (exception?.InnerException is RpcException rpcException &&
                     rpcException.StatusCode == Grpc.Core.StatusCode.Cancelled)
                    )
                {
                    return;
                }

                if (_settings.ReconnectOnSubscriptionDropped)
                {
                    _logger.LogWarning(exception, $"Dropped subscription to stream {stream} with id {subscription.SubscriptionId}. Reason {reason}");
                    SubscribeToStream(stream, CancellationToken.None).GetAwaiter().GetResult();
                }
                else
                {
                    _logger.LogError(exception, $"Dropped subscription to stream {stream} with id {subscription.SubscriptionId}. Reason {reason}");
                }
            };
        }
    }
}
