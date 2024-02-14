namespace EasyEvs.Internal;

using global::EventStore.Client;
using System.Threading.Tasks;

internal interface IConnectionProvider
{
    EventStorePersistentSubscriptionsClient PersistentSubscriptionClient { get; }

    EventStoreClient ReadClient { get; }

    EventStoreClient WriteClient { get; }

    ValueTask ReadClientDisconnected(EventStoreClient client);

    ValueTask WriteClientDisconnected(EventStoreClient client);

    ValueTask PersistentSubscriptionDisconnected(EventStorePersistentSubscriptionsClient client);
}
