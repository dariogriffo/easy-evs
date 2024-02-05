namespace EasyEvs.Aggregates.Internal;

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Contracts;
using EasyEvs.Contracts;
using Microsoft.Extensions.Logging;

internal sealed class AggregatesStore(
    IEventStore eventStore,
    IAggregateStreamResolver streamResolver,
    ILogger<AggregatesStore> logger
) : IAggregateStore
{
    public Task Create<T>([NotNull] T aggregate, CancellationToken cancellationToken = default)
        where T : Aggregate
    {
        string stream = streamResolver.StreamForAggregate(aggregate);
        return eventStore.Store(stream, aggregate.UncommittedChanges.ToArray(), cancellationToken);
    }

    public Task Save<T>([NotNull] T aggregate, CancellationToken cancellationToken = default)
        where T : Aggregate
    {
        string stream = streamResolver.StreamForAggregate(aggregate);
        return eventStore.Append(stream, aggregate.UncommittedChanges.ToArray(), cancellationToken);
    }

    public async Task<T> Load<T>(T aggregate, CancellationToken cancellationToken = default)
        where T : Aggregate
    {
        string id = aggregate.Id;
        string streamName = streamResolver.StreamForAggregate(aggregate);
        logger.LogDebug("Loading aggregate with id {Id} from stream {Stream}", id, streamName);
        List<IEvent> data = await eventStore.ReadStream(streamName, cancellationToken);
        aggregate.LoadFromHistory(data);
        logger.LogDebug("Aggregate with id {Id} loaded", id);
        return aggregate;
    }

    public async Task<T> Get<T>(string id, CancellationToken cancellationToken = default)
        where T : Aggregate, new()
    {
        string streamName = streamResolver.StreamForAggregate<T>(id);
        logger.LogDebug("Loading aggregate with id {Id} from stream {Stream}", id, streamName);
        List<IEvent> data = await eventStore.ReadStream(streamName, cancellationToken);
        T result = new();
        result.LoadFromHistory(data);
        logger.LogDebug("Aggregate with id {Id} loaded", id);
        return result;
    }
}
