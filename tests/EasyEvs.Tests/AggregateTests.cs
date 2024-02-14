namespace EasyEvs.Tests;

using System;
using System.Threading;
using System.Threading.Tasks;
using Aggregates;
using Contracts;
using Contracts.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

public class AggregateTests
{
    [Fact]
    public async Task Saved_Aggregate_Is_Correctly_Loaded()
    {
        ServiceCollection services = new();
        ICounter counter = Mock.Of<ICounter>();

        services
            .ConfigureEventStoreTestsDbWithLogging()
            .AddEasyEvs(sp => sp.GetEventStoreSettings())
            .AddEasyEvsAggregates()
            .AddSingleton(counter);

        await using ServiceProvider provider = services.BuildServiceProvider();
        IAggregateStore aggregateStore = provider.GetRequiredService<IAggregateStore>();
        string userId = Guid.NewGuid().ToString();
        User user = User.Create(userId);
        user.Update();
        user.Deactivate();
        await aggregateStore.Save(user, CancellationToken.None);
        User user1 = await aggregateStore.GetAggregateById<User>(
            user.Id,
            cancellationToken: CancellationToken.None
        );
        user1.Sum.Should().Be(11);
        user1.Status.Should().Be(User.UserStatus.Inactive);
    }

    [Fact]
    public async Task Create_Fails_On_Existing_Stream()
    {
        ServiceCollection services = new();
        ICounter counter = Mock.Of<ICounter>();

        services
            .ConfigureEventStoreTestsDbWithLogging()
            .AddEasyEvs(sp => sp.GetEventStoreSettings())
            .AddEasyEvsAggregates()
            .AddSingleton(counter);

        await using ServiceProvider provider = services.BuildServiceProvider();
        IAggregateStore aggregateStore = provider.GetRequiredService<IAggregateStore>();
        string userId = Guid.NewGuid().ToString();

        User user = User.Create(userId);
        User user1 = User.Create(userId);
        await aggregateStore.Create(user, CancellationToken.None);
        Func<Task> act = async () => await aggregateStore.Create(user1, CancellationToken.None);
        await act.Should().ThrowAsync<StreamAlreadyExists>();
    }

    [Fact]
    public async Task Save_And_Load_WorkAsExpected()
    {
        ServiceCollection services = new();
        ICounter counter = Mock.Of<ICounter>();

        services
            .ConfigureEventStoreTestsDbWithLogging()
            .AddEasyEvs(sp => sp.GetEventStoreSettings())
            .AddEasyEvsAggregates()
            .AddSingleton(counter);

        await using ServiceProvider provider = services.BuildServiceProvider();
        IAggregateStore aggregateStore = provider.GetRequiredService<IAggregateStore>();
        string userId = Guid.NewGuid().ToString();
        User user = User.Create(userId);
        user.Update();
        user.Deactivate();
        await user.Save(aggregateStore, CancellationToken.None);

        User user1 = User.Create(userId);
        await user1.Load(aggregateStore, CancellationToken.None);
        user1.Should().BeEquivalentTo(user, c => c.Excluding(u => u.UncommittedChanges));
    }
}
