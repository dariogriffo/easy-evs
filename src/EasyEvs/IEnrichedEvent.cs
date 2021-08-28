namespace EasyEvs
{
    using System.Collections.Generic;

    /// <summary>
    /// An interface that represents an Event with associated metadata
    /// </summary>
    public interface IEnrichedEvent : IEvent
    {
        /// <summary>
        /// The metadata associated to the event
        /// </summary>
        IReadOnlyDictionary<string, string> Metadata { get; set; } 
    }
}
