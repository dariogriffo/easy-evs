namespace EasyEvs.Tests.Events.Orders.v2;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Aggregates;
using Contracts;

[Aggregate<Order>]
public class OrderEventCancelled : IEvent
{
    [JsonConstructor]
    public OrderEventCancelled(Guid id, DateTime timestamp, Guid orderId, string? reason = default)
    {
        Id = id;
        Timestamp = timestamp;
        OrderId = orderId;
        Reason = reason;
    }

    public Guid Id { get; }

    public Guid OrderId { get; }
    public string Reason { get; }

    public DateTime Timestamp { get; }

    public string Version => "v2";

    public IReadOnlyDictionary<string, string> Metadata { get; set; }
}
