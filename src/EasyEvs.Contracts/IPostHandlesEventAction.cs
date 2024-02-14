namespace EasyEvs.Contracts;

using System.Threading;
using System.Threading.Tasks;
using Internal;

/// <summary>
/// Action to be executed after the <see cref="IHandlesEvent{T}"/>
/// </summary>
/// <typeparam name="T"></typeparam>
[PostHandlerEvent]
public interface IPostHandlesEventAction<in T>
    where T : IEvent
{
    /// <summary>
    /// Implement this to execute actions after <see cref="IHandlesEvent{T}"/>.
    /// Return an <see cref="OperationResult"/> specifying what the event store subscription should do with the event after being processed.
    /// </summary>
    /// <param name="event">The event that appeared on the persistent subscription.</param>
    /// <param name="context">The context of the event <see cref="IConsumerContext"/>.</param>
    /// <param name="result">The result of the previous <see cref="IPostHandlesEventAction{T}"></see>/> or <see cref="IHandlesEvent{T}"/></param>
    /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken"/>.</param>
    /// <returns>The task with an <see cref="OperationResult"/> to be awaited</returns>
    Task<OperationResult> Execute(
        T @event,
        IConsumerContext context,
        OperationResult result,
        CancellationToken cancellationToken = default
    );
}
