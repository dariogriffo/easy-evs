namespace EasyEvs.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts;
    using Events.Orders;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;

    public class PreActionsTests
    {
        [Fact]
        public async Task When_Actions_Are_Registered_They_Are_Executed()
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

            services.AddScoped<IPreHandlesEventAction<OrderEvent4>, OrderEventPreAction1>();
            services.AddScoped<IPreHandlesEventAction<OrderEvent4>, OrderEventPreAction2>();

            services.AddEasyEvs(configuration);
            var counter = Mock.Of<ICounter>();
            services.AddSingleton(counter);
            var provider = services.BuildServiceProvider();
            var eventStore = provider.GetRequiredService<IEventStore>();
            var streamProvider = provider.GetRequiredService<IStreamResolver>();
            var orderId = Guid.NewGuid();
            var @event = new OrderEvent4(Guid.NewGuid(), DateTime.UtcNow, "v1", orderId);
            await eventStore.SubscribeToStream(streamProvider.StreamForEvent(@event), CancellationToken.None);
            await eventStore.Append(@event, cancellationToken: CancellationToken.None);
            await Task.Delay(TimeSpan.FromSeconds(1));
            var mock = Mock.Get(counter);
            mock.Verify(x => x.Touch(), Times.Exactly(3));
        }
    }
}
