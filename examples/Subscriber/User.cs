namespace Subscriber
{
    using EasyEvs.Contracts;
    using Events;

    public class User : AggregateRoot
    {
        private void Apply(UserRegistered @event)
        {
            Id = @event.UserId;
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
}
