namespace EasyEvs.Internal;

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Contracts;
using global::EventStore.Client;
using Microsoft.Extensions.Logging;

internal sealed class InternalPersistentSubscriber(
    ILogger<InternalPersistentSubscriber> logger,
    IConnectionProvider connectionProvider,
    EventStoreSettings settings
) : IInternalPersistentSubscriber
{
    private readonly ConcurrentDictionary<string, IDisposable> _disposables = new();

    private Func<
        PersistentSubscription,
        ResolvedEvent,
        int?,
        CancellationToken,
        Task
    >? _onEventAppeared;

    public async Task Subscribe(
        string streamName,
        Func<PersistentSubscription, ResolvedEvent, int?, CancellationToken, Task> onEventAppeared,
        CancellationToken cancellationToken
    )
    {
        _onEventAppeared ??= onEventAppeared;
        if (_disposables.ContainsKey(streamName))
        {
            return;
        }

        PersistentSubscription sub = null!;
        try
        {
            PersistentSubscriptionSettings settings1 = new(settings.ResolveEvents);

            await connectionProvider.PersistentSubscriptionClient.CreateAsync(
                streamName,
                settings.SubscriptionGroup,
                settings1,
                cancellationToken: cancellationToken
            );

            logger.LogDebug(
                "Created subscription for stream {Stream} with group {Group}",
                streamName,
                settings.SubscriptionGroup
            );
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("AlreadyExists"))
        {
            // Nothing to do here
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error while creating subscription to stream {Stream} with group {Group}",
                streamName,
                settings.SubscriptionGroup
            );
            throw new SubscriptionFailed(streamName);
        }

        logger.LogDebug(
            "Subscribing to stream {Stream} with group {Group}",
            streamName,
            settings.SubscriptionGroup
        );

        try
        {
            sub = await connectionProvider.PersistentSubscriptionClient.SubscribeAsync(
                streamName,
                settings.SubscriptionGroup,
                onEventAppeared,
                OnSubscriptionDropped(streamName),
                bufferSize: settings.SubscriptionBufferSize,
                autoAck: false,
                cancellationToken: cancellationToken
            );
            _disposables.TryAdd(streamName, sub);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error while subscribing to stream {Stream} with group {Group}",
                streamName,
                settings.SubscriptionGroup
            );

            throw new SubscriptionFailed(streamName);
        }

        logger.LogDebug(
            "Subscribed to stream {Stream} with group {Group} id {SubscriptionId}",
            streamName,
            settings.SubscriptionGroup,
            sub.SubscriptionId
        );
    }

    private Action<
        PersistentSubscription,
        SubscriptionDroppedReason,
        Exception?
    > OnSubscriptionDropped(string streamName)
    {
        return (subscription, reason, exception) =>
        {
            if (reason == SubscriptionDroppedReason.Disposed)
            {
                _disposables.TryRemove(streamName, out _);
                return;
            }

            if (settings.ReconnectOnSubscriptionDropped)
            {
                logger.LogWarning(
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
                logger.LogError(
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
