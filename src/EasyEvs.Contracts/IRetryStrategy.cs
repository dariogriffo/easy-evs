namespace EasyEvs.Contracts;

using System;
using System.Threading.Tasks;

/// <summary>
/// Implement this interface to provide resilient connections
/// A default implementation is provided without any retries/>.
/// </summary>
public interface IRetryStrategy
{
    /// <summary>
    /// Function used to connect to a subscription using a retry mechanism
    /// </summary>
    /// <param name="func">The function called to subscribe to EventStore subscriptions</param>
    /// <returns>A task</returns>
    Task Subscribe(Func<Task> func);

    /// <summary>
    /// Function used to write to the EventStore using a retry mechanism
    /// </summary>
    /// <param name="func">The function called to write to the EventStore subscriptions</param>
    /// <returns>A task</returns>
    Task Write(Func<Task> func);

    /// <summary>
    /// Function used to read from the EventStore using a retry mechanism
    /// </summary>
    /// <param name="func">The function called to read from the EventStore subscriptions</param>
    /// <returns>A task</returns>
    Task Read(Func<Task> func);
}
