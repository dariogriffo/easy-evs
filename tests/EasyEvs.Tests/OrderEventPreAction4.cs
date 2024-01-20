namespace EasyEvs.Tests;

using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Events.Orders;

public class OrderEventPreAction4 : IPreHandlesEventAction<OrderAbandoned>
{
    private readonly ICounter _counter;

    public OrderEventPreAction4(ICounter counter)
    {
        _counter = counter;
    }

    public Task Execute(
        OrderAbandoned @event,
        IConsumerContext context,
        CancellationToken cancellationToken
    )
    {
        _counter.Touch();
        return Task.CompletedTask;
    }
}
