namespace EasyEvs.Tests;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Events.Orders;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

public class ConsumerContextTests
{
    [Fact]
    public async Task StreamNameIsCorrectlySet()
    {
        CancellationToken cancellationToken = CancellationToken.None;

        ServiceCollection services = new();
        ICounter counter = Mock.Of<ICounter>();

        services
            .ConfigureEventStoreTestsDbWithLogging()
            .AddEasyEvs(
                sp => sp.GetEventStoreSettings(),
                c =>
                {
                    c.AssembliesToScanForHandlers = [typeof(OrderEventHandler).Assembly];
                }
            )
            .AddSingleton(counter);

        await using ServiceProvider provider = services.BuildServiceProvider();
        IEventStore eventStore = provider.GetRequiredService<IEventStore>();
        Guid orderId = Guid.NewGuid();
        OrderCreated e1 = new(orderId);
        OrderCancelled e2 = new(orderId);
        OrderRefundRequested e3 = new(orderId);
        string streamName = $"order-{orderId.ToString()}";

        await eventStore.SubscribeToStream(streamName, cancellationToken);
        await eventStore.Append(streamName, e1, cancellationToken: cancellationToken);
        await eventStore.Append(streamName, e2, cancellationToken: cancellationToken);
        await eventStore.Append(streamName, e3, cancellationToken: cancellationToken);
        await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);

        IStreamNames streamNames = provider.GetRequiredService<IStreamNames>();
        streamNames.Count().Should().Be(3);
        streamNames.Streams.Distinct().Should().BeEquivalentTo([streamName]);
    }
}
