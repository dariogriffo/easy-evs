namespace EasyEvs.Tests.Events.Orders
{
    using System;
    using System.Text.Json.Serialization;

    public class OrderEvent4 : IEvent
    {
        [JsonConstructor]
        public OrderEvent4(Guid id, DateTime timestamp, string version, Guid orderId)
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
    }
}
