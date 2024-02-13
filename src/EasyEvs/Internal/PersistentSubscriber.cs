namespace EasyEvs.Internal;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Contracts;
using global::EventStore.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

internal class PersistentSubscriber(
    ILogger<PersistentSubscriber> logger,
    IInternalPersistentSubscriber subscriber,
    IHandlesFactory handlesFactory,
    ISerializer serializer,
    IOptions<EasyEvsConfiguration> easyEvsConfiguration,
    EventStoreSettings settings
) : IPersistentSubscriber
{
    private async Task OnEventAppeared(
        PersistentSubscription subscription,
        ResolvedEvent resolvedEvent,
        int? retryCount,
        CancellationToken cancellationToken
    )
    {
        if (resolvedEvent.IsResolved && !settings.ResolveEvents)
        {
            await subscription.Ack(resolvedEvent);
        }

        try
        {
            IEvent @event = serializer.Deserialize(resolvedEvent.OriginalEvent);
            logger.LogDebug("Event with id {EventId} arrived", @event.Id);

            if (!handlesFactory.TryGetScopeFor(@event, out IServiceScope? scope))
            {
                if (easyEvsConfiguration.Value.TreatMissingHandlersAsErrors)
                {
                    await ParkEventAndLogWarning(@event, subscription, resolvedEvent);
                }
                else
                {
                    logger.LogDebug("Event with id {EventId} ACK with no handler", @event.Id);
                    await subscription.Ack(resolvedEvent);
                }

                return;
            }

            ConsumerContext context = new(resolvedEvent.OriginalStreamId, retryCount);

            dynamic dynamicEvent = @event;

            async Task<OperationResult> ExecuteActionsAndHandler()
            {
                if (
                    handlesFactory.TryGetPreActionsFor(
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

                handlesFactory.TryGetHandlerFor(@event, scope!, out dynamic? handler);

                Task<OperationResult> task = (handler!).Handle(
                    dynamicEvent,
                    context,
                    cancellationToken
                );
                OperationResult operationResult = await task;

                if (
                    handlesFactory.TryGetPostActionsFor(
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
            if (handlesFactory.TryGetPipelinesFor(@event, scope!, out List<dynamic>? pipelines))
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
            logger.LogDebug(
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
        logger.LogError(
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
        logger.LogWarning("Handler for event of type {EventType} not found", @event.GetType());
        await subscription.Nack(
            PersistentSubscriptionNakEventAction.Park,
            $"Handler for event of type {@event.GetType()} not found",
            resolvedEvent
        );
    }

    public Task Subscribe(string streamName, CancellationToken cancellationToken)
    {
        return subscriber.Subscribe(streamName, OnEventAppeared, cancellationToken);
    }
}
