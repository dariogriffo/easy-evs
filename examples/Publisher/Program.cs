namespace Publisher;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using EasyEvs;
using EasyEvs.Contracts;
using Events;
using Microsoft.Extensions.DependencyInjection;

static class Program
{
    static async Task Main(string[] args)
    {
        var fixture = new Fixture();
        var services = new ServiceCollection();

        services
            .ConfigureEventStoreDbWithLogging()
            .AddEasyEvs(sp => sp.GetEventStoreSettings())
            .AddEasyEvsAggregates();

        var provider = services.BuildServiceProvider();
        var eventStore = provider.GetRequiredService<IEventStore>();

        for (var i = 0; i < 500; ++i)
        {
            var @event = fixture.Create<UserRegistered>();
            var event1 = fixture.Create<UserUpdated>();
            var event2 = fixture.Create<UserDeleted>();

            event1.UserId = event2.UserId = @event.UserId;
            @event.Timestamp = DateTime.UtcNow;
            @event.Metadata = new Dictionary<string, string>()
            {
                { "created-by", Guid.NewGuid().ToString() }
            };

            string aggregateId = @event.UserId.ToString();

            await eventStore.Append($"user-{aggregateId}", @event);
            event1.Timestamp = DateTime.UtcNow;
            event1.Metadata = new Dictionary<string, string>()
            {
                { "updated-by", Guid.NewGuid().ToString() }
            };

            await eventStore.Append($"user-{aggregateId}", @event1);

            event2.Timestamp = DateTime.UtcNow;
            event2.Metadata = new Dictionary<string, string>()
            {
                { "deleted-by", Guid.NewGuid().ToString() }
            };
            await eventStore.Append($"user-{aggregateId}", @event2);
        }
    }
}
