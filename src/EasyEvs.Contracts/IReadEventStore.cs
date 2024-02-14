namespace EasyEvs.Contracts;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

    /// <summary>
    /// Reads all the events from the streamName until the Id of the lastEventToRead
    /// matches one in the stream or the end of the stream is reached 
    /// </summary>
    /// <param name="streamName">The streamName name</param>
    /// <param name="lastEventToRead">The last event to load into the aggregate</param>
    /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken"/>.</param>
    /// <returns>A list of tuples containing the events with their associated (optional) metadata</returns>
    Task<List<IEvent>> ReadStreamUntilEvent(
        string streamName,
        IEvent lastEventToRead,
        CancellationToken cancellationToken = default
    );
    
    /// <summary>
    /// Reads all the events from the streamName that their Timestamp is lower or equal than timestamp
    /// </summary>
    /// <param name="streamName">The streamName name</param>
    /// <param name="timestamp">The timestamp in UTC included to load events</param>
    /// <param name="cancellationToken">The optional <see cref="System.Threading.CancellationToken"/>.</param>
    /// <returns>A list of tuples containing the events with their associated (optional) metadata</returns>
    Task<List<IEvent>> ReadStreamUntilTimestamp(
        string streamName,
        DateTime timestamp,
        CancellationToken cancellationToken = default
    );
}
