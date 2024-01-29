namespace EasyEvs.Tests;

using System;
using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Events.Orders;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

public class SubscribeTests
{
    [Fact]
    public async Task All_Events_Handled()
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
