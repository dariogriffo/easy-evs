namespace EasyEvs.Tests.Pipelines;

using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Events.Orders;

public class OrderEventPostAction4(ICounter counter) : IPostHandlesEventAction<OrderAbandoned>
{
    public Task<OperationResult> Execute(
        OrderAbandoned @event,
        IConsumerContext context,
        OperationResult result,
        CancellationToken cancellationToken
    )
    {
        counter.Touch();
        return Task.FromResult(result);
    }
}
