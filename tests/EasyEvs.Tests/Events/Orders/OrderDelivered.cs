namespace EasyEvs.Tests.Events.Orders;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Aggregates;
using Contracts;
using EasyEvs.Aggregates.Contracts;

[Aggregate<Order>]
public class OrderDelivered : IEvent
{
    [JsonConstructor]
    public OrderDelivered(Guid id, DateTime timestamp, Guid orderId)
    {
        Id = id;
        Timestamp = timestamp;
        OrderId = orderId;
    }

    public Guid Id { get; }

    public Guid OrderId { get; }

    public DateTime Timestamp { get; }

    public IReadOnlyDictionary<string, string>? Metadata { get; set; } =
        new Dictionary<string, string>();
}
