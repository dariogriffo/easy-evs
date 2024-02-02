namespace EasyEvs.Tests;

using System;
using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Events.Orders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

public class PreActionsTests
{
    [Fact]
    public async Task When_Actions_Are_Registered_They_Are_Executed()
    {
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
        OrderAbandoned @event = new(Guid.NewGuid(), DateTime.UtcNow, orderId);
        string stream = orderId.ToString();
        await eventStore.SubscribeToStream(stream, CancellationToken.None);
        await eventStore.Append(stream, @event, cancellationToken: CancellationToken.None);
        await Task.Delay(TimeSpan.FromSeconds(1));
        Mock<ICounter> mock = Mock.Get(counter);
        mock.Verify(x => x.Touch(), Times.Exactly(3));
    }
}
