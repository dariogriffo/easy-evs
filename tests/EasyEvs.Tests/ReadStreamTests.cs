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
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class ReadStreamTests
{
    [Fact]
    public async Task With_No_Position_All_Events_AreRetrieved()
    {
        ServiceCollection services = new();
        Dictionary<string, string> dict =
            new() { { "EasyEvs:ConnectionString", "esdb://localhost:2113?tls=false" } };

        IConfigurationRoot conf = new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
        services.AddLogging(configure => configure.AddConsole()).AddSingleton((IConfiguration)conf);

        EasyEvsDependencyInjectionConfiguration configuration =
            new()
            {
                DefaultStreamResolver = true,
                Assemblies = new[] { typeof(OrderEventHandler).Assembly }
            };

        services.AddEasyEvs(configuration);
        ICounter counter = Mock.Of<ICounter>();
        services.AddSingleton(counter);
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
        ServiceCollection services = new();
        Dictionary<string, string> dict =
            new() { { "EasyEvs:ConnectionString", "esdb://localhost:2113?tls=false" } };

        IConfigurationRoot conf = new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
        services.AddLogging(configure => configure.AddConsole()).AddSingleton((IConfiguration)conf);

        EasyEvsDependencyInjectionConfiguration configuration =
            new()
            {
                DefaultStreamResolver = true,
                Assemblies = new[] { typeof(OrderEventHandler).Assembly }
            };

        services.AddEasyEvs(configuration);
        ICounter counter = Mock.Of<ICounter>();
        services.AddSingleton(counter);
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
