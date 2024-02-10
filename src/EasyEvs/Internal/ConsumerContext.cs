namespace EasyEvs.Internal;

using System;
using Contracts;

internal sealed class ConsumerContext : IConsumerContext
{
    internal ConsumerContext(string streamName, Guid correlationId, int? retryCount = null, string? aggregateId = null)
    {
        StreamName = streamName;
        RetryCount = retryCount;
        AggregateId = aggregateId;
        CorrelationId = correlationId;
    }
    
    public int? RetryCount { get; }

    public string? AggregateId { get; }


    public Guid CorrelationId { get; }

    public string StreamName { get; }
}
