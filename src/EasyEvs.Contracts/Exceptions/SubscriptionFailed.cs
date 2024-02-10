namespace EasyEvs.Contracts.Exceptions;

using System;

/// <summary>
/// An exception representing that the subscription to the streamName failed
/// </summary>
public class SubscriptionFailed : Exception
{
    /// <summary>
    /// The constructor
    /// </summary>
    /// <param name="streamName"></param>
    internal SubscriptionFailed(string streamName)
        : base($"Subscription to stream {streamName} failed")
    {
        Stream = streamName;
    }

    /// <summary>
    /// The name of the streamName
    /// </summary>
    public string Stream { get; }
}
