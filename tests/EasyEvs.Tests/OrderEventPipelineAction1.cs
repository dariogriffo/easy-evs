namespace EasyEvs.Tests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts;
    using Events.Orders;

    public class OrderEventPipelineAction1 :
        IPipelineHandlesEventAction<OrderEvent5>
    {
        private readonly ICounter _counter;

        public OrderEventPipelineAction1(ICounter counter)
        {
            _counter = counter;
        }

        public async Task<OperationResult> Execute(OrderEvent5 @event, IConsumerContext context, Func<Task<OperationResult>> next,
            CancellationToken cancellationToken)
        {
            Console.WriteLine("a");
            _counter.Touch();
            var result = await next();
            _counter.Touch();
            return result;
        }
    }
}
