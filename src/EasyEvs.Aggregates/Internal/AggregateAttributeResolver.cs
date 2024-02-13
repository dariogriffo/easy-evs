namespace EasyEvs.Internal;

using System;
using System.Collections.Concurrent;
using Contracts;

internal sealed class AggregateAttributeResolver : IAggregateStreamResolver
{
    private readonly ConcurrentDictionary<Type, string> _cachedTypes = new();

    public string StreamForAggregate<T>(string aggregateId)
        where T : Aggregate
    {
        var prefix = _cachedTypes.GetOrAdd(
            typeof(T),
            (_) =>
            {
                string prefix = typeof(T).Name.ToSnakeCase('-');
                return prefix;
            }
        );
        return $"{prefix}_{aggregateId}";
    }

    public string StreamForAggregate<T>(T aggregate)
        where T : Aggregate
    {
        var prefix = _cachedTypes.GetOrAdd(
            aggregate.GetType(),
            (_) =>
            {
                string prefix = typeof(T).Name.ToSnakeCase('-');
                return prefix;
            }
        );
        return $"{prefix}_{aggregate.Id}";
    }

    public string AggregateIdForStream(string streamName)
    {
        return streamName.Split('_')[1];
    }
}
