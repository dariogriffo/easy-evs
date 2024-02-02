namespace EasyEvs.Contracts;

/// <summary>
/// An interface required to be implemented in order for EasyEvs to work
/// </summary>
public interface IAggregateStreamResolver
{
    /// <summary>
    /// The name of the stream for the aggregate
    /// </summary>
    /// <typeparam name="T"><see cref="IEvent"/>.</typeparam>
    /// <param name="aggregateId"></param>
    /// <returns>The name of the stream.</returns>
    string StreamForAggregate<T>(string aggregateId)
        where T : Aggregate;

    /// <summary>
    /// The name of the stream for this aggregate.
    /// </summary>
    /// <typeparam name="T"><see cref="Aggregate"/>.</typeparam>
    /// <param name="aggregate">The aggregate root.</param>
    /// <returns>The name of the stream.</returns>
    string StreamForAggregate<T>(T aggregate)
        where T : Aggregate;
}
