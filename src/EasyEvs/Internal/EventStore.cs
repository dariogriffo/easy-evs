namespace EasyEvs.Internal;

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Contracts;

internal sealed class EventStore : IEventStore
{
    private readonly IWriteEventStore _write;
    private readonly IReadEventStore _read;
    private readonly IPersistentSubscriber _subscriber;

    public EventStore(IWriteEventStore write,
        IReadEventStore read,
        IPersistentSubscriber subscriber)
    {
        _write = write;
        _read = read;
        _subscriber = subscriber;
    }

    public Task Append<T>(
        string streamName,
        [NotNull] T @event,
        CancellationToken cancellationToken = default
    )
        where T : IEvent => _write.Append(streamName, @event, cancellationToken);

    public Task Store<T>(
        string streamName,
        [NotNull] T @event,
        CancellationToken cancellationToken = default
    )
        where T : IEvent => _write.Store(streamName, @event, cancellationToken);

    public Task Store<T>(
        string streamName,
        T[] events,
        CancellationToken cancellationToken = default
    )
        where T : IEvent => _write.Store(streamName, events, cancellationToken);

    public Task Append<T>(
        string streamName,
        T[] events,
        CancellationToken cancellationToken = default
    )
        where T : IEvent => _write.Store(streamName, events, cancellationToken);

    public Task Append<T>([NotNull] T @event, CancellationToken cancellationToken = default)
        where T : IEvent => _write.Append(@event, cancellationToken);

    public Task<List<IEvent>> ReadStream(
        string streamName,
        IEvent lastEventToLoad,
        CancellationToken cancellationToken = default
    ) => _read.ReadStream(streamName, lastEventToLoad, cancellationToken);
    
    public Task<List<IEvent>> ReadStream(
        string streamName,

        CancellationToken cancellationToken = default
    ) => _read.ReadStream(streamName, cancellationToken);

    public Task SubscribeToStream(
        string streamName,
        CancellationToken cancellationToken = default
    ) => _subscriber.Subscribe(streamName, cancellationToken);
}
