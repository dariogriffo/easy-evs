namespace EasyEvs.Tests;

using System;
using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Events.Orders.v2;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Pipelines;
using Xunit;

public class PipelineActionsTests
{
    [Fact]
    public async Task When_Actions_Are_Registered_They_Are_Executed()
    {
        ServiceCollection services = new();
        ICounter counter = Mock.Of<ICounter>();

        services
            .AddSingleton(counter)
            .ConfigureEventStoreDbWithLogging()
            .AddEasyEvs(
                sp => sp.GetEventStoreSettings(),
                c =>
                {
                    c.AssembliesToScanForHandlers = [typeof(OrderEventHandler).Assembly];
                }
            )
            .WithPipeline<OrderEventPipelineAction1>()
            .WithPipeline<OrderEventPipelineAction2>();

        await using ServiceProvider provider = services.BuildServiceProvider();
        IEventStore eventStore = provider.GetRequiredService<IEventStore>();
        Guid orderId = Guid.NewGuid();
        string streamName = $"order-{orderId}";
        OrderEventCancelled @event = new(orderId, "No reason");
        await eventStore.SubscribeToStream(streamName, CancellationToken.None);
        await eventStore.Append(streamName, @event, cancellationToken: CancellationToken.None);
        await Task.Delay(TimeSpan.FromSeconds(2));
        Mock<ICounter> mock = Mock.Get(counter);
        mock.Verify(x => x.Touch(), Times.Exactly(5));
    }
}
