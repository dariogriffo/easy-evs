namespace EasyEvs.Internal;

using global::EventStore.Client;

internal interface IConnectionProvider
{
    EventStorePersistentSubscriptionsClient PersistentSubscriptionClient { get; }

    EventStoreClient ReadClient { get; }

    EventStoreClient WriteClient { get; }

    void ReadClientDisconnected(EventStoreClient client);
    
    void WriteClientDisconnected(EventStoreClient client);
    
    void PersistentSubscriptionDisconnected(EventStorePersistentSubscriptionsClient client);
}
