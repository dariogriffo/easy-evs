namespace EasyEvs.Tests.Events.Orders;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Aggregates;
using Contracts;
using EasyEvs.Aggregates.Contracts;

[Aggregate<Order>]
[method: JsonConstructor]
public class OrderAbandoned(Guid id, DateTime timestamp, Guid orderId) : IEvent
{
    public Guid Id { get; } = id;

    public Guid OrderId { get; } = orderId;

    public DateTime Timestamp { get; } = timestamp;

    public IReadOnlyDictionary<string, string>? Metadata { get; set; } =
        new Dictionary<string, string>();
}
