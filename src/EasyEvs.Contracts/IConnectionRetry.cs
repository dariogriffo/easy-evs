namespace EasyEvs.Contracts
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Implement this interface to provide resilient connections
    /// A default implementation is provided without any reconnection but that can be configured with <see cref="EventStoreSettings.SubscriptionReconnectionAttempts"/>.
    /// </summary>
    public interface IConnectionRetry
    {
        /// <summary>
        /// Function used to connect to a subscription using a retry mechanism
        /// </summary>
        /// <param name="func">The function called to subscribe to EventStore subscriptions</param>
        /// <param name="onException">A function called when the exception occurs.</param>
        /// <returns>A task</returns>
        Task Subscribe(Func<Task> func, Func<Exception, Task> onException);
        
        /// <summary>
        /// Function used to write to the EventStore using a retry mechanism
        /// </summary>
        /// <param name="func">The function called to write to the EventStore subscriptions</param>
        /// <param name="onException">A function called when the exception occurs.</param>
        /// <returns>A task</returns>
        Task Write(Func<Task> func, Func<Exception, Task> onException);

        /// <summary>
        /// Function used to read from the EventStore using a retry mechanism
        /// </summary>
        /// <param name="func">The function called to read from the EventStore subscriptions</param>
        /// <param name="onException">A function called when the exception occurs.</param>
        /// <returns>A task</returns>
        Task Read(Func<Task> func, Func<Exception, Task> onException);
    }
}
