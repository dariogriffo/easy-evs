namespace Subscriber
{
    using System;
    using EasyEvs.Contracts;
    using Events;

    public class StreamResolver : IStreamResolver
    {
        
        public string StreamForEvent<T>(T @event) where T : IEvent
        {
            var userId = @event switch
            {
                UserUpdated e => e.UserId,
                UserRegistered e => e.UserId,
                UserDeleted e => e.UserId,
                _ => throw new ArgumentException($"Unknown event {@event.GetType().Name}")
            };

            return "user-" + userId;
        }

        public string StreamForAggregateRoot<T>(T aggregateRoot) where T : AggregateRoot
        {
            throw new NotImplementedException();
        }

        public string StreamForAggregateRoot<T>(System.Guid id) where T : AggregateRoot
        {
            throw new NotImplementedException();
        }
    }
}
