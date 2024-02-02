namespace Publisher
{
    using EasyEvs.Contracts;

    public class StreamResolver : IStreamResolver
    {
        public string StreamForEvent<T>(string aggregateId)
            where T : IEvent
        {
            return aggregateId;
        }

        public string StreamForAggregate<T>(string aggregateId)
            where T : Aggregate
        {
            return aggregateId;
        }

        public string StreamForAggregate<T>(T aggregate)
            where T : Aggregate
        {
            return aggregate.Id;
        }
    }
}
