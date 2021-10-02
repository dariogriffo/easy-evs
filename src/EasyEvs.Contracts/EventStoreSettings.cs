namespace EasyEvs.Contracts
{
    using System;
    using System.Reflection;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// The configuration for the <see cref="IEventStore"/>
    /// </summary>
    public class EventStoreSettings
    {
        /// <summary>
        /// The configuration section name to be provided in the <see cref="Microsoft.Extensions.Configuration.IConfiguration"/>
        /// </summary>
        public static readonly string ConfigurationSectionName = "EasyEvs";

        /// <summary>
        /// The constructor
        /// </summary>
        /// <param name="configuration">The <see cref="Microsoft.Extensions.Configuration.IConfiguration"/> with a section called EasyEvs for this class</param>
        public EventStoreSettings(IConfiguration configuration)
        {
            configuration.GetSection(ConfigurationSectionName).Bind(this);
            if (string.IsNullOrEmpty(ConnectionString))
            {
                throw new ArgumentException("EasyEvs configuration is invalid");
            }

            SubscriptionGroup ??= Assembly.GetEntryAssembly()!.GetName().Name!.ToLowerInvariant();
        }

        /// <summary>
        /// The connection string to access the EventStore
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// The subscription group for the persistent subscription.
        /// If not configured the Executing assembly name is used instead.
        /// This can be dangerous, but also really useful.
        /// </summary>
        public string SubscriptionGroup { get; set; }

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
        /// The interval between connections to retry upon subscriptions dropped.
        /// Default value is 1 second
        /// </summary>
        public TimeSpan SubscriptionReconnectionInterval { get; set; } = TimeSpan.FromSeconds(1);

        /// <summary>
        /// The amount of connection retries.
        /// Default DOES NOT retry.
        /// Set -1 for infinite retries.
        /// </summary>
        public int SubscriptionReconnectionAttempts { get; set; } = 0;

        /// <summary>
        /// The interval between connections to retry upon write failures.
        /// Default value is 1 second
        /// </summary>
        public TimeSpan WriteReconnectionInterval { get; set; } = TimeSpan.FromSeconds(1);

        /// <summary>
        /// The amount of connection retries to write.
        /// Default DOES NOT retry.
        /// Set -1 for infinite retries.
        /// </summary>
        public int WriteReconnectionAttempts { get; set; } = 0;

        /// <summary>
        /// The interval between connections to retry upon read failures.
        /// Default value is 1 second
        /// </summary>
        public TimeSpan ReadReconnectionInterval { get; set; } = TimeSpan.FromSeconds(1);

        /// <summary>
        /// The amount of connection retries to write.
        /// Default DOES NOT retry.
        /// Set -1 for infinite retries.
        /// </summary>
        public int ReadReconnectionAttempts { get; set; } = 0;
    }
}
