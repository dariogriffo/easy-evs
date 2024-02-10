namespace EasyEvs.Contracts;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// The interface that gives read access to https://github.com/EventStore/EventStore
/// </summary>
public interface IReadEventStore
{
    /// <summary>
    /// Reads all the events from the streamName
    /// </summary>
    /// <param name="streamName">The streamName name</param>
    /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken"/>.</param>
    /// <returns>A list of tuples containing the events with their associated (optional) metadata</returns>
    Task<List<IEvent>> ReadStream(string streamName, CancellationToken cancellationToken = default);
}
