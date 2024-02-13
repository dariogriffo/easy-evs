namespace EasyEvs.Contracts;

using System;
using System.Threading;
using System.Threading.Tasks;
using Exceptions;

/// <summary>
/// The interface to have access to Persistent Subscriptions
/// </summary>
public interface IPersistentSubscriber
{
    /// <summary>
    /// Subscribes asynchronously to a streamName.
    /// </summary>
    /// <param name="streamName">The name of the stream</param>
    /// <param name="cancellationToken">The CancellationToken</param>
    /// <exception cref="SubscriptionFailed"></exception>
    public Task Subscribe(string streamName, CancellationToken cancellationToken);
}
