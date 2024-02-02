namespace EasyEvs.Tests;

using System;
using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Events.Orders.v2;
using Microsoft.Extensions.Configuration;
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
            .ConfigureEventStoreDb()
            .AddEasyEvs(
                sp =>
                    sp.GetRequiredService<IConfiguration>()
                        .GetSection("EasyEvs")
                        .Get<EventStoreSettings>()!,
                c =>
                {
                    c.Assemblies = [typeof(OrderEventHandler).Assembly];
                }
            )
            .WithPipeline<OrderEventPipelineAction1>()
            .WithPipeline<OrderEventPipelineAction2>();

        ServiceProvider provider = services.BuildServiceProvider();
        IEventStore eventStore = provider.GetRequiredService<IEventStore>();
        Guid orderId = Guid.NewGuid();
        string stream = $"orders-{orderId}";
        OrderEventCancelled @event = new(Guid.NewGuid(), DateTime.UtcNow, orderId, "No reason");
        await eventStore.SubscribeToStream(stream, CancellationToken.None);
        await eventStore.Append(stream, @event, cancellationToken: CancellationToken.None);
        await Task.Delay(TimeSpan.FromSeconds(2));
        Mock<ICounter> mock = Mock.Get(counter);
        mock.Verify(x => x.Touch(), Times.Exactly(5));
    }
}
