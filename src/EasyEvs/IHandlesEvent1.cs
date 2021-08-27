namespace EasyEvs
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// An interface to implement to handle <see cref="IEvent"/>s.
    /// </summary>
    /// <typeparam name="T"><see cref="IEvent"/></typeparam>
    public interface IHandlesEvent<in T> : IHandlesEvent
        where T : IEvent
    {
        /// <summary>
        /// Implement this to handle the incoming events.
        /// Return an <see cref="OperationResult"/> specifying what the event store subscription should do with the event after being processed.
        /// </summary>
        /// <param name="event">The event that appeared on the persistent subscription.</param>
        /// <param name="context">The context of the event <see cref="ConsumerContext"/>.</param>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken"/>.</param>
        /// <returns>The task with an <see cref="OperationResult"/> to be awaited</returns>
        Task<OperationResult> Handle([NotNull]T @event, ConsumerContext context, CancellationToken cancellationToken);
    }
}
