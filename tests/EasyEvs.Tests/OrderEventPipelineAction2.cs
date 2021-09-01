namespace EasyEvs.Tests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts;
    using Events.Orders;

    public class OrderEventPipelineAction2 :
        IPipelineHandlesEventAction<OrderEvent5>
    {
        private readonly ICounter _counter;

        public OrderEventPipelineAction2(ICounter counter)
        {
            _counter = counter;
        }

        public async Task<OperationResult> Execute(OrderEvent5 @event, IConsumerContext context, Func<Task<OperationResult>> next,
            CancellationToken cancellationToken)
        {
            _counter.Touch();
            var result = await next();
            _counter.Touch();
            return result;
        }
    }
}
