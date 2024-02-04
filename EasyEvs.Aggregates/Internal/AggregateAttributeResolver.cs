namespace EasyEvs.Aggregates.Internal;

using Contracts;

internal sealed class AggregateAttributeResolver : IAggregateStreamResolver
{
    public string StreamForAggregate<T>(string aggregateId)
        where T : Aggregate
    {
        string suffix = typeof(T).Name.ToSnakeCase('-');
        return $"{suffix}_{aggregateId}";
    }

    public string StreamForAggregate<T>(T aggregate)
        where T : Aggregate
    {
        string suffix = aggregate.GetType().Name.ToSnakeCase('-');
        return $"{suffix}_{aggregate.Id}";
    }
}
