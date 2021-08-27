namespace EasyEvs
{
    /// <summary>
    /// The result of the handling the <see cref="IEvent"/>
    /// </summary>
    public enum OperationResult
    {
        /// <summary>
        /// The event was handled correctly and should be marked as processed on the subscription.
        /// </summary>
        Ok,
        /// <summary>
        /// There was a transient error and the event must be retried later.
        /// </summary>
        Retry,
        /// <summary>
        /// There was an unrecoverable error, and manual intervention is required.
        /// The event will be parked and can be replied manually later.
        /// </summary>
        Park
    }
}
