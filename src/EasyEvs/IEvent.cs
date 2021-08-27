namespace EasyEvs
{
    using System;

    /// <summary>
    /// An interface that represents an Event
    /// </summary>
    public interface IEvent
    {
        /// <summary>
        /// The id of the event
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// When the event occurred
        /// </summary>
        DateTime Timestamp { get; }

        /// <summary>
        /// The version of the event
        /// </summary>
        string Version { get; }
    }
}
