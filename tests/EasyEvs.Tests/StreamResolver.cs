namespace EasyEvs.Tests
{
    using System;
    using Contracts;
    using Events.Orders;
    using Events.Orders.v2;
    using Events.Users;
    using OrderRefundRequested = Events.Orders.v2.OrderRefundRequested;

    public class StreamResolver : IStreamResolver
    {
        public string StreamForEvent<T>(T @event) where T : IEvent
        {
            var stream = @event switch
            {
                OrderCreated e => $"Order-{e.OrderId}",
                OrderCancelled e => $"Order-{e.OrderId}",
                Events.Orders.OrderRefundRequested e => $"Order-{e.OrderId}",
                OrderRefundRequested e => $"Order-{e.OrderId}",
                OrderEventCancelled e => $"Order-{e.OrderId}",
            };
            return stream;
        }

        public string StreamForAggregateRoot<T>(T aggregateRoot) where T : AggregateRoot
        {
            var stream = aggregateRoot switch
            {
                User e => $"User-{e.Id}"
            };
            return stream;
        }

        public string StreamForAggregateRoot<T>(string id) where T : AggregateRoot
        {
            var suffix = typeof(T).Name;
            return $"{suffix}-{id}";
        }
    }
}
