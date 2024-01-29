namespace EasyEvs.Tests.Aggregates;

using System;
using Contracts;
using Events.Users;

public class User : Aggregate
{
    public User() { }

    public User(string id)
    {
        Id = id;
    }

    public int Sum { get; private set; }

    private void Apply(UserCreated e)
    {
        Id = e.UserId;
        Sum = 1;
    }

    private void Apply(UserUpdated e)
    {
        Sum += 10;
    }

    private void Apply(UserDeactivated e)
    {
        Sum += 100;
    }

    public void Create(Guid id)
    {
        ApplyChange(new UserCreated(Guid.NewGuid(), DateTime.UtcNow, id.ToString()));
    }

    public void Update()
    {
        ApplyChange(new UserUpdated(Guid.NewGuid(), DateTime.UtcNow, Id));
    }

    public void Deactivate()
    {
        ApplyChange(new UserDeactivated(Guid.NewGuid(), DateTime.UtcNow, Id));
    }
}
