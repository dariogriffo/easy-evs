namespace EasyEvs.Contracts;

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// The interface that gives access to https://github.com/EventStore/EventStore
/// </summary>
public interface IEventStore
{
    /// <summary>
    /// Appends the event asynchronously.
    /// </summary>
    /// <typeparam name="T"><see cref="IEvent"/></typeparam>
    /// <param name="stream">The stream where to append the event.</param>
    /// <param name="event">The event to be stored.</param>
    /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken"/>.</param>
    /// <returns>A <see cref="System.Threading.Tasks.Task"/> to be awaited.</returns>
    Task Append<T>(string stream, [NotNull] T @event, CancellationToken cancellationToken = default)
        where T : IEvent;

    /// <summary>
    /// Saves the event asynchronously. The stream MUST NOT exist
    /// </summary>
    /// <typeparam name="T"><see cref="IEvent"/></typeparam>
    /// <param name="stream">The stream where to append the event.</param>
    /// <param name="event">The event to be stored.</param>
    /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken"/>.</param>
    /// <returns>A <see cref="System.Threading.Tasks.Task"/> to be awaited.</returns>
    Task Store<T>(string stream, [NotNull] T @event, CancellationToken cancellationToken = default)
        where T : IEvent;

    /// <summary>
    /// Saves the event asynchronously. The stream MUST NOT exist
    /// </summary>
    /// <typeparam name="T"><see cref="IEvent"/></typeparam>
    /// <param name="stream">The stream where to append the event.</param>
    /// <param name="events">The list of events to be stored.</param>
    /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken"/>.</param>
    /// <returns>A <see cref="System.Threading.Tasks.Task"/> to be awaited.</returns>
    Task Store<T>(string stream, T[] events, CancellationToken cancellationToken = default)
        where T : IEvent;

    /// <summary>
    /// Appends the events asynchronously.
    /// </summary>
    /// <typeparam name="T"><see cref="IEvent"/></typeparam>
    /// <param name="stream">The stream where to append the event.</param>
    /// <param name="events">The list of events to be stored.</param>
    /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken"/>.</param>
    /// <returns>A <see cref="System.Threading.Tasks.Task"/> to be awaited.</returns>
    Task Append<T>(string stream, T[] events, CancellationToken cancellationToken = default)
        where T : IEvent;

    /// <summary>
    /// Appends the event asynchronously.
    /// </summary>
    /// <typeparam name="T"><see cref="IEvent"/></typeparam>
    /// <param name="event">The event to be stored.</param>
    /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken"/>.</param>
    /// <returns>A <see cref="System.Threading.Tasks.Task"/> to be awaited.</returns>
    Task Append<T>([NotNull] T @event, CancellationToken cancellationToken = default)
        where T : IEvent;

    /// <summary>
    /// Reads all the events from the stream
    /// </summary>
    /// <param name="stream">The stream name</param>
    /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken"/>.</param>
    /// <returns>A list of tuples containing the events with their associated (optional) metadata</returns>
    Task<List<IEvent>> ReadStream(string stream, CancellationToken cancellationToken = default);

    /// <summary>
    /// Subscribes asynchronously to a stream.
    /// Will use <see cref="EventStoreSettings.TreatMissingHandlersAsErrors"/> as value to deal with missing handlers
    /// </summary>
    /// <param name="stream">The stream name</param>
    /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken"/>.</param>
    /// <returns>A <see cref="System.Threading.Tasks.Task"/> to be awaited.</returns>
    Task SubscribeToStream(string stream, CancellationToken cancellationToken = default);

    /// <summary>
    /// Subscribes asynchronously to a stream
    /// </summary>
    /// <param name="command">The <see cref="SubscribeCommand"/>.</param>
    /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken"/>.</param>
    /// <returns>A <see cref="System.Threading.Tasks.Task"/> to be awaited.</returns>
    Task SubscribeToStream(SubscribeCommand command, CancellationToken cancellationToken = default);
}
