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
        ServiceCollection services = new();
        ICounter counter = Mock.Of<ICounter>();

        services.ConfigureEventStoreDb().AddEasyEvs().AddSingleton(counter);

        ServiceProvider provider = services.BuildServiceProvider();
        IEventStore eventStore = provider.GetRequiredService<IEventStore>();
        Guid orderId = Guid.NewGuid();
        OrderCreated e1 = new(Guid.NewGuid(), DateTime.UtcNow, orderId);
        OrderCancelled e2 = new(Guid.NewGuid(), DateTime.UtcNow, orderId);
        OrderRefundRequested e3 = new(Guid.NewGuid(), DateTime.UtcNow, orderId);
        await eventStore.SubscribeToStream($"order-{orderId}", CancellationToken.None);
        await eventStore.Append(
            orderId.ToString(),
            e1,
            $"order-{orderId}",
            cancellationToken: CancellationToken.None
        );
        await eventStore.Append(
            orderId.ToString(),
            e2,
            $"order-{orderId}",
            cancellationToken: CancellationToken.None
        );
        await eventStore.Append(
            orderId.ToString(),
            e3,
            $"order-{orderId}",
            cancellationToken: CancellationToken.None
        );
        List<IEvent> events = await eventStore.ReadStream(
            $"order-{orderId}",
            cancellationToken: CancellationToken.None
        );
        events.Count.Should().Be(3);
    }
}
