namespace EasyEvs.Tests.Pipelines;

using System;
using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Events.Orders.v2;

public class OrderEventPipelineAction1 : IPipelineHandlesEventAction<OrderEventCancelled>
{
    private readonly ICounter _counter;

    public OrderEventPipelineAction1(ICounter counter)
    {
        _counter = counter;
    }

    public async Task<OperationResult> Execute(
        OrderEventCancelled @event,
        IConsumerContext context,
        Func<Task<OperationResult>> next,
        CancellationToken cancellationToken
    )
    {
        Console.WriteLine("a");
        _counter.Touch();
        OperationResult result = await next();
        _counter.Touch();
        return result;
    }
}
