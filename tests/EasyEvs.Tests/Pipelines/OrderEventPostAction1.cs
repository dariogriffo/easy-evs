namespace EasyEvs.Tests.Pipelines;

using System.Threading;
using System.Threading.Tasks;
using Contracts;
using OrderRefundRequested = Events.Orders.v2.OrderRefundRequested;

public class OrderEventPostAction1 : IPostHandlesEventAction<OrderRefundRequested>
{
    private readonly ICounter _counter;

    public OrderEventPostAction1(ICounter counter)
    {
        _counter = counter;
    }

    public Task<OperationResult> Execute(
        OrderRefundRequested @event,
        IConsumerContext context,
        OperationResult result,
        CancellationToken cancellationToken
    )
    {
        _counter.Touch();
        return Task.FromResult(result);
    }
}
