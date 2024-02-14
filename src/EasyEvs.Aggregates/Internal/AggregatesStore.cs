namespace EasyEvs.Internal;

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Microsoft.Extensions.Logging;

internal sealed class AggregatesStore : IAggregateStore
{
    private readonly IEventStore _eventStore;
    private readonly IAggregateStreamResolver _streamNameResolver;
    private readonly ILogger<AggregatesStore> _logger;

    public AggregatesStore(
        IEventStore eventStore,
        IAggregateStreamResolver streamNameResolver,
        ILogger<AggregatesStore> logger
    )
    {
        _eventStore = eventStore;
        _streamNameResolver = streamNameResolver;
        _logger = logger;
    }

    public Task Create<T>([NotNull] T aggregate, CancellationToken cancellationToken = default)
        where T : Aggregate
    {
        string streamName = _streamNameResolver.StreamForAggregate(aggregate);
        return _eventStore.Store(
            streamName,
            aggregate.UncommittedChanges.ToArray(),
            cancellationToken
        );
    }

    public Task Save<T>([NotNull] T aggregate, CancellationToken cancellationToken = default)
        where T : Aggregate
    {
        string streamName = _streamNameResolver.StreamForAggregate(aggregate);
        return _eventStore.Append(
            streamName,
            aggregate.UncommittedChanges.ToArray(),
            cancellationToken
        );
    }

    public async Task<T> Load<T>(T aggregate, CancellationToken cancellationToken = default)
        where T : Aggregate
    {
        string id = aggregate.Id;
        string streamNameName = _streamNameResolver.StreamForAggregate(aggregate);
        _logger.LogDebug("Loading aggregate with id {Id} from stream {Stream}", id, streamNameName);
        List<IEvent> data = await _eventStore.ReadStream(
            streamNameName,
            cancellationToken: cancellationToken
        );
        aggregate.LoadFromHistory(data);
        _logger.LogDebug("Aggregate with id {Id} loaded", id);
        return aggregate;
    }

    public async Task<T> Load<T>(
        T aggregate,
        IEvent lastEventToLoad,
        CancellationToken cancellationToken = default
    )
        where T : Aggregate
    {
        string id = aggregate.Id;
        string streamNameName = _streamNameResolver.StreamForAggregate(aggregate);
        _logger.LogDebug("Loading aggregate with id {Id} from stream {Stream}", id, streamNameName);
        List<IEvent> data = await _eventStore.ReadStream(
            streamNameName,
            lastEventToLoad,
            cancellationToken: cancellationToken
        );
        aggregate.LoadFromHistory(data);
        _logger.LogDebug("Aggregate with id {Id} loaded", id);
        return aggregate;
    }

    public async Task<T> GetAggregateById<T>(
        string id,
        CancellationToken cancellationToken = default
    )
        where T : Aggregate, new()
    {
        string streamNameName = _streamNameResolver.StreamForAggregate<T>(id);
        _logger.LogDebug("Loading aggregate with id {Id} from stream {Stream}", id, streamNameName);
        List<IEvent> data = await _eventStore.ReadStream(
            streamNameName,
            cancellationToken: cancellationToken
        );
        T aggregate = new();
        aggregate.LoadFromHistory(data);
        _logger.LogDebug("Aggregate with id {Id} loaded", id);
        return aggregate;
    }

    public async Task<T> GetAggregateById<T>(
        string id,
        IEvent lastEventToLoad,
        CancellationToken cancellationToken = default
    )
        where T : Aggregate, new()
    {
        string streamNameName = _streamNameResolver.StreamForAggregate<T>(id);
        _logger.LogDebug("Loading aggregate with id {Id} from stream {Stream}", id, streamNameName);
        List<IEvent> history = await _eventStore.ReadStream(
            streamNameName,
            lastEventToLoad,
            cancellationToken: cancellationToken
        );
        T aggregate = new();
        aggregate.LoadFromHistory(history);
        _logger.LogDebug("Aggregate with id {Id} loaded", id);
        return aggregate;
    }

    public async Task<T> GetAggregateFromStream<T>(
        string streamName,
        CancellationToken cancellationToken = default
    )
        where T : Aggregate, new()
    {
        string id = _streamNameResolver.AggregateIdForStream(streamName);
        _logger.LogDebug("Loading aggregate with id {Id} from stream {Stream}", id, streamName);
        List<IEvent> data = await _eventStore.ReadStream(
            streamName,
            cancellationToken: cancellationToken
        );
        T aggregate = new();

        aggregate.LoadFromHistory(data);
        _logger.LogDebug("Aggregate with id {Id} loaded", streamName);
        return aggregate;
    }

    public async Task<T> GetAggregateFromStream<T>(
        string streamName,
        IEvent lastEventToLoad,
        CancellationToken cancellationToken = default
    )
        where T : Aggregate, new()
    {
        string id = _streamNameResolver.AggregateIdForStream(streamName);
        _logger.LogDebug("Loading aggregate with id {Id} from stream {Stream}", id, streamName);
        List<IEvent> data = await _eventStore.ReadStream(
            streamName,
            cancellationToken: cancellationToken
        );
        T aggregate = new();
        IEnumerable<IEvent> history = data.TakeWhile(
            x => x.Timestamp < lastEventToLoad.Timestamp || x.Id == lastEventToLoad.Id
        );

        aggregate.LoadFromHistory(history);
        _logger.LogDebug("Aggregate with id {Id} loaded", streamName);
        return aggregate;
    }
}
