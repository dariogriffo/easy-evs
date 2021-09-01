﻿namespace EasyEvs.Tests
{
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts;
    using Events.Orders;

    public class OrderEventPreAction1 :
        IPreHandlesEventAction<OrderEvent4>
    {
        private readonly ICounter _counter;

        public OrderEventPreAction1(ICounter counter)
        {
            _counter = counter;
        }

        public Task Execute(OrderEvent4 @event, IConsumerContext context, CancellationToken cancellationToken)
        {
            _counter.Touch();
            return Task.CompletedTask;
        }
    }
}
