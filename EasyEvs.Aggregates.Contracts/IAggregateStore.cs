namespace EasyEvs.Aggregates.Contracts;

using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// The interface that gives access to https://github.com/EventStore/EventStore
/// </summary>
public interface IAggregateStore
{
    /// <summary>
    /// Saves the aggregate root to the Event Store, might throw an exception if the stream for the aggregate root exists
    /// </summary>
    /// <typeparam name="T"><see cref="EasyEvs.Aggregates.Contracts.Aggregate"/></typeparam>
    /// <param name="aggregate">The aggregate root to be saved.</param>
    /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken"/>.</param>
    /// <returns>A <see cref="System.Threading.Tasks.Task"/> to be awaited.</returns>
    Task Create<T>([NotNull] T aggregate, CancellationToken cancellationToken = default)
        where T : Aggregate;

    /// <summary>
    /// Saves the aggregate root to the Event Store
    /// </summary>
    /// <typeparam name="T"><see cref="EasyEvs.Aggregates.Contracts.Aggregate"/></typeparam>
    /// <param name="aggregate">The aggregate root to be saved.</param>
    /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken"/>.</param>
    /// <returns>A <see cref="System.Threading.Tasks.Task"/> to be awaited.</returns>
    Task Save<T>([NotNull] T aggregate, CancellationToken cancellationToken = default)
        where T : Aggregate;

    /// <summary>
    /// Gets the aggregate root from the Event Store
    /// </summary>
    /// <param name="id">The aggregate root's id.</param>
    /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken"/>.</param>
    /// <returns>A <see cref="System.Threading.Tasks.Task"/> to be awaited.</returns>
    Task<T> Get<T>(string id, CancellationToken cancellationToken = default)
        where T : Aggregate, new();

    /// <summary>
    /// Hydrates the aggregate root from the Event Store
    /// </summary>
    /// <param name="aggregate">The aggregate root.</param>
    /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken"/>.</param>
    /// <returns>A <see cref="System.Threading.Tasks.Task"/> to be awaited.</returns>
    Task<T> Load<T>(T aggregate, CancellationToken cancellationToken = default)
        where T : Aggregate;
}
