namespace EasyEvs.Internal;

using System;
using System.Linq;
using Contracts;

internal class AggregateRootAttributeResolver : IStreamResolver
{
    public string StreamForEvent<T>(string aggregateId)
        where T : IEvent
    {
        var genericType = typeof(AggregateAttribute<>);
        var firstOrDefault = typeof(T)
            .GetCustomAttributes(true)
            .Where(a =>
            {
                var type = a.GetType();
                return type.IsGenericType && type.Name == genericType.Name;
            })
            .Select(a => a.GetType().GenericTypeArguments[0])
            .FirstOrDefault();

        if (firstOrDefault is null)
        {
            throw new Exception($"Event of type {typeof(T).Name} MUST have an AggregateAttribute");
        }

        return $"{firstOrDefault.Name.ToSnakeCase('-')}_{aggregateId}";
    }

    public string StreamForAggregateRoot<T>(string aggregateId)
        where T : Aggregate
    {
        var suffix = typeof(T).Name.ToSnakeCase('-');
        return $"{suffix}_{aggregateId}";
    }

    public string StreamForAggregateRootId<T>(T aggregate)
        where T : Aggregate
    {
        var suffix = aggregate.GetType().Name;
        return $"{suffix.ToSnakeCase('-')}_{aggregate.Id}";
    }
}
