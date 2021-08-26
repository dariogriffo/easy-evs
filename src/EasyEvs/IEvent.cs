namespace EasyEvs
{
    using System;

    public interface IEvent
    {
        Guid Id { get; }

        DateTime Timestamp { get; }

        string Version { get; }
    }
}
