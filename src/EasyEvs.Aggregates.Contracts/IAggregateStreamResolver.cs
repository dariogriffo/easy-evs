namespace EasyEvs.Contracts;

/// <summary>
/// An interface required to be implemented in order for EasyEvs to work
/// </summary>
public interface IAggregateStreamResolver
{
    /// <summary>
    /// Returns the name of the streamName for the aggregate
    /// </summary>
    /// <typeparam name="T"><see cref="IEvent"/>.</typeparam>
    /// <param name="aggregateId"></param>
    /// <returns>The name of the streamName.</returns>
    string StreamForAggregate<T>(string aggregateId)
        where T : Aggregate;

    /// <summary>
    /// Returns the name of the streamName for this aggregate.
    /// </summary>
    /// <typeparam name="T"><see cref="Aggregate"/>.</typeparam>
    /// <param name="aggregate">The aggregate.</param>
    /// <returns>The name of the streamName.</returns>
    string StreamForAggregate<T>(T aggregate)
        where T : Aggregate;

    /// <summary>
    /// Returns the id for a given stream name
    /// </summary>
    /// <param name="streamName"></param>
    /// <returns></returns>
    string AggregateIdForStream(string streamName);
}
