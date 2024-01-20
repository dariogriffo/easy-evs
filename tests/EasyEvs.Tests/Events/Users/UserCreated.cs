namespace EasyEvs.Tests.Events.Users;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Aggregates;
using Contracts;

[Aggregate<User>]
public class UserCreated : IEvent
{
    [JsonConstructor]
    public UserCreated(Guid id, DateTime timestamp, string version, string userId)
    {
        Id = id;
        Timestamp = timestamp;
        Version = version;
        UserId = userId;
    }

    public Guid Id { get; }

    public string UserId { get; }

    public DateTime Timestamp { get; }

    public string Version { get; }

    public IReadOnlyDictionary<string, string> Metadata { get; set; }
}
