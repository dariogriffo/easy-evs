namespace Publisher
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoFixture;
    using Configuration.Extensions.EnvironmentFile;
    using EasyEvs;
    using EasyEvs.Contracts;
    using Events;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    class Program
    {
        static async Task Main(string[] args)
        {
            var conf = new ConfigurationBuilder().AddEnvironmentFile().Build();
            var fixture = new Fixture();
            var services = new ServiceCollection();
            var configuration = new EasyEvsConfiguration()
            {
                Assemblies = new[] { typeof(Program).Assembly }
            };
            services
                .AddLogging(configure => configure.SetMinimumLevel(LogLevel.Debug).AddConsole())
                .AddSingleton((IConfiguration)conf)
                .AddEasyEvs(configuration);

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

                await eventStore.Append(aggregateId, @event);
                event1.Timestamp = DateTime.UtcNow;
                event1.Metadata = new Dictionary<string, string>()
                {
                    { "updated-by", Guid.NewGuid().ToString() }
                };

                await eventStore.Append(aggregateId, @event1);

                event2.Timestamp = DateTime.UtcNow;
                event2.Metadata = new Dictionary<string, string>()
                {
                    { "deleted-by", Guid.NewGuid().ToString() }
                };
                await eventStore.Append(aggregateId, @event2);
            }
        }
    }
}
