namespace EasyEvs.Tests
{
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts;
    using Events.Orders;

    public class OrderEventHandler : 
        IHandlesEvent<OrderEvent1>, 
        IHandlesEvent<OrderEvent2>, 
        IHandlesEvent<OrderEvent3>,
        IHandlesEvent<OrderEvent4>,
        IHandlesEvent<OrderEvent5>
    {
        private readonly ICounter _counter;

        public OrderEventHandler(ICounter counter)
        {
            _counter = counter;
        }

        public Task<OperationResult> Handle(OrderEvent1 @event, IConsumerContext context, CancellationToken cancellationToken)
        {
            _counter.Touch();
            return Task.FromResult(OperationResult.Ok);
        }

        public Task<OperationResult> Handle(OrderEvent2 @event, IConsumerContext context, CancellationToken cancellationToken)
        {
            _counter.Touch();
            return Task.FromResult(OperationResult.Ok);
        }

        public Task<OperationResult> Handle(OrderEvent3 @event, IConsumerContext context, CancellationToken cancellationToken)
        {
            _counter.Touch();
            return Task.FromResult(OperationResult.Ok);
        }

        public Task<OperationResult> Handle(OrderEvent4 @event, IConsumerContext context, CancellationToken cancellationToken)
        {
            _counter.Touch();
            return Task.FromResult(OperationResult.Ok);
        }

        public Task<OperationResult> Handle(OrderEvent5 @event, IConsumerContext context, CancellationToken cancellationToken)
        {
            _counter.Touch();
            return Task.FromResult(OperationResult.Ok);
        }
    }
}
