namespace EasyEvs.Contracts;

/// <summary>
/// The configuration for the <see cref="IEventStore"/>
/// </summary>
public class EventStoreSettings
{
    /// <summary>
    /// The connection string to access the EventStore
    /// </summary>
    public string ConnectionString { get; set; } = null!;

    /// <summary>
    /// The group for the persistent subscription.
    /// Required
    /// This can be dangerous, but also really useful.
    /// </summary>
    public string SubscriptionGroup { get; set; } = null!;

    /// <summary>
    /// The buffer size of the persistent subscription
    /// </summary>
    public int SubscriptionBufferSize { get; set; } = 10;

    /// <summary>
    /// Configure the behavior of the event store if the subscription gets dropped
    /// </summary>
    public bool ReconnectOnSubscriptionDropped { get; set; } = true;

    /// <summary>
    /// Configure if the events don't have a registered handler to log a warning message and Park them
    /// </summary>
    public bool TreatMissingHandlersAsErrors { get; set; } = false;

    /// <summary>
    /// Configure is the subscription resolve events.
    /// https://developers.eventstore.com/clients/dotnet/5.0/reading.html#resolvedevent
    /// </summary>
    public bool ResolveEvents { get; set; }
}
