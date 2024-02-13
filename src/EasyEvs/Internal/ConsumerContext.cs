namespace EasyEvs.Internal;

using System;
using Contracts;

internal sealed class ConsumerContext : IConsumerContext
{
    internal ConsumerContext(string streamName, int? retryCount = null)
    {
        StreamName = streamName;
        RetryCount = retryCount;
    }

    public int? RetryCount { get; }

    public string StreamName { get; }
}
