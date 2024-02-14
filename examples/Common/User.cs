namespace Events;

using System.Text.Json;
using EasyEvs.Contracts;

public class User : Aggregate
{
    static readonly JsonSerializerOptions SerializationOptions = new() { WriteIndented = true };

    private void Apply(UserRegistered @event)
    {
        Id = @event.UserId.ToString();
        Status = UserStatus.Registered;
    }

    private void Apply(UserUpdated @event)
    {
        Status = UserStatus.Updated;
    }

    private void Apply(UserDeleted @event)
    {
        Status = UserStatus.Deleted;
    }

    public UserStatus Status { get; private set; }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this, SerializationOptions);
    }
}
