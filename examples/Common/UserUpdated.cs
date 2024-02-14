namespace Events;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using EasyEvs.Contracts;

[Aggregate<User>]
public class UserUpdated : IEvent
{
    [method: JsonConstructor]
    public UserUpdated(Guid id,
        DateTime timestamp,
        Guid userId,
        string firstName,
        string lastName,
        string emailAddress)
    {
        Id = id;
        Timestamp = timestamp;
        UserId = userId;
        FirstName = firstName;
        LastName = lastName;
        EmailAddress = emailAddress;
    }

    public Guid Id { get; internal set; }
    public DateTime Timestamp { get; internal set; }

    public IReadOnlyDictionary<string, string> Metadata { get; set; }
    public Guid UserId { get; internal set; }
    public string FirstName { get; internal set; }
    public string LastName { get; internal set; }
    public string EmailAddress { get; internal set; }
}
