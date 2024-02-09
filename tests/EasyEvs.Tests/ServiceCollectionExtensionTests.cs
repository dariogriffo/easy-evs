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

public class ServiceCollectionExtensionTests
{
    [Fact]
    public async Task With_No_Parameters_We_Can_Run()
    {
        CancellationToken cancellationToken = CancellationToken.None;
        ServiceCollection services = new();
        ICounter counter = Mock.Of<ICounter>();

        services
            .ConfigureEventStoreDb()
            .AddEasyEvs(sp => sp.GetEventStoreSettings())
            .AddSingleton(counter);

        await using ServiceProvider provider = services.BuildServiceProvider();
        IEventStore eventStore = provider.GetRequiredService<IEventStore>();
        IReadEventStore readEventStore = provider.GetRequiredService<IReadEventStore>();
        Guid orderId = Guid.NewGuid();
        OrderCreated e1 = new(orderId);
        OrderCancelled e2 = new(orderId);
        OrderRefundRequested e3 = new(orderId);
        string streamName = $"order-{orderId}";
        await eventStore.SubscribeToStream(streamName, cancellationToken);
        await eventStore.Append(streamName, e1, cancellationToken: cancellationToken);
        await eventStore.Append(streamName, e2, cancellationToken: cancellationToken);
        await eventStore.Append(streamName, e3, cancellationToken: cancellationToken);
        List<IEvent> events = await readEventStore.ReadStream(
            streamName,
            cancellationToken: cancellationToken
        );
        events.Count.Should().Be(3);
    }
}
