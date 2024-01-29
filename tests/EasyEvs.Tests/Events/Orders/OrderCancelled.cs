namespace EasyEvs.Tests.Events.Orders;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Aggregates;
using Contracts;

[Aggregate<Order>]
public class OrderCancelled : IEvent
{
    [JsonConstructor]
    public OrderCancelled(Guid id, DateTime timestamp, Guid orderId)
    {
        Id = id;
        Timestamp = timestamp;
        OrderId = orderId;
    }

    public Guid Id { get; }

    public Guid OrderId { get; }

    public DateTime Timestamp { get; }

    public IReadOnlyDictionary<string, string>? Metadata { get; set; }
}
