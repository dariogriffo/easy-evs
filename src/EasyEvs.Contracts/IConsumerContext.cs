namespace EasyEvs.Contracts;

using System;

/// <summary>
/// Context with all the information necessary besides the <see cref="IEvent"/> to be handled
/// </summary>
public interface IConsumerContext
{
    /// <summary>
    /// The amount of times the event has been processed
    /// </summary>
    int? RetryCount { get; }

    /// <summary>
    /// A unique correlation id. Useful to identify correlated operations across different contexts.
    /// </summary>
    Guid CorrelationId { get; }
    
    /// <summary>
    /// The name of the stream where the event was saved
    /// </summary>
    string StreamName { get; }
    
    /// <summary>
    /// The id of the aggregate the event was applied
    /// </summary>
    string? AggregateId { get; }
}
