namespace EasyEvs.Internal;

using System.Threading.Tasks;
using global::EventStore.Client;

internal interface IConnectionProvider
{
    EventStorePersistentSubscriptionsClient PersistentSubscriptionClient { get; }

    EventStoreClient ReadClient { get; }

    EventStoreClient WriteClient { get; }

    ValueTask ReadClientDisconnected(EventStoreClient client);

    ValueTask WriteClientDisconnected(EventStoreClient client);

    ValueTask PersistentSubscriptionDisconnected(EventStorePersistentSubscriptionsClient client);
}
