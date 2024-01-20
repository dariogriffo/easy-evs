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

public class ServiceCollectionExtensionTests
{
    [Fact]
    public async Task With_No_Parameters_We_Can_Run()
    {
        var services = new ServiceCollection();
        var dict = new Dictionary<string, string>()
        {
            { "EasyEvs:ConnectionString", "esdb://localhost:2113?tls=false" }
        };

        var conf = new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
        services.AddLogging(configure => configure.AddConsole()).AddSingleton((IConfiguration)conf);

        services.AddEasyEvs();
        var counter = Mock.Of<ICounter>();
        services.AddSingleton(counter);
        var provider = services.BuildServiceProvider();
        var eventStore = provider.GetRequiredService<IEventStore>();
        var orderId = Guid.NewGuid();
        var e1 = new OrderCreated(Guid.NewGuid(), DateTime.UtcNow, orderId);
        var e2 = new OrderCancelled(Guid.NewGuid(), DateTime.UtcNow, orderId);
        var e3 = new OrderRefundRequested(Guid.NewGuid(), DateTime.UtcNow, orderId);
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
        var events = await eventStore.ReadStream(
            $"order-{orderId}",
            cancellationToken: CancellationToken.None
        );
        events.Count.Should().Be(3);
    }
}
