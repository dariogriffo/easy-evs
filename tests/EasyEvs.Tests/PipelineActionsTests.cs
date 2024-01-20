namespace EasyEvs.Tests;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Events.Orders.v2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class PipelineActionsTests
{
    [Fact]
    public async Task When_Actions_Are_Registered_They_Are_Executed()
    {
        ServiceCollection services = new();
        Dictionary<string, string> dict =
            new() { { "EasyEvs:ConnectionString", "esdb://localhost:2113?tls=false" } };

        IConfigurationRoot conf = new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
        services.AddLogging(configure => configure.AddConsole()).AddSingleton((IConfiguration)conf);

        EasyEvsDependencyInjectionConfiguration configuration =
            new()
            {
                DefaultStreamResolver = true,
                Assemblies = new[] { typeof(OrderEventHandler).Assembly }
            };

        services
            .AddEasyEvs(configuration)
            .WithPipeline<OrderEventPipelineAction1>()
            .WithPipeline<OrderEventPipelineAction2>();

        ICounter counter = Mock.Of<ICounter>();
        services.AddSingleton(counter);
        ServiceProvider provider = services.BuildServiceProvider();
        IEventStore eventStore = provider.GetRequiredService<IEventStore>();
        IStreamResolver streamProvider = provider.GetRequiredService<IStreamResolver>();
        Guid orderId = Guid.NewGuid();
        OrderEventCancelled @event = new(Guid.NewGuid(), DateTime.UtcNow, orderId, "No reason");
        await eventStore.SubscribeToStream(
            streamProvider.StreamForEvent<OrderEventCancelled>(orderId.ToString()),
            CancellationToken.None
        );
        await eventStore.Append(
            orderId.ToString(),
            @event,
            cancellationToken: CancellationToken.None
        );
        await Task.Delay(TimeSpan.FromSeconds(2));
        Mock<ICounter> mock = Mock.Get(counter);
        mock.Verify(x => x.Touch(), Times.Exactly(5));
    }
}
