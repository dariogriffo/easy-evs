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
    [JsonConstructor]
    public UserUpdated(Guid id, DateTime timestamp, string userId)
    {
        Id = id;
        Timestamp = timestamp;
        UserId = userId;
    }

    public Guid Id { get; }

    public string UserId { get; }

    public DateTime Timestamp { get; }

    public IReadOnlyDictionary<string, string>? Metadata { get; set; } =
        new Dictionary<string, string>();
}
