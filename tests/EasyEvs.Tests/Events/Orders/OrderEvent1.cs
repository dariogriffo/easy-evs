namespace EasyEvs.Tests.Events.Orders
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    using Contracts;

    public class OrderEvent1 : IEvent
    {
        [JsonConstructor]
        public OrderEvent1(Guid id, DateTime timestamp, string version, Guid orderId)
        {
            Id = id;
            Timestamp = timestamp;
            Version = version;
            OrderId = orderId;
        }

        
        public Guid Id { get; }
        
        public Guid OrderId { get; }
        
        public DateTime Timestamp { get; }
        
        public string Version { get; }
        public IReadOnlyDictionary<string, string> Metadata { get; set; }
    }
}
