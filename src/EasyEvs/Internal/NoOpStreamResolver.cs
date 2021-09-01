namespace EasyEvs.Internal
{
    using System;
    using Contracts;

#pragma warning disable 1591
    public class NoOpStreamResolver : IStreamResolver
    {
        public string StreamForEvent<T>(T @event) where T : IEvent
        {
            throw new System.NotImplementedException();
        }

        public string StreamForAggregateRoot<T>(T aggregateRoot) where T : AggregateRoot
        {
            throw new NotImplementedException();
        }

        public string StreamForAggregateRoot<T>(Guid id) where T : AggregateRoot
        {
            throw new NotImplementedException();
        }
    }
#pragma warning restore 1591
}
