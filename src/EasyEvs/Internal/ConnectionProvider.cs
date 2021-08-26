namespace EasyEvs.Internal
{
    using System;
    using System.Threading.Tasks;
    using global::EventStore.Client;

    internal class ConnectionProvider : IConnectionProvider, IAsyncDisposable
    {
        private readonly Lazy<EventStorePersistentSubscriptionsClient> _persistent;
        private readonly Lazy<EventStoreClient> _write;
        private readonly Lazy<EventStoreClient> _read;

        public ConnectionProvider(EventStoreSettings settings)
        {
            _persistent = new Lazy<EventStorePersistentSubscriptionsClient>(() => new EventStorePersistentSubscriptionsClient(EventStoreClientSettings.Create(settings.ConnectionString)));
            _write = new Lazy<EventStoreClient>(() => new EventStoreClient(EventStoreClientSettings.Create(settings.ConnectionString)));
            _read = new Lazy<EventStoreClient>(() => new EventStoreClient(EventStoreClientSettings.Create(settings.ConnectionString)));
        }

        public EventStorePersistentSubscriptionsClient GetPersistentReadConnection() => _persistent.Value;

        public EventStoreClient GetWriteConnection() => _write.Value;

        public EventStoreClient GetReadConnection() => _read.Value;

        public async ValueTask DisposeAsync()
        {
            var t0 = _persistent.IsValueCreated
                ? _persistent.Value.DisposeAsync()
                : ValueTask.CompletedTask;

            var t1 = _write.IsValueCreated
                ? _write.Value.DisposeAsync()
                : ValueTask.CompletedTask;

            var t2 = _read.IsValueCreated
                ? _read.Value.DisposeAsync()
                : ValueTask.CompletedTask;

            await t0; await t1; await t2;
        }
    }
}

