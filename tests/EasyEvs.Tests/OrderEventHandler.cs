namespace EasyEvs.Tests
{
    using System.Threading;
    using System.Threading.Tasks;
    using Events.Orders;

    public class OrderEventHandler : 
        IHandlesEvent<OrderEvent1>, 
        IHandlesEvent<OrderEvent2>, 
        IHandlesEvent<OrderEvent3>,
        IHandlesEvent<OrderEvent4>
    {
        private readonly ICounter _counter;

        public OrderEventHandler(ICounter counter)
        {
            _counter = counter;
        }

        public Task<OperationResult> Handle(OrderEvent1 @event, ConsumerContext context, CancellationToken cancellationToken)
        {
            _counter.Touch();
            return Task.FromResult(OperationResult.Ok);
        }

        public Task<OperationResult> Handle(OrderEvent2 @event, ConsumerContext context, CancellationToken cancellationToken)
        {
            _counter.Touch();
            return Task.FromResult(OperationResult.Ok);
        }

        public Task<OperationResult> Handle(OrderEvent3 @event, ConsumerContext context, CancellationToken cancellationToken)
        {
            _counter.Touch();
            return Task.FromResult(OperationResult.Ok);
        }

        public Task<OperationResult> Handle(OrderEvent4 @event, ConsumerContext context, CancellationToken cancellationToken)
        {
            _counter.Touch();
            return Task.FromResult(OperationResult.Ok);
        }
    }
}
