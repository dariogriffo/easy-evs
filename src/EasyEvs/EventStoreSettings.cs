namespace EasyEvs
{
    using System;
    using System.Reflection;
    using Microsoft.Extensions.Configuration;

    public class EventStoreSettings
    {
        public EventStoreSettings(IConfiguration configuration)
        {
            configuration.GetSection("EasyEvs").Bind(this);
            if (string.IsNullOrEmpty(ConnectionString))
            {
                throw new ArgumentException("EasyEvs configuration is invalid");
            }

            SubscriptionGroup ??= Assembly.GetEntryAssembly().GetName().Name.ToLowerInvariant();
        }

        public string ConnectionString { get; set; }

        public string SubscriptionGroup { get; set; }

        public int SubscriptionBufferSize { get; set; } = 10;

        public bool ResolveLinkTos { get; set; } = true;

        public bool ReconnectOnSubscriptionDropped { get; set; } = true;
    }
}
