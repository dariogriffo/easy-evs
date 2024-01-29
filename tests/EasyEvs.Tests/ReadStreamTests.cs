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
        EasyEvsDependencyInjectionConfiguration configuration =
            new()
            {
                DefaultStreamResolver = true,
                Assemblies = [typeof(OrderEventHandler).Assembly]
            };

        ServiceCollection services = new();
        ICounter counter = Mock.Of<ICounter>();

        services.AddSingleton(counter).ConfigureEventStoreDb().AddEasyEvs(configuration);

        ServiceProvider provider = services.BuildServiceProvider();
        IEventStore eventStore = provider.GetRequiredService<IEventStore>();
        IStreamResolver streamProvider = provider.GetRequiredService<IStreamResolver>();
        Guid orderId = Guid.NewGuid();
        OrderCreated e1 = new(Guid.NewGuid(), DateTime.UtcNow, orderId);
        OrderCancelled e2 = new(Guid.NewGuid(), DateTime.UtcNow, orderId);
        OrderCancelled e3 = new(Guid.NewGuid(), DateTime.UtcNow, orderId);
        await eventStore.Append(orderId.ToString(), e1, cancellationToken: CancellationToken.None);
        await eventStore.Append(orderId.ToString(), e2, cancellationToken: CancellationToken.None);
        await eventStore.Append(orderId.ToString(), e3, cancellationToken: CancellationToken.None);
        List<IEvent> events = await eventStore.ReadStream(
            streamProvider.StreamForEvent<OrderCancelled>(orderId.ToString()),
            cancellationToken: CancellationToken.None
        );
        events.Count.Should().Be(3);
        events[1].Should().BeEquivalentTo(e2);
        events[2].Should().BeEquivalentTo(e3);
    }

    [Fact]
    public async Task With_Position_Correct_Events_AreRetrieved()
    {
        EasyEvsDependencyInjectionConfiguration configuration =
            new()
            {
                DefaultStreamResolver = true,
                Assemblies = [typeof(OrderEventHandler).Assembly]
            };

        ServiceCollection services = new();
        ICounter counter = Mock.Of<ICounter>();

        services.ConfigureEventStoreDb().AddEasyEvs(configuration).AddSingleton(counter);

        ServiceProvider provider = services.BuildServiceProvider();
        IEventStore eventStore = provider.GetRequiredService<IEventStore>();
        IStreamResolver streamProvider = provider.GetRequiredService<IStreamResolver>();
        Guid orderId = Guid.NewGuid();
        OrderCreated e1 = new(Guid.NewGuid(), DateTime.UtcNow, orderId);
        OrderCancelled e2 = new(Guid.NewGuid(), DateTime.UtcNow, orderId);
        OrderCancelled e3 = new(Guid.NewGuid(), DateTime.UtcNow, orderId);
        await eventStore.Append(orderId.ToString(), e1, cancellationToken: CancellationToken.None);
        await eventStore.Append(orderId.ToString(), e2, cancellationToken: CancellationToken.None);
        await eventStore.Append(orderId.ToString(), e3, cancellationToken: CancellationToken.None);
        List<IEvent> events = await eventStore.ReadStream(
            streamProvider.StreamForEvent<OrderCreated>(orderId.ToString()),
            1,
            CancellationToken.None
        );
        events.Count.Should().Be(2);
        events[0].Should().BeEquivalentTo(e2);
        events[1].Should().BeEquivalentTo(e3);
    }
}
