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
        OrderRefundRequested e3 = new(Guid.NewGuid(), DateTime.UtcNow, orderId);
        await eventStore.SubscribeToStream(
            streamProvider.StreamForEvent<OrderCreated>(orderId.ToString()),
            CancellationToken.None
        );
        await eventStore.Append(orderId.ToString(), e1, cancellationToken: CancellationToken.None);
        await eventStore.Append(orderId.ToString(), e2, cancellationToken: CancellationToken.None);
        await eventStore.Append(orderId.ToString(), e3, cancellationToken: CancellationToken.None);
        await Task.Delay(TimeSpan.FromSeconds(1));
        Mock<ICounter> mock = Mock.Get(counter);
        mock.Verify(x => x.Touch(), Times.Exactly(3));
    }
}
