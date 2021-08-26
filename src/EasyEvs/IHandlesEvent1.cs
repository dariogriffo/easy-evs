namespace EasyEvs
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IHandlesEvent<T> : IHandlesEvent
        where T : IEvent
    {
        Task<OperationResult> Handle([NotNull]T @event, ConsumerContext context, CancellationToken cancellationToken);
    }
}
