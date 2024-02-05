namespace EasyEvs.Tests.Aggregates;

using EasyEvs.Aggregates.Contracts;
using Events.Users;

public class User : Aggregate
{
    public enum UserStatus
    {
        Active,
        Inactive,
        Deleted,
        Created,
        Updated
    }
    
    public User() { }

    
    public static User Create(string id)
    {
        User user = new();
        var @event = new UserCreated(id);
        user.ApplyChange(@event);
        return user;
    }

    public int Sum { get; private set; }

    private void Apply(UserCreated e)
    {
        Id = e.UserId;
        Sum = 1;
        Status = UserStatus.Created;
    }

    private void Apply(UserUpdated e)
    {
        Sum += 10;
        Status = UserStatus.Updated;
    }

    private void Apply(UserDeactivated e)
    {
        Status = UserStatus.Inactive;
    }

    public UserStatus Status { get; private set; }


    public void Update()
    {
        ApplyChange(new UserUpdated(Id));
    }

    public void Deactivate()
    {
        ApplyChange(new UserDeactivated(Id));
    }
}
