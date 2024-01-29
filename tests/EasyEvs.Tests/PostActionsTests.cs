namespace EasyEvs.Tests;

using System;
using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Events.Orders;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

public class PostActionsTests
{
    [Fact]
    public async Task When_Actions_Are_Registered_They_Are_Executed()
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
        OrderDelivered @event = new(Guid.NewGuid(), DateTime.UtcNow, orderId);
        await eventStore.SubscribeToStream(
            streamProvider.StreamForEvent<OrderDelivered>(orderId.ToString()),
            CancellationToken.None
        );
        await eventStore.Append(
            orderId.ToString(),
            @event,
            cancellationToken: CancellationToken.None
        );
        await Task.Delay(TimeSpan.FromSeconds(1));
        Mock<ICounter> mock = Mock.Get(counter);
        mock.Verify(x => x.Touch(), Times.Exactly(3));
    }
}
