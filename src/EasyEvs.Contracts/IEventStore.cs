namespace EasyEvs.Contracts;

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Exceptions;

/// <summary>
/// The interface that gives access to https://github.com/EventStore/EventStore
/// </summary>
public interface IEventStore
{
    /// <summary>
    /// Appends the event asynchronously.
    /// </summary>
    /// <typeparam name="T"><see cref="IEvent"/></typeparam>
    /// <param name="streamName">The streamName where to append the event.</param>
    /// <param name="event">The event to be stored.</param>
    /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken"/>.</param>
    /// <returns>A <see cref="System.Threading.Tasks.Task"/> to be awaited.</returns>
    Task Append<T>(
        string streamName,
        [NotNull] T @event,
        CancellationToken cancellationToken = default
    )
        where T : IEvent;

    /// <summary>
    /// Saves the event asynchronously. The streamName MUST NOT exist
    /// </summary>
    /// <typeparam name="T"><see cref="IEvent"/></typeparam>
    /// <param name="streamName">The streamName where to append the event.</param>
    /// <param name="event">The event to be stored.</param>
    /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken"/>.</param>
    /// <returns>A <see cref="System.Threading.Tasks.Task"/> to be awaited.</returns>
    /// <exception cref="StreamAlreadyExists"></exception>
    Task Store<T>(
        string streamName,
        [NotNull] T @event,
        CancellationToken cancellationToken = default
    )
        where T : IEvent;

    /// <summary>
    /// Saves the event asynchronously. The streamName MUST NOT exist
    /// </summary>
    /// <typeparam name="T"><see cref="IEvent"/></typeparam>
    /// <param name="streamName">The streamName where to append the event.</param>
    /// <param name="events">The list of events to be stored.</param>
    /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken"/>.</param>
    /// <returns>A <see cref="System.Threading.Tasks.Task"/> to be awaited.</returns>
    /// <exception cref="StreamAlreadyExists"></exception>
    Task Store<T>(string streamName, T[] events, CancellationToken cancellationToken = default)
        where T : IEvent;

    /// <summary>
    /// Appends the events asynchronously.
    /// </summary>
    /// <typeparam name="T"><see cref="IEvent"/></typeparam>
    /// <param name="streamName">The streamName where to append the event.</param>
    /// <param name="events">The list of events to be stored.</param>
    /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken"/>.</param>
    /// <returns>A <see cref="System.Threading.Tasks.Task"/> to be awaited.</returns>
    Task Append<T>(string streamName, T[] events, CancellationToken cancellationToken = default)
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
    /// Reads all the events from the streamName
    /// </summary>
    /// <param name="streamName">The streamName name</param>
    /// <param name="lastEventToLoad">The last event to load into the aggregate</param>
    /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken"/>.</param>
    /// <returns>A list of tuples containing the events with their associated (optional) metadata</returns>
    Task<List<IEvent>> ReadStream(
        string streamName,
        IEvent lastEventToLoad,
        CancellationToken cancellationToken = default
    );
    
    /// <summary>
    /// Reads all the events from the streamName
    /// </summary>
    /// <param name="streamName">The streamName name</param>

    /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken"/>.</param>
    /// <returns>A list of tuples containing the events with their associated (optional) metadata</returns>
    Task<List<IEvent>> ReadStream(
        string streamName,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Subscribes asynchronously to a streamName.
    /// </summary>
    /// <param name="streamName">The streamName name</param>
    /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken"/>.</param>
    /// <returns>A <see cref="System.Threading.Tasks.Task"/> to be awaited.</returns>
    /// /// <exception cref="SubscriptionFailed"></exception>
    Task SubscribeToStream(string streamName, CancellationToken cancellationToken = default);
}
