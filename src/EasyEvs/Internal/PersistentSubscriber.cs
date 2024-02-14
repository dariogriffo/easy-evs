namespace EasyEvs.Internal;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Contracts;
using global::EventStore.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

internal class PersistentSubscriber : IPersistentSubscriber
{
    private readonly ILogger<PersistentSubscriber> _logger;
    private readonly IInternalPersistentSubscriber _subscriber;
    private readonly IHandlesFactory _handlesFactory;
    private readonly ISerializer _serializer;
    private readonly IOptions<EasyEvsConfiguration> _easyEvsConfiguration;
    private readonly EventStoreSettings _settings;

    public PersistentSubscriber(
        ILogger<PersistentSubscriber> logger,
        IInternalPersistentSubscriber subscriber,
        IHandlesFactory handlesFactory,
        ISerializer serializer,
        IOptions<EasyEvsConfiguration> easyEvsConfiguration,
        EventStoreSettings settings
    )
    {
        _logger = logger;
        _subscriber = subscriber;
        _handlesFactory = handlesFactory;
        _serializer = serializer;
        _easyEvsConfiguration = easyEvsConfiguration;
        _settings = settings;
    }

    private async Task OnEventAppeared(
        PersistentSubscription subscription,
        ResolvedEvent resolvedEvent,
        int? retryCount,
        CancellationToken cancellationToken
    )
    {
        if (resolvedEvent.IsResolved && !_settings.ResolveEvents)
        {
            await subscription.Ack(resolvedEvent);
        }

        try
        {
            IEvent @event = _serializer.Deserialize(resolvedEvent.Event);
            _logger.LogDebug("Event with id {EventId} arrived", @event.Id);

            if (!_handlesFactory.TryGetScopeFor(@event, out IServiceScope? scope))
            {
                if (_easyEvsConfiguration.Value.TreatMissingHandlersAsErrors)
                {
                    await ParkEventAndLogWarning(@event, subscription, resolvedEvent);
                }
                else
                {
                    _logger.LogDebug("Event with id {EventId} ACK with no handler", @event.Id);
                    await subscription.Ack(resolvedEvent);
                }

                return;
            }

            ConsumerContext context = new(resolvedEvent.Event.EventStreamId, retryCount);

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
                        Task preTask = action.Execute(dynamicEvent, context, cancellationToken);
                        await preTask;
                    }
                }

                _handlesFactory.TryGetHandlerFor(@event, scope!, out dynamic? handler);

                Task<OperationResult> task = (handler!).Handle(
                    dynamicEvent,
                    context,
                    cancellationToken
                );
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
                            cancellationToken
                        );
                        operationResult = await postTask;
                    }
                }

                return operationResult;
            }

            OperationResult result = OperationResult.Ok;
            if (_handlesFactory.TryGetPipelinesFor(@event, scope!, out List<dynamic>? pipelines))
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
                        current.Execute(dynamicEvent, context, next, cancellationToken);
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
            await ParkEventAndLogError(subscription, ex, resolvedEvent, retryCount);
        }
    }

    private async Task ParkEventAndLogError(
        PersistentSubscription subscription,
        Exception ex,
        ResolvedEvent resolvedEvent,
        int? retryCount
    )
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

    private async Task ParkEventAndLogWarning(
        IEvent @event,
        PersistentSubscription subscription,
        ResolvedEvent resolvedEvent
    )
    {
        _logger.LogWarning("Handler for event of type {EventType} not found", @event.GetType());
        await subscription.Nack(
            PersistentSubscriptionNakEventAction.Park,
            $"Handler for event of type {@event.GetType()} not found",
            resolvedEvent
        );
    }

    public Task Subscribe(string streamName, CancellationToken cancellationToken)
    {
        return _subscriber.Subscribe(streamName, OnEventAppeared, cancellationToken);
    }
}
