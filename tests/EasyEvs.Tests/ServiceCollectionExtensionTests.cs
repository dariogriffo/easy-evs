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

        ServiceProvider provider = services.BuildServiceProvider();
        IEventStore eventStore = provider.GetRequiredService<IEventStore>();
        Guid orderId = Guid.NewGuid();
        OrderCreated e1 = new(Guid.NewGuid(), DateTime.UtcNow, orderId);
        OrderCancelled e2 = new(Guid.NewGuid(), DateTime.UtcNow, orderId);
        OrderRefundRequested e3 = new(Guid.NewGuid(), DateTime.UtcNow, orderId);
        string stream = $"order-{orderId}";
        await eventStore.SubscribeToStream(stream, cancellationToken);
        await eventStore.Append(stream, e1, cancellationToken: cancellationToken);
        await eventStore.Append(stream, e2, cancellationToken: cancellationToken);
        await eventStore.Append(stream, e3, cancellationToken: cancellationToken);
        List<IEvent> events = await eventStore.ReadStream(
            stream,
            cancellationToken: cancellationToken
        );
        events.Count.Should().Be(3);
    }
}
