namespace EasyEvs.Tests;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Events.Orders;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
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
            .ConfigureEventStoreDb()
            .AddEasyEvs(
                sp => sp.GetEventStoreSettings(),
                c =>
                {
                    c.AssembliesToScanForHandlers = [typeof(OrderEventHandler).Assembly];
                }
            );

        ServiceProvider provider = services.BuildServiceProvider();
        IEventStore eventStore = provider.GetRequiredService<IEventStore>();
        provider.GetRequiredService<IEventsStreamResolver>();
        Guid orderId = Guid.NewGuid();
        OrderCreated e1 = new(Guid.NewGuid(), DateTime.UtcNow, orderId);
        OrderCancelled e2 = new(Guid.NewGuid(), DateTime.UtcNow, orderId);
        OrderCancelled e3 = new(Guid.NewGuid(), DateTime.UtcNow, orderId);
        string stream = $"orders-{orderId.ToString()}";

        await eventStore.Append(stream, e1, cancellationToken: CancellationToken.None);
        await eventStore.Append(stream, e2, cancellationToken: CancellationToken.None);
        await eventStore.Append(stream, e3, cancellationToken: CancellationToken.None);
        List<IEvent> events = await eventStore.ReadStream(
            stream,
            cancellationToken: CancellationToken.None
        );
        events.Count.Should().Be(3);
        events[1].Should().BeEquivalentTo(e2);
        events[2].Should().BeEquivalentTo(e3);
    }
}
