namespace EasyEvs.Tests.Events.Users
{
    using System;

    public class User : AggregateRoot
    {
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
            ApplyChange(new UserCreated(Guid.NewGuid(), DateTime.UtcNow, "v1", id));
        }

        public void Update()
        {
            ApplyChange(new UserUpdated(Guid.NewGuid(), DateTime.UtcNow, "v1", Id));
        }

        public void Deactivate()
        {
            ApplyChange(new UserDeactivated(Guid.NewGuid(), DateTime.UtcNow, "v1", Id));
        }
    }
}
