namespace EasyEvs.Tests
{
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts;
    using Events.Orders;
    using Events.Orders.v2;
    using OrderRefundRequested = Events.Orders.v2.OrderRefundRequested;

    public class OrderEventPostAction2 :
        IPostHandlesEventAction<OrderRefundRequested>
    {
        private readonly ICounter _counter;

        public OrderEventPostAction2(ICounter counter)
        {
            _counter = counter;
        }

        public Task<OperationResult> Execute(OrderRefundRequested @event, IConsumerContext context, OperationResult result, CancellationToken cancellationToken)
        {
            _counter.Touch();
            return Task.FromResult(result);
        }
    }
}
