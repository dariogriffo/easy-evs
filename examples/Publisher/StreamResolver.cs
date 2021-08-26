namespace Publisher
{
    using System;
    using EasyEvs;
    using Events;

    public class StreamResolver : IStreamResolver
    {
        public string StreamNameFor<T>(T @event) where T : IEvent
        {
            var userId = @event switch
            {
                UserUpdated e => e.UserId,
                UserRegistered e => e.UserId,
                UserDeleted e => e.UserId,
                _ => throw new ArgumentException($"Unknown event {@event.GetType().Name}")
            };

            return "user_" + userId;
        }
    }
}
