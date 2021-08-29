namespace EasyEvs.Tests
{
    using System;
    using Events.Orders;
    using Events.Users;

    public class StreamResolver : IStreamResolver
    {
        public string StreamForEvent<T>(T @event) where T : IEvent
        {
            var stream = @event switch
            {
                OrderEvent1 e => $"Order-{e.OrderId}",
                OrderEvent2 e => $"Order-{e.OrderId}",
                OrderEvent3 e => $"Order-{e.OrderId}",
                OrderEvent4 e => $"Order-{e.OrderId}",
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

        public string StreamForAggregateRoot<T>(Guid id) where T : AggregateRoot
        {
            var suffix = typeof(T).Name;
            return $"{suffix}-{id}";
        }
    }
}
