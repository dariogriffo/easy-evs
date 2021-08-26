namespace EasyEvs
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IEventStore
    {
        Task Append<T>([NotNull] T @event, IReadOnlyDictionary<string, string>? metadata = null, CancellationToken cancellationToken = default)
            where T : IEvent;

        Task Append<T>([NotNull] T @event, long position, IReadOnlyDictionary<string, string>? metadata = null, CancellationToken cancellationToken = default)
            where T : IEvent;

        Task<List<(IEvent, IReadOnlyDictionary<string, string>?)>> ReadStream([NotNull] string stream, long? position = null, CancellationToken cancellationToken = default);

        Task SubscribeToStream([NotNull] string stream, CancellationToken cancellationToken = default);
    }
}
