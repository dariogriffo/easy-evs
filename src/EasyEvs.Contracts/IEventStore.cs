namespace EasyEvs.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// The simplified interface that gives access to https://github.com/EventStore/EventStore
    /// </summary>
    public interface IEventStore
    {
        /// <summary>
        /// Appends the event asynchronously.
        /// </summary>
        /// <typeparam name="T"><see cref="IEvent"/></typeparam>
        /// <param name="event">The event to be stored.</param>
        /// <param name="stream">The stream where to append the event.</param>
        /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken"/>.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> to be awaited.</returns>
        Task Append<T>(
            [NotNull] T @event,
            [NotNull]string stream,
            CancellationToken cancellationToken = default)
            where T : IEvent;

        /// <summary>
        /// Appends the event asynchronously.
        /// </summary>
        /// <typeparam name="T"><see cref="IEvent"/></typeparam>
        /// <param name="event">The event to be stored.</param>
        /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken"/>.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> to be awaited.</returns>
        Task Append<T>(
            [NotNull] T @event,
            CancellationToken cancellationToken = default)
            where T : IEvent;

        /// <summary>
        /// Appends the event asynchronously into the specific position.
        /// </summary>
        /// <typeparam name="T"><see cref="IEvent"/></typeparam>
        /// <param name="event">The event to be stored.</param>
        /// <param name="position">The position where to store the event in the stream.</param>
        /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken"/>.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> to be awaited.</returns>
        Task Append<T>(
            [NotNull] T @event,
            long position,
            CancellationToken cancellationToken = default)
            where T : IEvent;

        /// <summary>
        /// Saves the aggregate root to the Event Store, might throw an exception if the stream for the aggregate root exists
        /// </summary>
        /// <typeparam name="T"><see cref="AggregateRoot"/></typeparam>
        /// <param name="aggregateRoot">The aggregate root to be saved.</param>
        /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken"/>.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> to be awaited.</returns>
        Task Create<T>([NotNull]T aggregateRoot,
            CancellationToken cancellationToken = default)
            where T : AggregateRoot;    

        /// <summary>
        /// Saves the aggregate root to the Event Store
        /// </summary>
        /// <typeparam name="T"><see cref="AggregateRoot"/></typeparam>
        /// <param name="aggregateRoot">The aggregate root to be saved.</param>
        /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken"/>.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> to be awaited.</returns>
        Task Save<T>(
            [NotNull] T aggregateRoot,
            CancellationToken cancellationToken = default)
            where T : AggregateRoot;

        /// <summary>
        /// Reads all the events from the stream
        /// </summary>
        /// <param name="stream">The stream name</param>
        /// <param name="position">The optional starting position where to read. If not set, all the available events are returned</param>
        /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken"/>.</param>
        /// <returns>A list of tuples containing the events with their associated (optional) metadata</returns>
        Task<List<IEvent>> ReadStream(
            [NotNull] string stream,
            long? position = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Subscribes asynchronously to a stream.
        /// Will use <see cref="EventStoreSettings.TreatMissingHandlersAsErrors"/> as value to deal with missing handlers
        /// </summary>
        /// <param name="stream">The stream name</param>
        /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken"/>.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> to be awaited.</returns>
        Task SubscribeToStream([NotNull] string stream, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the aggregate root from the Event Store
        /// </summary>
        /// <param name="id">The aggregate root's id.</param>
        /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken"/>.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> to be awaited.</returns>
        Task<T> Get<T>(Guid id, CancellationToken cancellationToken = default)
            where T : AggregateRoot, new();

        /// <summary>
        /// Subscribes asynchronously to a stream
        /// </summary>
        /// <param name="command">The <see cref="SubscribeCommand"/>.</param>
        /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken"/>.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> to be awaited.</returns>
        Task SubscribeToStream(SubscribeCommand command, CancellationToken cancellationToken = default);
    }
}
