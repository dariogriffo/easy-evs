namespace EasyEvs.Internal;

using global::EventStore.Client;

internal interface IConnectionProvider
{
    EventStorePersistentSubscriptionsClient PersistentSubscriptionClient { get; }

    EventStoreClient ReadClient { get; }

    EventStoreClient WriteClient { get; }
}
