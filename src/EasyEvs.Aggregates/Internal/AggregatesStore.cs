namespace EasyEvs.Internal;

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Microsoft.Extensions.Logging;

internal sealed class AggregatesStore(
    IEventStore eventStore,
    IAggregateStreamResolver streamNameResolver,
    ILogger<AggregatesStore> logger
) : IAggregateStore
{
    public Task Create<T>([NotNull] T aggregate, CancellationToken cancellationToken = default)
        where T : Aggregate
    {
        string streamName = streamNameResolver.StreamForAggregate(aggregate);
        return eventStore.Store(
            streamName,
            aggregate.UncommittedChanges.ToArray(),
            cancellationToken
        );
    }

    public Task Save<T>([NotNull] T aggregate, CancellationToken cancellationToken = default)
        where T : Aggregate
    {
        string streamName = streamNameResolver.StreamForAggregate(aggregate);
        return eventStore.Append(
            streamName,
            aggregate.UncommittedChanges.ToArray(),
            cancellationToken
        );
    }

    public async Task<T> Load<T>(T aggregate, CancellationToken cancellationToken = default)
        where T : Aggregate
    {
        string id = aggregate.Id;
        string streamNameName = streamNameResolver.StreamForAggregate(aggregate);
        logger.LogDebug("Loading aggregate with id {Id} from stream {Stream}", id, streamNameName);
        List<IEvent> data = await eventStore.ReadStream(
            streamNameName,
            cancellationToken: cancellationToken
        );
        aggregate.LoadFromHistory(data);
        logger.LogDebug("Aggregate with id {Id} loaded", id);
        return aggregate;
    }

    public async Task<T> Load<T>(
        T aggregate,
        IEvent? lastEventToLoad = default,
        CancellationToken cancellationToken = default
    )
        where T : Aggregate
    {
        string id = aggregate.Id;
        string streamNameName = streamNameResolver.StreamForAggregate(aggregate);
        logger.LogDebug("Loading aggregate with id {Id} from stream {Stream}", id, streamNameName);
        List<IEvent> data = await eventStore.ReadStream(
            streamNameName,
            cancellationToken: cancellationToken
        );
        aggregate.LoadFromHistory(data);
        logger.LogDebug("Aggregate with id {Id} loaded", id);
        return aggregate;
    }

    public async Task<T> CreateAggregateById<T>(
        string id,
        IEvent? lastEventToLoad = default,
        CancellationToken cancellationToken = default
    )
        where T : Aggregate, new()
    {
        string streamNameName = streamNameResolver.StreamForAggregate<T>(id);
        logger.LogDebug("Loading aggregate with id {Id} from stream {Stream}", id, streamNameName);
        List<IEvent> data = await eventStore.ReadStream(
            streamNameName,
            cancellationToken: cancellationToken
        );
        T aggregate = new();
        IEnumerable<IEvent> history =
            lastEventToLoad == default
                ? data
                : data.TakeWhile(
                    x => x.Timestamp < lastEventToLoad.Timestamp || x.Id == lastEventToLoad.Id
                );
        aggregate.LoadFromHistory(history);
        logger.LogDebug("Aggregate with id {Id} loaded", id);
        return aggregate;
    }

    public async Task<T> CreateAggregateByStreamId<T>(
        string streamName,
        IEvent? lastEventToLoad = default,
        CancellationToken cancellationToken = default
    )
        where T : Aggregate, new()
    {
        string id = streamNameResolver.AggregateIdForStream(streamName);
        logger.LogDebug("Loading aggregate with id {Id} from stream {Stream}", id, streamName);
        List<IEvent> data = await eventStore.ReadStream(
            streamName,
            cancellationToken: cancellationToken
        );
        T aggregate = new();
        IEnumerable<IEvent> history =
            lastEventToLoad == default
                ? data
                : data.TakeWhile(
                    x => x.Timestamp < lastEventToLoad.Timestamp || x.Id == lastEventToLoad.Id
                );

        aggregate.LoadFromHistory(history);
        logger.LogDebug("Aggregate with id {Id} loaded", streamName);
        return aggregate;
    }
}
