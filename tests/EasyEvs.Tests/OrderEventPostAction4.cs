namespace EasyEvs.Tests;

using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Events.Orders;

public class OrderEventPostAction4 : IPostHandlesEventAction<OrderAbandoned>
{
    private readonly ICounter _counter;

    public OrderEventPostAction4(ICounter counter)
    {
        _counter = counter;
    }

    public Task<OperationResult> Execute(
        OrderAbandoned @event,
        IConsumerContext context,
        OperationResult result,
        CancellationToken cancellationToken
    )
    {
        _counter.Touch();
        return Task.FromResult(result);
    }
}
