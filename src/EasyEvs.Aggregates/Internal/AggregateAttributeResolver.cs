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
        return $"{prefix}-{aggregateId}";
    }

    public string StreamForAggregate<T>(T aggregate)
        where T : Aggregate
    {
        Type type = aggregate.GetType();
        var prefix = _cachedTypes.GetOrAdd(
            type,
            (_) =>
            {
                string prefix = type.Name.ToSnakeCase('-');
                return prefix;
            }
        );
        return $"{prefix}-{aggregate.Id}";
    }

    public string AggregateIdForStream(string streamName)
    {
        return streamName.Replace($"{streamName.Split('-')[0]}-", string.Empty);
    }
}
