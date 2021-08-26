namespace Publisher
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoFixture;
    using EasyEvs;
    using Events;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    class Program
    {
        static async Task Main(string[] args)
        {
            var dict = new Dictionary<string, string>()
            {
                { "EasyEvs:ConnectionString", "esdb://localhost:2113?tls=false" }
            };
            var conf = new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
            var fixture = new Fixture();
            var services = new ServiceCollection();
            services
                .AddLogging(configure => configure.AddConsole())
                .AddSingleton((IConfiguration)conf)
                .AddEasyEvs(typeof(StreamResolver));

            var provider = services.BuildServiceProvider();
            var eventStore = provider.GetRequiredService<IEventStore>();

            for (var i = 0; i < 500; ++i)
            {
                var now = DateTime.UtcNow;
                var @event = fixture.Create<UserRegistered>();
                var @event1 = fixture.Create<UserUpdated>();
                var @event2 = fixture.Create<UserDeleted>();
                @event1.UserId = @event2.UserId = @event.UserId;
                @event1.Timestamp = @event2.Timestamp = @event.Timestamp = now;
                await eventStore.Append(@event, new Dictionary<string, string>() { { "created-by", Guid.NewGuid().ToString() } });
                await eventStore.Append(@event1, new Dictionary<string, string>() { { "updated-by", Guid.NewGuid().ToString() } });
                await eventStore.Append(@event2, new Dictionary<string, string>() { { "deleted-by", Guid.NewGuid().ToString() } });
            }
        }
    }
}
