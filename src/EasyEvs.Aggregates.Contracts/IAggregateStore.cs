namespace EasyEvs.Contracts;

using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// The interface that gives access to https://github.com/EventStore/EventStore
/// </summary>
public interface IAggregateStore
{
    /// <summary>
    /// Saves the aggregate root to the Event Store, might throw an exception if the streamName for the aggregate root exists
    /// </summary>
    /// <typeparam name="T"><see cref="Aggregate"/></typeparam>
    /// <param name="aggregate">The aggregate to be saved.</param>
    /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken"/>.</param>
    /// <returns>A <see cref="System.Threading.Tasks.Task"/> to be awaited.</returns>
    Task Create<T>([NotNull] T aggregate, CancellationToken cancellationToken = default)
        where T : Aggregate;

    /// <summary>
    /// Saves the aggregate root to the Event Store
    /// </summary>
    /// <typeparam name="T"><see cref="Aggregate"/></typeparam>
    /// <param name="aggregate">The aggregate to be saved.</param>
    /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken"/>.</param>
    /// <returns>A <see cref="System.Threading.Tasks.Task"/> to be awaited.</returns>
    Task Save<T>([NotNull] T aggregate, CancellationToken cancellationToken = default)
        where T : Aggregate;

    /// <summary>
    /// Gets the aggregate from the Store.
    /// The Aggregate must have a parameterless constructor
    /// </summary>
    /// <param name="id">The aggregate id.</param>
    /// <param name="lastEventToLoad">The last event to load into the aggregate</param>
    /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken"/>.</param>
    /// <returns>A <see cref="System.Threading.Tasks.Task"/> to be awaited.</returns>
    Task<T> CreateAggregateById<T>(
        string id,
        IEvent? lastEventToLoad = default,
        CancellationToken cancellationToken = default
    )
        where T : Aggregate, new();

    /// <summary>
    /// Gets the aggregate from the Store.
    /// The Aggregate must have a parameterless constructor
    /// </summary>
    /// <param name="streamName">The stream id.</param>
    /// <param name="lastEventToLoad">The last event to load into the aggregate</param>
    /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken"/>.</param>
    /// <returns>A <see cref="System.Threading.Tasks.Task"/> to be awaited.</returns>
    Task<T> CreateAggregateByStreamId<T>(
        string streamName,
        IEvent? lastEventToLoad = default,
        CancellationToken cancellationToken = default
    )
        where T : Aggregate, new();

    /// <summary>
    /// Hydrates the aggregate root from the Event Store
    /// </summary>
    /// <param name="aggregate">The aggregate.</param>
    /// <param name="lastEventToLoad">The last event to load into the aggregate</param>
    /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken"/>.</param>
    /// <returns>A <see cref="System.Threading.Tasks.Task"/> to be awaited.</returns>
    Task<T> Load<T>(
        T aggregate,
        IEvent? lastEventToLoad = default,
        CancellationToken cancellationToken = default
    )
        where T : Aggregate;
}
