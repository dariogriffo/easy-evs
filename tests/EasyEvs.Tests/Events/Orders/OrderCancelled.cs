namespace EasyEvs.Tests.Events.Orders;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Aggregates;
using Contracts;
using EasyEvs.Aggregates.Contracts;

[Aggregate<Order>]
public class OrderCancelled : IEvent
{
    [method: JsonConstructor]
    private OrderCancelled(Guid id, DateTime timestamp, Guid orderId)
    {
        Id = id;
        OrderId = orderId;
        Timestamp = timestamp;
    }
    
    public OrderCancelled(Guid orderId)
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        Timestamp = DateTime.UtcNow;
    }

    public Guid Id { get; }

    public Guid OrderId { get; }

    public DateTime Timestamp { get; }

    public IReadOnlyDictionary<string, string>? Metadata { get; set; }
}
