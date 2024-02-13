namespace EasyEvs.Internal;

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Contracts;

internal sealed class EventStore(
    IWriteEventStore write,
    IReadEventStore read,
    IPersistentSubscriber subscriber
) : IEventStore
{
    public Task Append<T>(
        string streamName,
        [NotNull] T @event,
        CancellationToken cancellationToken = default
    )
        where T : IEvent => write.Append(streamName, @event, cancellationToken);

    public Task Store<T>(
        string streamName,
        [NotNull] T @event,
        CancellationToken cancellationToken = default
    )
        where T : IEvent => write.Store(streamName, @event, cancellationToken);

    public Task Store<T>(
        string streamName,
        T[] events,
        CancellationToken cancellationToken = default
    )
        where T : IEvent => write.Store(streamName, events, cancellationToken);

    public Task Append<T>(
        string streamName,
        T[] events,
        CancellationToken cancellationToken = default
    )
        where T : IEvent => write.Store(streamName, events, cancellationToken);

    public Task Append<T>([NotNull] T @event, CancellationToken cancellationToken = default)
        where T : IEvent => write.Append(@event, cancellationToken);

    public Task<List<IEvent>> ReadStream(
        string streamName,
        IEvent? lastEventToLoad = default,
        CancellationToken cancellationToken = default
    ) => read.ReadStream(streamName, lastEventToLoad, cancellationToken);

    public Task SubscribeToStream(
        string streamName,
        CancellationToken cancellationToken = default
    ) => subscriber.Subscribe(streamName, cancellationToken);
}
