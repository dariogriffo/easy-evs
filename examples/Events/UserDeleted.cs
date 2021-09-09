using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
[assembly: InternalsVisibleTo("AutoFixture")]
[assembly: InternalsVisibleTo("Publisher")]

namespace Events
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    using EasyEvs.Contracts;

    public class UserDeleted : IEvent
    {
        [JsonConstructor]
        public UserDeleted(
            Guid id,
            DateTime timestamp,
            Guid userId)
        {
            Id = id;
            Timestamp = timestamp;
            UserId = userId;
        }

        public Guid Id { get; internal set; }
        public DateTime Timestamp { get; internal set;}
        public string Version => "v1";
        public IReadOnlyDictionary<string, string> Metadata { get; set; }
        public Guid UserId { get; internal set;}
    }
}

