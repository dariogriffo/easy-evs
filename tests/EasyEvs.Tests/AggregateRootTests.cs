namespace EasyEvs.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts;
    using Events.Users;
    using FluentAssertions;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    public class AggregateRootTests
    {
        [Fact]
        public async Task Saved_Aggregate_Is_Correctly_Loaded()
        {
            var services = new ServiceCollection();
            var dict =
                new Dictionary<string, string>() {
                {
                    "EasyEvs:ConnectionString", "esdb://localhost:2113?tls=false"
                }};

            var conf = new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
            services
                .AddLogging(configure => configure.AddConsole().SetMinimumLevel(LogLevel.Debug))
                .AddSingleton((IConfiguration)conf);

            var configuration = new EasyEvsDependencyInjectionConfiguration()
            {
                StreamResolver = typeof(StreamResolver)
            };

            services.AddEasyEvs(configuration);
            var counter = Mock.Of<ICounter>();
            services.AddSingleton(counter);
            var provider = services.BuildServiceProvider();
            var eventStore = provider.GetRequiredService<IEventStore>();
            var userId = Guid.NewGuid();

            var user = new User();
            user.Create(userId);
            user.Update();
            user.Deactivate();
            await eventStore.Save(user, CancellationToken.None);
            var user1 = await eventStore.Get<User>(user.Id, CancellationToken.None);
            user1.Sum.Should().Be(111);
        }

        [Fact]
        public async Task Create_Fails_On_Existing_Stream()
        {
            var services = new ServiceCollection();
            var dict =
                new Dictionary<string, string>() {
                {
                    "EasyEvs:ConnectionString", "esdb://localhost:2113?tls=false"
                }};

            var conf = new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
            services
                .AddLogging(configure => configure.AddConsole().SetMinimumLevel(LogLevel.Debug))
                .AddSingleton((IConfiguration)conf);

            var configuration = new EasyEvsDependencyInjectionConfiguration()
            {
                StreamResolver = typeof(StreamResolver)
            };

            services.AddEasyEvs(configuration);
            var counter = Mock.Of<ICounter>();
            services.AddSingleton(counter);
            var provider = services.BuildServiceProvider();
            var eventStore = provider.GetRequiredService<IEventStore>();
            var userId = Guid.NewGuid();

            var user = new User();
            user.Create(userId);
            var user1 = new User();
            user1.Create(userId);
            await eventStore.Create(user, CancellationToken.None);
            Func<Task> act = async () => await eventStore.Create(user1, CancellationToken.None);
            await act.Should().ThrowAsync<StreamAlreadyExists>();
        }
    }
}
