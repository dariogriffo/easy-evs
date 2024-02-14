namespace EasyEvs.Tests.Events.Orders;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Aggregates;
using Contracts;

[Aggregate<Order>]
public class OrderAbandoned : IEvent
{
    [method: JsonConstructor]
    private OrderAbandoned(Guid id, DateTime timestamp, Guid orderId)
    {
        Id = id;
        OrderId = orderId;
        Timestamp = timestamp;
    }

    public OrderAbandoned(Guid orderId)
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        Timestamp = DateTime.UtcNow;
    }

    public Guid Id { get; }

    public Guid OrderId { get; }

    public DateTime Timestamp { get; }

    public IReadOnlyDictionary<string, string>? Metadata { get; set; } =
        new Dictionary<string, string>();
}
