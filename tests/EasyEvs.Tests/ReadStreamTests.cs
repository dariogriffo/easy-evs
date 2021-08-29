namespace EasyEvs.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Events.Orders;
    using FluentAssertions;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    public class ReadStreamTests
    {
        [Fact]
        public async Task With_No_Position_All_Events_AreRetrieved()
        {
            var services = new ServiceCollection();
            var dict =
                new Dictionary<string, string>() {
                {
                    "EasyEvs:ConnectionString", "esdb://localhost:2113?tls=false"
                }};

            var conf = new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
            services
                .AddLogging(configure => configure.AddConsole())
                .AddSingleton((IConfiguration)conf);

            var configuration = new EasyEvsDependencyInjectionConfiguration()
            {
                StreamResolver = typeof(StreamResolver),
                Assemblies = new[] { typeof(OrderEventHandler).Assembly }
            };

            services.AddEasyEvs(configuration);
            var counter = Mock.Of<ICounter>();
            services.AddSingleton(counter);
            var provider = services.BuildServiceProvider();
            var eventStore = provider.GetRequiredService<IEventStore>();
            var streamProvider = provider.GetRequiredService<IStreamResolver>();
            var OrderId = Guid.NewGuid();
            var e1 = new OrderEvent1(Guid.NewGuid(), DateTime.UtcNow, "v1", OrderId);
            var e2 = new OrderEvent2(Guid.NewGuid(), DateTime.UtcNow, "v1", OrderId);
            var e3 = new OrderEvent2(Guid.NewGuid(), DateTime.UtcNow, "v1", OrderId);
            await eventStore.Append(e1, cancellationToken: CancellationToken.None);
            await eventStore.Append(e2, cancellationToken: CancellationToken.None);
            await eventStore.Append(e3, cancellationToken: CancellationToken.None);
            var events = await eventStore.ReadStream(streamProvider.StreamForEvent(e1), cancellationToken: CancellationToken.None);
            events.Count.Should().Be(3);
            events[0].Item1.Should().BeEquivalentTo(e1);
            events[1].Item1.Should().BeEquivalentTo(e2);
            events[2].Item1.Should().BeEquivalentTo(e3);
        }

        [Fact]
        public async Task With_Position_Correct_Events_AreRetrieved()
        {
            var services = new ServiceCollection();
            var dict =
                new Dictionary<string, string>() {
                {
                    "EasyEvs:ConnectionString", "esdb://localhost:2113?tls=false"
                }};

            var conf = new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
            services
                .AddLogging(configure => configure.AddConsole())
                .AddSingleton((IConfiguration)conf);

            var configuration = new EasyEvsDependencyInjectionConfiguration()
            {
                StreamResolver = typeof(StreamResolver),
                Assemblies = new[] { typeof(OrderEventHandler).Assembly }
            };

            services.AddEasyEvs(configuration);
            var counter = Mock.Of<ICounter>();
            services.AddSingleton(counter);
            var provider = services.BuildServiceProvider();
            var eventStore = provider.GetRequiredService<IEventStore>();
            var streamProvider = provider.GetRequiredService<IStreamResolver>();
            var OrderId = Guid.NewGuid();
            var e1 = new OrderEvent1(Guid.NewGuid(), DateTime.UtcNow, "v1", OrderId);
            var e2 = new OrderEvent2(Guid.NewGuid(), DateTime.UtcNow, "v1", OrderId);
            var e3 = new OrderEvent2(Guid.NewGuid(), DateTime.UtcNow, "v1", OrderId);
            await eventStore.Append(e1, cancellationToken: CancellationToken.None);
            await eventStore.Append(e2, cancellationToken: CancellationToken.None);
            await eventStore.Append(e3, cancellationToken: CancellationToken.None);
            var events = await eventStore.ReadStream(streamProvider.StreamForEvent(e1), 1, CancellationToken.None);
            events.Count.Should().Be(2);
            events[0].Item1.Should().BeEquivalentTo(e2);
            events[1].Item1.Should().BeEquivalentTo(e3);
        }
    }
}
