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
        EasyEvsDependencyInjectionConfiguration configuration =
            new()
            {
                DefaultStreamResolver = true,
                Assemblies = [typeof(OrderEventHandler).Assembly]
            };

        ServiceCollection services = new();
        ICounter counter = Mock.Of<ICounter>();

        services
            .AddSingleton(counter)
            .ConfigureEventStoreDb()
            .AddEasyEvs(configuration)
            .WithPipeline<OrderEventPipelineAction1>()
            .WithPipeline<OrderEventPipelineAction2>();

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
