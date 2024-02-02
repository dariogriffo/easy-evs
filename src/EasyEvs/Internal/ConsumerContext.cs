namespace EasyEvs.Internal;

using System;
using Contracts;

internal sealed class ConsumerContext : IConsumerContext
{
    internal ConsumerContext(Guid correlationId, int? retryCount = null)
    {
        RetryCount = retryCount;
        CorrelationId = correlationId;
    }

    /// <summary>
    /// The amount of times the event has been processed
    /// </summary>
    public int? RetryCount { get; }

    /// <summary>
    /// A unique correlation id. Useful to identify correlated operations across different contexts.
    /// </summary>
    public Guid CorrelationId { get; }
}
