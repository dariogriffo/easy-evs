namespace EasyEvs.Tests.Events.Orders.v2;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Aggregates;
using Contracts;
using EasyEvs.Aggregates.Contracts;

[Aggregate<Order>]
[method: JsonConstructor]
public class OrderEventCancelled(
    Guid id,
    DateTime timestamp,
    Guid orderId,
    string? reason = default
) : IEvent
{
    public Guid Id { get; } = id;

    public Guid OrderId { get; } = orderId;
    public string? Reason { get; } = reason;

    public DateTime Timestamp { get; } = timestamp;

    public IReadOnlyDictionary<string, string>? Metadata { get; set; } =
        new Dictionary<string, string>();
}
