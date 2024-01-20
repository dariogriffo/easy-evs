namespace EasyEvs.Tests;

using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Events.Orders;

public class OrderEventPostAction3 : IPostHandlesEventAction<OrderDelivered>
{
    private readonly ICounter _counter;

    public OrderEventPostAction3(ICounter counter)
    {
        _counter = counter;
    }

    public Task<OperationResult> Execute(
        OrderDelivered @event,
        IConsumerContext context,
        OperationResult result,
        CancellationToken cancellationToken
    )
    {
        _counter.Touch();
        return Task.FromResult(result);
    }
}
