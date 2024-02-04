namespace EasyEvs.Aggregates.Internal;

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Contracts;
using EasyEvs.Contracts;
using Microsoft.Extensions.Logging;

internal sealed class AggregatesStore : IAggregateStore
{
    private readonly IEventStore _eventStore;
    private readonly IAggregateStreamResolver _streamResolver;
    private readonly ILogger<AggregatesStore> _logger;

    public AggregatesStore(
        IEventStore eventStore,
        IAggregateStreamResolver streamResolver,
        ILogger<AggregatesStore> logger
    )
    {
        _eventStore = eventStore;
        _streamResolver = streamResolver;
        _logger = logger;
    }

    public Task Create<T>([NotNull] T aggregate, CancellationToken cancellationToken = default)
        where T : Aggregate
    {
        string stream = _streamResolver.StreamForAggregate(aggregate);
        return _eventStore.Store(stream, aggregate.UncommittedChanges.ToArray(), cancellationToken);
    }

    public Task Save<T>([NotNull] T aggregate, CancellationToken cancellationToken = default)
        where T : Aggregate
    {
        string stream = _streamResolver.StreamForAggregate(aggregate);
        return _eventStore.Append(
            stream,
            aggregate.UncommittedChanges.ToArray(),
            cancellationToken
        );
    }

    public async Task<T> Load<T>(T aggregate, CancellationToken cancellationToken = default)
        where T : Aggregate
    {
        string id = aggregate.Id;
        string streamName = _streamResolver.StreamForAggregate(aggregate);
        _logger.LogDebug("Loading aggregate with id {Id} from stream {Stream}", id, streamName);
        List<IEvent> data = await _eventStore.ReadStream(streamName, cancellationToken);
        aggregate.LoadFromHistory(data);
        _logger.LogDebug("Aggregate with id {Id} loaded", id);
        return aggregate;
    }

    public async Task<T> Get<T>(string id, CancellationToken cancellationToken = default)
        where T : Aggregate, new()
    {
        string streamName = _streamResolver.StreamForAggregate<T>(id);
        _logger.LogDebug("Loading aggregate with id {Id} from stream {Stream}", id, streamName);
        List<IEvent> data = await _eventStore.ReadStream(streamName, cancellationToken);
        T result = new();
        result.LoadFromHistory(data);
        _logger.LogDebug("Aggregate with id {Id} loaded", id);
        return result;
    }
}
