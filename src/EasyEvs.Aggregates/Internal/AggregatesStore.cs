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
        List<IEvent> data = await eventStore.ReadStream(streamNameName, cancellationToken);
        aggregate.LoadFromHistory(data);
        logger.LogDebug("Aggregate with id {Id} loaded", id);
        return aggregate;
    }

    public async Task<T> Get<T>(string id, CancellationToken cancellationToken = default)
        where T : Aggregate, new()
    {
        string streamNameName = streamNameResolver.StreamForAggregate<T>(id);
        logger.LogDebug("Loading aggregate with id {Id} from stream {Stream}", id, streamNameName);
        List<IEvent> data = await eventStore.ReadStream(streamNameName, cancellationToken);
        T result = new();
        result.LoadFromHistory(data);
        logger.LogDebug("Aggregate with id {Id} loaded", id);
        return result;
    }
}
