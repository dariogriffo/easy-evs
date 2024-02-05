namespace EasyEvs.Tests.Events.Users;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Aggregates;
using Contracts;
using EasyEvs.Aggregates.Contracts;

[Aggregate<User>]
public class UserUpdated : IEvent
{
    [method: JsonConstructor]
    private UserUpdated(Guid id, DateTime timestamp, string userId)
    {
        Id = id;
        UserId = userId;
        Timestamp = timestamp;
    }

    public UserUpdated(string userId)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Timestamp = DateTime.UtcNow;
    }

    public Guid Id { get; }

    public string UserId { get; }

    public DateTime Timestamp { get; }

    public IReadOnlyDictionary<string, string>? Metadata { get; set; } =
        new Dictionary<string, string>();
}
