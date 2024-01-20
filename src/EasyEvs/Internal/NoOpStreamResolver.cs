namespace EasyEvs.Internal;

using System;
using Contracts;

#pragma warning disable 1591
public class NoOpStreamResolver : IStreamResolver
{
    public string StreamForEvent<T>(string aggregateId)
        where T : IEvent
    {
        throw new NotImplementedException();
    }

    public string StreamForAggregateRoot<T>(string aggregateId)
        where T : Aggregate
    {
        throw new NotImplementedException();
    }

    public string StreamForAggregateRootId<T>(T aggregate)
        where T : Aggregate
    {
        throw new NotImplementedException();
    }
}
#pragma warning restore 1591
