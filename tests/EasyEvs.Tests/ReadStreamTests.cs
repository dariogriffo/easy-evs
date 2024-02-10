namespace EasyEvs.Tests;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Events.Orders;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

public class ReadStreamTests
{
    [Fact]
    public async Task With_No_Position_All_Events_AreRetrieved()
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
            );

        await using ServiceProvider provider = services.BuildServiceProvider();
        IEventStore eventStore = provider.GetRequiredService<IEventStore>();
        IReadEventStore readEventStore = provider.GetRequiredService<IReadEventStore>();
        provider.GetRequiredService<IEventsStreamResolver>();
        Guid orderId = Guid.NewGuid();
        OrderCreated e1 = new(orderId);
        OrderCancelled e2 = new(orderId);
        OrderCancelled e3 = new(orderId);
        string streamName = $"order-{orderId.ToString()}";

        await eventStore.Append(streamName, e1, cancellationToken: CancellationToken.None);
        await eventStore.Append(streamName, e2, cancellationToken: CancellationToken.None);
        await eventStore.Append(streamName, e3, cancellationToken: CancellationToken.None);
        List<IEvent> events = await readEventStore.ReadStream(
            streamName,
            cancellationToken: CancellationToken.None
        );
        events.Count.Should().Be(3);
        events[1].Should().BeEquivalentTo(e2);
        events[2].Should().BeEquivalentTo(e3);
    }
}
