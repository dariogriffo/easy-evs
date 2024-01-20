namespace EasyEvs.Tests;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Events.Orders;
using Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class SubscribeTests
{
    [Fact]
    public async Task All_Events_Handled()
    {
        var services = new ServiceCollection();
        var dict = new Dictionary<string, string>()
        {
            { "EasyEvs:ConnectionString", "esdb://localhost:2113?tls=false" }
        };

        var conf = new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
        services.AddLogging(configure => configure.AddConsole()).AddSingleton((IConfiguration)conf);

        var configuration = new EasyEvsDependencyInjectionConfiguration()
        {
            DefaultStreamResolver = true,
            Assemblies = new[] { typeof(OrderEventHandler).Assembly }
        };

        services.AddEasyEvs(configuration);
        var counter = Mock.Of<ICounter>();
        services.AddSingleton(counter);
        var provider = services.BuildServiceProvider();
        var eventStore = provider.GetRequiredService<IEventStore>();
        var streamProvider = provider.GetRequiredService<IStreamResolver>();
        var orderId = Guid.NewGuid();
        var e1 = new OrderCreated(Guid.NewGuid(), DateTime.UtcNow, orderId);
        var e2 = new OrderCancelled(Guid.NewGuid(), DateTime.UtcNow, orderId);
        var e3 = new OrderRefundRequested(Guid.NewGuid(), DateTime.UtcNow, orderId);
        await eventStore.SubscribeToStream(
            streamProvider.StreamForEvent<OrderCreated>(orderId.ToString()),
            CancellationToken.None
        );
        await eventStore.Append(orderId.ToString(), e1, cancellationToken: CancellationToken.None);
        await eventStore.Append(orderId.ToString(), e2, cancellationToken: CancellationToken.None);
        await eventStore.Append(orderId.ToString(), e3, cancellationToken: CancellationToken.None);
        await Task.Delay(TimeSpan.FromSeconds(1));
        var mock = Mock.Get(counter);
        mock.Verify(x => x.Touch(), Times.Exactly(3));
    }
}
