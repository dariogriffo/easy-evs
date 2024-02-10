namespace EasyEvs.Internal;

using System;
using System.Threading.Tasks;
using Contracts;
using global::EventStore.Client;

internal sealed class ConnectionProvider : IConnectionProvider, IAsyncDisposable
{
    private Lazy<EventStoreClient> _read;
    private Lazy<EventStoreClient> _write;
    private Lazy<EventStorePersistentSubscriptionsClient> _persistent;
    private readonly EventStoreSettings _settings;
    private IReconnectionStrategy _reconnectionStrategy;

    public ConnectionProvider(IReconnectionStrategy reconnectionStrategy, EventStoreSettings settings)
    {
        _settings = settings;
        _reconnectionStrategy = reconnectionStrategy;
        _read = new Lazy<EventStoreClient>(ClientFactory);
        _write = new Lazy<EventStoreClient>(ClientFactory);
        _persistent =
            new Lazy<EventStorePersistentSubscriptionsClient>(PersistentConnectionFactory);
    }

    public EventStorePersistentSubscriptionsClient PersistentSubscriptionClient =>
        _persistent.Value;

    public EventStoreClient ReadClient => _read.Value;

    public EventStoreClient WriteClient => _write.Value;

    public void ReadClientDisconnected(EventStoreClient client)
    {
        _read = new Lazy<EventStoreClient>(ClientFactory);
    }

    public void WriteClientDisconnected(EventStoreClient client)
    {
        _write = new Lazy<EventStoreClient>(ClientFactory);
    }

    public void PersistentSubscriptionDisconnected(EventStorePersistentSubscriptionsClient client)
    {
        _persistent =
            new Lazy<EventStorePersistentSubscriptionsClient>(PersistentConnectionFactory);
    }

    private EventStoreClient ClientFactory()
    {
        EventStoreClientSettings settings = EventStoreClientSettings.Create(_settings.ConnectionString);
        return new EventStoreClient(settings);
    }
    
    private EventStorePersistentSubscriptionsClient PersistentConnectionFactory()
    {
        EventStoreClientSettings settings = EventStoreClientSettings.Create(_settings.ConnectionString);
        return new EventStorePersistentSubscriptionsClient(settings);
    }

    public async ValueTask DisposeAsync()
    {
        ValueTask task = _read.IsValueCreated
            ? _read.Value.DisposeAsync()
            : ValueTask.CompletedTask;
        await task;
    }
}
