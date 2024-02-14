namespace EasyEvs.Tests.Pipelines;

using System.Threading;
using System.Threading.Tasks;
using Contracts;
using OrderRefundRequested = Events.Orders.v2.OrderRefundRequested;

public class OrderEventPostAction1(ICounter counter) : IPostHandlesEventAction<OrderRefundRequested>
{
    public Task<OperationResult> Execute(
        OrderRefundRequested @event,
        IConsumerContext context,
        OperationResult result,
        CancellationToken cancellationToken
    )
    {
        counter.Touch();
        return Task.FromResult(result);
    }
}
