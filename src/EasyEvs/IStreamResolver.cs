namespace EasyEvs
{
    using System;

    /// <summary>
    /// An interface required to be implemented in order for the framework to work
    /// This interface has to decide where the event must be sent.
    /// </summary>
    public interface IStreamResolver
    {
        /// <summary>
        /// The name of the stream this specific event is stored in.
        /// </summary>
        /// <typeparam name="T"><see cref="IEvent"/>.</typeparam>
        /// <param name="event">The event.</param>
        /// <returns>The name of the stream.</returns>
        string StreamForEvent<T>(T @event) where T : IEvent;

        /// <summary>
        /// The name of the stream for this aggregate root.
        /// </summary>
        /// <typeparam name="T"><see cref="AggregateRoot"/>.</typeparam>
        /// <param name="aggregateRoot">The aggregate root.</param>
        /// <returns>The name of the stream.</returns>
        string StreamForAggregateRoot<T>(T aggregateRoot) where T : AggregateRoot;
        
        /// <summary>
        /// The name of the stream for the aggregate root with the specified id.
        /// </summary>
        /// <param name="id">The aggregate root's id.</param>
        /// <returns>The name of the stream.</returns>
        string StreamForAggregateRoot<T>(Guid id) where T : AggregateRoot;
    }
}
