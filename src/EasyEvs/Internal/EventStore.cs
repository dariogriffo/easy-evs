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


    internal class EventStore : IEventStore, IDisposable
    {
        private readonly ISerializer _serializer;
        private readonly IStreamResolver _streamResolver;
        private readonly IConnectionProvider _connectionProvider;
        private readonly IHandlesFactory _handlesFactory;
        private readonly ConcurrentBag<IDisposable> _disposables = new ConcurrentBag<IDisposable>();
        private readonly ILogger<EventStore> _logger;
        private readonly EventStoreSettings _settings;

        public EventStore(
            IConnectionProvider connectionProvider,
            ISerializer serializer,
            IStreamResolver streamResolver,
            IHandlesFactory handlesFactory,
            ILogger<EventStore> logger,
            EventStoreSettings settings)
        {
            _connectionProvider = connectionProvider;
            _serializer = serializer;
            _streamResolver = streamResolver;
            _logger = logger;
            _settings = settings;
            _handlesFactory = handlesFactory;
        }

        public async Task Append<T>(T @event, IReadOnlyDictionary<string, string>? metadata, CancellationToken cancellationToken)
            where T : IEvent
        {
            var stream = _streamResolver.StreamNameFor(@event);
            _logger.LogDebug($"Appending event with id {@event.Id} of type {@event.GetType()} to stream {stream}");

            var data = _serializer.Serialize(@event, metadata);
            await _connectionProvider
                .GetWriteConnection()
                    .AppendToStreamAsync(
                        stream,
                        StreamState.Any,
                        new[] { data },
                        cancellationToken: cancellationToken);

            _logger.LogDebug($"Event with id {@event.Id} sent to evs");
        }

        public async Task Append<T>(T @event, long position, IReadOnlyDictionary<string, string> metadata = null, CancellationToken cancellationToken = default) where T : IEvent
        {
            if (position <= 0)
            {
                throw new ArgumentException("position MUST be greater than 0", nameof(position));
            }

            var stream = _streamResolver.StreamNameFor(@event);
            _logger.LogDebug($"Appending event with id {@event.Id} of type {@event.GetType()} to stream {stream} at position {position}");

            var data = _serializer.Serialize(@event, metadata);
            await _connectionProvider
                    .GetWriteConnection()
                    .AppendToStreamAsync(
                        stream,
                        StreamRevision.FromInt64(position),
                        new[] { data },
                        cancellationToken: cancellationToken);

            _logger.LogDebug($"Event with id {@event.Id} sent to evs");
        }
        public async Task<List<(IEvent, IReadOnlyDictionary<string, string>)>> ReadStream(string stream, long? position = null, CancellationToken cancellationToken = default)
        {
            var streamPosition =
                position.HasValue && position.Value > 0 ? 
                    StreamPosition.FromInt64(position.Value) : 
                    StreamPosition.Start;
            
            _logger.LogDebug($"Reading events on stream {stream} from position {streamPosition}");

            var result = new List<(IEvent, IReadOnlyDictionary<string, string>)>();
            var events =
                _connectionProvider
                    .GetReadConnection()
                    .ReadStreamAsync(Direction.Forwards, stream, streamPosition, cancellationToken: cancellationToken);

            await foreach (var @event in events)
            {
                result.Add(_serializer.Deserialize(@event));
            }

            _logger.LogDebug($"{result.Count} events found on stream {stream}");
            return result;
        }

        public async Task SubscribeToStream(string stream, CancellationToken cancellationToken)
        {
            var group = _settings.SubscriptionGroup;
            var connection = _connectionProvider.GetPersistentReadConnection();
            try
            {
                var settings = new PersistentSubscriptionSettings(_settings.ResolveLinkTos);

                await connection.CreateAsync(
                    stream,
                    group,
                    settings,
                    cancellationToken: cancellationToken);

                _logger.LogDebug($"Created subscription for stream {stream} with group {group}");
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("AlreadyExists"))
            {
                // Nothing to do here
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while creating subscription to stream {stream} with group {group}");
                _logger.LogInformation($"Not subscribed to {stream} with group: {group}");
                return;
            }

            _logger.LogDebug($"Subscribing to stream {stream} with group {group}");

            var sub = await connection.SubscribeAsync(
                stream,
                group,
                OnEventAppeared,
                OnSubscriptionDropped(stream),
                bufferSize: _settings.SubscriptionBufferSize,
                cancellationToken: cancellationToken);

            _logger.LogDebug($"Subscribed to stream {stream} with group {group} id {sub.SubscriptionId}");

            _disposables.Add(sub);
        }

        private Action<PersistentSubscription, SubscriptionDroppedReason, Exception> OnSubscriptionDropped(string stream)
        {
            return (subscription, reason, exception) =>
            {
                if (_settings.ReconnectOnSubscriptionDropped)
                {
                    _logger.LogWarning(exception, $"Dropped subscription to stream {stream} with id {subscription.SubscriptionId}");
                    SubscribeToStream(stream, CancellationToken.None).GetAwaiter().GetResult();
                }
                else
                {
                    _logger.LogError(exception, $"Dropped subscription to stream {stream} with id {subscription.SubscriptionId}");
                }
            };
        }

        private async Task OnEventAppeared(PersistentSubscription subscription, ResolvedEvent resolvedEvent, int? retryCount, CancellationToken c)
        {
            try
            {
                (IEvent @event, IReadOnlyDictionary<string, string> metadata) = _serializer.Deserialize(resolvedEvent);
                _logger.LogDebug($"Event with id: {@event.Id} arrived");

                if (!_handlesFactory.TryGetHandlerFor(@event, out var handler, out var scope))
                {
                    _logger.LogDebug($"Handler for event of type {@event.GetType()} not found");
                    await subscription.Nack(PersistentSubscriptionNakEventAction.Park, $"Handler for event of type {@event.GetType()} not found", resolvedEvent);
                    return;
                }

                var context = new ConsumerContext(Trace.CorrelationManager.ActivityId, metadata, retryCount);
                Task<OperationResult> task = ((dynamic)handler).Handle((dynamic)@event, context, c);
                var result = await task;
                scope.Dispose();
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
        }

        public void Dispose()
        {
            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }
        }
    }
}
