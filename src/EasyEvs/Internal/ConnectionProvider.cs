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

    public ConnectionProvider(
        EventStoreSettings settings
    )
    {
        _settings = settings;
        _read = new Lazy<EventStoreClient>(ClientFactory);
        _write = new Lazy<EventStoreClient>(ClientFactory);
        _persistent = new Lazy<EventStorePersistentSubscriptionsClient>(
            PersistentConnectionFactory
        );
    }

    public EventStorePersistentSubscriptionsClient PersistentSubscriptionClient =>
        _persistent.Value;

    public EventStoreClient ReadClient => _read.Value;

    public EventStoreClient WriteClient => _write.Value;

    public async ValueTask ReadClientDisconnected(EventStoreClient client)
    {
        try
        {
            await client.DisposeAsync();
        }
        catch
        {
            // ignored
        }
        
        _read = new Lazy<EventStoreClient>(ClientFactory);
    }

    public async ValueTask WriteClientDisconnected(EventStoreClient client)
    {
        try
        {
            await client.DisposeAsync();
        }
        catch
        {
            // ignored
        }

        _write = new Lazy<EventStoreClient>(ClientFactory);
    }

    public async ValueTask PersistentSubscriptionDisconnected(EventStorePersistentSubscriptionsClient client)
    {
        try
        {
            await client.DisposeAsync();
        }
        catch
        {
            // ignored
        }
        _persistent = new Lazy<EventStorePersistentSubscriptionsClient>(
            PersistentConnectionFactory
        );
    }

    private EventStoreClient ClientFactory()
    {
        EventStoreClientSettings settings = EventStoreClientSettings.Create(
            _settings.ConnectionString
        );
        return new EventStoreClient(settings);
    }

    private EventStorePersistentSubscriptionsClient PersistentConnectionFactory()
    {
        EventStoreClientSettings settings = EventStoreClientSettings.Create(
            _settings.ConnectionString
        );
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
