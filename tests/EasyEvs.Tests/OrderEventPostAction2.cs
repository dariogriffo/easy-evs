﻿namespace EasyEvs.Tests
{
    using System.Threading;
    using System.Threading.Tasks;
    using Events.Orders;

    public class OrderEventPostAction2 :
        IPostHandlesEventAction<OrderEvent4>
    {
        private readonly ICounter _counter;

        public OrderEventPostAction2(ICounter counter)
        {
            _counter = counter;
        }

        public Task<OperationResult> Execute(OrderEvent4 @event, ConsumerContext context, OperationResult result, CancellationToken cancellationToken)
        {
            _counter.Touch();
            return Task.FromResult(result);
        }
    }
}