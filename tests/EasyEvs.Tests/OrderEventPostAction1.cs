namespace EasyEvs.Tests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts;
    using Events.Orders;

    public class OrderEventPostAction1 :
        IPostHandlesEventAction<OrderEvent4>
    {
        private readonly ICounter _counter;

        public OrderEventPostAction1(ICounter counter)
        {
            _counter = counter;
        }

        public Task<OperationResult> Execute(OrderEvent4 @event, IConsumerContext context, OperationResult result, CancellationToken cancellationToken)
        {
            _counter.Touch();
            return Task.FromResult(result);
        }
    }
}
