namespace Events
{
    using System;
    using System.Text.Json.Serialization;
    using EasyEvs;

    public class UserRegistered : IEvent
    {
        [JsonConstructor]
        public UserRegistered(
            Guid id, 
            DateTime timestamp, 
            Guid userId, 
            string firstName, 
            string lastName, 
            string emailAddress)
        {
            Id = id;
            Timestamp = timestamp;
            FirstName = firstName;
            LastName = lastName;
            EmailAddress = emailAddress;
            UserId = userId;
        }

        public Guid Id { get; internal set;}
        public DateTime Timestamp { get; internal set;}
        public string Version => "v1";
        public Guid UserId { get; internal set;}
        public string FirstName { get; internal set;}
        public string LastName { get; internal set;}
        public string EmailAddress { get; internal set;}
    }
}
