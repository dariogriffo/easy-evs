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
        CancellationToken cancellationToken = CancellationToken.None;

        ServiceCollection services = new();
        ICounter counter = Mock.Of<ICounter>();

        services
            .ConfigureEventStoreDb()
            .AddEasyEvs(
                sp => sp.GetEventStoreSettings(),
                c =>
                {
                    c.AssembliesToScanForHandlers = [typeof(OrderEventHandler).Assembly];
                }
            )
            .AddSingleton(counter);

        ServiceProvider provider = services.BuildServiceProvider();
        IEventStore eventStore = provider.GetRequiredService<IEventStore>();
        Guid orderId = Guid.NewGuid();
        OrderCreated e1 = new(orderId);
        OrderCancelled e2 = new(orderId);
        OrderRefundRequested e3 = new(orderId);
        string stream = $"orders-{orderId.ToString()}";

        await eventStore.SubscribeToStream(stream, cancellationToken);
        await eventStore.Append(stream, e1, cancellationToken: cancellationToken);
        await eventStore.Append(stream, e2, cancellationToken: cancellationToken);
        await eventStore.Append(stream, e3, cancellationToken: cancellationToken);
        await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);

        Mock<ICounter> mock = Mock.Get(counter);
        mock.Verify(x => x.Touch(), Times.Exactly(3));
    }
}
