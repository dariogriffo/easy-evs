namespace EasyEvs
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Action to be executed before the <see cref="IHandlesEvent{T}"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IPreHandlesEventAction<in T> : IPreHandlesEventAction
        where T : IEvent
    {
        /// <summary>
        /// Implement this to execute actions before <see cref="OperationResult"/>.
        /// Return an <see cref="OperationResult"/> specifying what the event store subscription should do with the event after being processed.
        /// </summary>
        /// <param name="event">The event that appeared on the persistent subscription.</param>
        /// <param name="context">The context of the event <see cref="ConsumerContext"/>.</param>
        /// <param name="cancellationToken">The <see cref="System.Threading"/>.</param>
        /// <returns>The task with an <see cref="IHandlesEvent{T}"/> to be awaited</returns>
        Task Execute([NotNull]T @event, ConsumerContext context, CancellationToken cancellationToken);
    }
}
