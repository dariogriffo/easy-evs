namespace EasyEvs.Contracts;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Internal;

/// <summary>
/// A Pipeline is to be executed before and after <see cref="IHandlesEvent{T}"/>
/// </summary>
/// <typeparam name="T"></typeparam>
[PipelineHandler]
public interface IPipelineHandlesEventAction<in T>
    where T : IEvent
{
    /// <summary>
    /// Implement this to execute actions after <see cref="IHandlesEvent{T}"/>.
    /// Return an <see cref="OperationResult"/> specifying what the event store subscription should do with the event after being processed.
    /// </summary>
    /// <param name="event">The event that appeared on the persistent subscription.</param>
    /// <param name="context">The context of the event <see cref="IConsumerContext"/>.</param>
    /// <param name="next">The next action to be executed</param>
    /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken"/>.</param>
    /// <returns>The task with an <see cref="OperationResult"/> to be awaited</returns>
    Task<OperationResult> Execute(
        [NotNull] T @event,
        IConsumerContext context,
        Func<Task<OperationResult>> next,
        CancellationToken cancellationToken
    );
}
