namespace Events;

using EasyEvs.Contracts;

public class User : Aggregate
{
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
}
