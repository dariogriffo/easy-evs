namespace EasyEvs.Tests.Events.Users
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    using Contracts;

    public class UserCreated : IEvent
    {
        [JsonConstructor]
        public UserCreated(Guid id, DateTime timestamp, string version, Guid userId)
        {
            Id = id;
            Timestamp = timestamp;
            Version = version;
            UserId = userId;
        }


        public Guid Id { get; }

        public Guid UserId { get; }

        public DateTime Timestamp { get; }

        public string Version { get; }

        public IReadOnlyDictionary<string, string> Metadata { get; set; }
    }
}
