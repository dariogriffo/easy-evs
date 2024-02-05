namespace EasyEvs.Tests.Events.Orders.v2;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Aggregates;
using Contracts;
using EasyEvs.Aggregates.Contracts;

[Aggregate<Order>]
[method: JsonConstructor]
public class OrderRefundRequested(
    Guid id,
    DateTime timestamp,
    Guid orderId,
    decimal amount,
    bool isPartial
) : IEvent
{
    public Guid Id { get; } = id;

    public Guid OrderId { get; } = orderId;

    public decimal Amount { get; } = amount;

    public bool IsPartial { get; } = isPartial;

    public DateTime Timestamp { get; } = timestamp;

    public IReadOnlyDictionary<string, string>? Metadata { get; set; } =
        new Dictionary<string, string>();
}
