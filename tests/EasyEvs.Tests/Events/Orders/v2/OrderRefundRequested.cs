namespace EasyEvs.Tests.Events.Orders.v2;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using AggregateRoots;
using Contracts;

[Aggregate<Order>]
public class OrderRefundRequested : IEvent
{
    [JsonConstructor]
    public OrderRefundRequested(
        Guid id,
        DateTime timestamp,
        Guid orderId,
        decimal amount,
        bool isPartial
    )
    {
        Id = id;
        Timestamp = timestamp;
        OrderId = orderId;
        Amount = amount;
        IsPartial = isPartial;
    }

    public Guid Id { get; }

    public Guid OrderId { get; }

    public decimal Amount { get; }

    public bool IsPartial { get; }

    public DateTime Timestamp { get; }

    public string Version => "v2";

    public IReadOnlyDictionary<string, string> Metadata { get; set; }
}
