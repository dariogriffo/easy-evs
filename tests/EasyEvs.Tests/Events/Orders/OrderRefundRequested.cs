namespace EasyEvs.Tests.Events.Orders;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Aggregates;
using Contracts;

[Aggregate<Order>]
public class OrderRefundRequested : IEvent
{
    [JsonConstructor]
    public OrderRefundRequested(Guid id, DateTime timestamp, Guid orderId)
    {
        Id = id;
        Timestamp = timestamp;
        OrderId = orderId;
    }

    public Guid Id { get; }

    public Guid OrderId { get; }

    public DateTime Timestamp { get; }

    public string Version => "v1";

    public IReadOnlyDictionary<string, string> Metadata { get; set; }
}
