namespace EasyEvs.Contracts;

using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// The interface that gives access to write to https://github.com/EventStore/EventStore
/// </summary>
public interface IWriteEventStore
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
}
