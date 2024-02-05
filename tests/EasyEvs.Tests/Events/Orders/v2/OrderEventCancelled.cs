namespace EasyEvs.Tests.Events.Orders.v2;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Aggregates;
using Contracts;
using EasyEvs.Aggregates.Contracts;

[Aggregate<Order>]
public class OrderEventCancelled : IEvent
{
    [method: JsonConstructor]
    private OrderEventCancelled(Guid id,
        DateTime timestamp,
        Guid orderId,
        string? reason = default)
    {
        Id = id;
        OrderId = orderId;
        Reason = reason;
        Timestamp = timestamp;
    }

    public OrderEventCancelled(
        Guid orderId,
        string? reason = default)
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        Reason = reason;
        Timestamp = DateTime.UtcNow;
    }

    public Guid Id { get; }

    public Guid OrderId { get; }
    public string? Reason { get; }

    public DateTime Timestamp { get; }

    public IReadOnlyDictionary<string, string>? Metadata { get; set; } =
        new Dictionary<string, string>();
}
