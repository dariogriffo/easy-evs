namespace EasyEvs.Internal
{
    using global::EventStore.Client;

    internal interface IConnectionProvider
    {
        EventStorePersistentSubscriptionsClient GetPersistentReadConnection();

        EventStoreClient GetWriteConnection();
    
        EventStoreClient GetReadConnection();

    }
}
