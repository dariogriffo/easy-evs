namespace EasyEvs.Internal;

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Contracts.Exceptions;
using global::EventStore.Client;
using Grpc.Core;
using Microsoft.Extensions.Logging;

internal sealed class InternalPersistentSubscriber : IInternalPersistentSubscriber
{
    private readonly ConcurrentDictionary<string, IDisposable> _disposables = new();
    private readonly IConnectionStrategy _connectionStrategy;

    private Func<
        PersistentSubscription,
        ResolvedEvent,
        int?,
        CancellationToken,
        Task
    >? _onEventAppeared;

    private readonly ILogger<InternalPersistentSubscriber> _logger;
    private readonly IConnectionProvider _connectionProvider;
    private readonly EventStoreSettings _settings;

    public InternalPersistentSubscriber(
        ILogger<InternalPersistentSubscriber> logger,
        IConnectionProvider connectionProvider,
        IConnectionStrategy connectionStrategy,
        EventStoreSettings settings)
    {
        _logger = logger;
        _connectionProvider = connectionProvider;
        _connectionStrategy = connectionStrategy;
        _settings = settings;
    }

    public async Task Subscribe(
        string streamName,
        Func<PersistentSubscription, ResolvedEvent, int?, CancellationToken, Task> onEventAppeared,
        CancellationToken cancellationToken
    )
    {
#pragma warning disable CA2208
        _ = _settings.SubscriptionGroup ?? throw new ArgumentNullException(nameof(_settings.SubscriptionGroup));
#pragma warning restore CA2208

        _onEventAppeared ??= onEventAppeared;
        if (_disposables.ContainsKey(streamName))
        {
            return;
        }
        
        await _connectionStrategy.Execute(DoSubscribe, cancellationToken);
        return;


        async Task DoSubscribe(CancellationToken c)
        {
            PersistentSubscription sub = null!;
            try
            {
                PersistentSubscriptionSettings settings1 = new(_settings.ResolveEvents);

                await _connectionProvider.PersistentSubscriptionClient.CreateToStreamAsync(
                    streamName,
                    _settings.SubscriptionGroup,
                    settings1,
                    cancellationToken: c
                );

                _logger.LogDebug(
                    "Created subscription for stream {Stream} with group {Group}",
                    streamName,
                    _settings.SubscriptionGroup
                );
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.AlreadyExists)
            {
                int i = 0;
                ++i;
                // Nothing to do here
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while creating subscription to stream {Stream} with group {Group}",
                    streamName,
                    _settings.SubscriptionGroup
                );
                throw new SubscriptionFailed(streamName);
            }

            _logger.LogDebug(
                "Subscribing to stream {Stream} with group {Group}",
                streamName,
                _settings.SubscriptionGroup
            );

            try
            {
                sub = await _connectionProvider.PersistentSubscriptionClient.SubscribeToStreamAsync(
                    streamName,
                    _settings.SubscriptionGroup,
                    onEventAppeared,
                    OnSubscriptionDropped(streamName),
                    bufferSize: _settings.SubscriptionBufferSize,
                    cancellationToken: cancellationToken
                );
                _disposables.TryAdd(streamName, sub);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while subscribing to stream {Stream} with group {Group}",
                    streamName,
                    _settings.SubscriptionGroup
                );

                throw new SubscriptionFailed(streamName);
            }

            _logger.LogDebug(
                "Subscribed to stream {Stream} with group {Group} id {SubscriptionId}",
                streamName,
                _settings.SubscriptionGroup,
                sub.SubscriptionId
            );
        }
    }

    private Action<
        PersistentSubscription,
        SubscriptionDroppedReason,
        Exception?
    > OnSubscriptionDropped(string streamName)
    {
        return (subscription, reason, exception) =>
        {
            _disposables.TryRemove(streamName, out var s);

            if (reason == SubscriptionDroppedReason.Disposed)
            {
                return;
            }

            try
            {
                s?.Dispose();
            }
            catch
            {
                // ignored
            }

            if (_settings.ReconnectOnSubscriptionDropped)
            {
                _logger.LogWarning(
                    exception,
                    "Dropped subscription to stream {Stream} with id {SubscriptionId}. Reason {Reason}",
                    streamName,
                    subscription.SubscriptionId,
                    reason
                );
                Subscribe(streamName, _onEventAppeared!, CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
            }
            else
            {
                _logger.LogError(
                    exception,
                    "Dropped subscription to stream {Stream} with id {SubscriptionId}. Reason {Reason}",
                    streamName,
                    subscription.SubscriptionId,
                    reason
                );
            }
        };
    }
}
