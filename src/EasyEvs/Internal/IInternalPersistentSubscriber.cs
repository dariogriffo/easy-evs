namespace EasyEvs.Internal;

using System;
using System.Threading;
using System.Threading.Tasks;
using global::EventStore.Client;

internal interface IInternalPersistentSubscriber
{
    Task Subscribe(
        string streamName,
        Func<PersistentSubscription, ResolvedEvent, int?, CancellationToken, Task> onEventAppeared,
        CancellationToken cancellationToken
    );
}
