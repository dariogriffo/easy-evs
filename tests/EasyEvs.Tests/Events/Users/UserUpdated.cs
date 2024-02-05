namespace EasyEvs.Tests.Events.Users;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Aggregates;
using Contracts;
using EasyEvs.Aggregates.Contracts;

[Aggregate<User>]
[method: JsonConstructor]
public class UserUpdated(Guid id, DateTime timestamp, string userId) : IEvent
{
    public Guid Id { get; } = id;

    public string UserId { get; } = userId;

    public DateTime Timestamp { get; } = timestamp;

    public IReadOnlyDictionary<string, string>? Metadata { get; set; } =
        new Dictionary<string, string>();
}
