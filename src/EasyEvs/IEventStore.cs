namespace EasyEvs
{
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
        /// <param name="metadata">Any metadata required to be saved along with the @event.</param>
        /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken"/>.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> to be awaited.</returns>
        Task Append<T>(
            [NotNull] T @event, 
            IReadOnlyDictionary<string, string>? metadata = null, 
            CancellationToken cancellationToken = default)
            where T : IEvent;

        /// <summary>
        /// Appends the event asynchronously into the specific position.
        /// </summary>
        /// <typeparam name="T"><see cref="IEvent"/></typeparam>
        /// <param name="event">The event to be stored.</param>
        /// <param name="position">The position where to store the event in the stream.</param>
        /// <param name="metadata">Any metadata required to be saved along with the @event.</param>
        /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken"/>.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> to be awaited.</returns>
        Task Append<T>([NotNull] T @event, long position, IReadOnlyDictionary<string, string>? metadata = null, CancellationToken cancellationToken = default)
            where T : IEvent;

        /// <summary>
        /// Reads all the events from the stream
        /// </summary>
        /// <param name="stream">The stream name</param>
        /// <param name="position">The optional starting position where to read. If not set, all the available events are returned</param>
        /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken"/>.</param>
        /// <returns>A list of tuples containing the events with their associated (optional) metadata</returns>
        Task<List<(IEvent, IReadOnlyDictionary<string, string>)>> ReadStream([NotNull] string stream, long? position = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Subscribes asynchronously to a stream
        /// </summary>
        /// <param name="stream">The stream name</param>
        /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken"/>.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> to be awaited.</returns>
        Task SubscribeToStream([NotNull] string stream, CancellationToken cancellationToken = default);
    }
}
