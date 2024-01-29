namespace EasyEvs.Tests;

using System;
using System.Threading;
using System.Threading.Tasks;
using Aggregates;
using Contracts;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

public class AggregateTests
{
    [Fact]
    public async Task Saved_Aggregate_Is_Correctly_Loaded()
    {
        EasyEvsDependencyInjectionConfiguration configuration =
            new() { DefaultStreamResolver = true, };
        ServiceCollection services = new();
        ICounter counter = Mock.Of<ICounter>();

        services.ConfigureEventStoreDb().AddEasyEvs(configuration).AddSingleton(counter);

        ServiceProvider provider = services.BuildServiceProvider();
        IEventStore eventStore = provider.GetRequiredService<IEventStore>();
        Guid userId = Guid.NewGuid();
        User user = new();
        user.Create(userId);
        user.Update();
        user.Deactivate();
        await eventStore.Save(user, CancellationToken.None);
        User user1 = await eventStore.Get<User>(user.Id, CancellationToken.None);
        user1.Sum.Should().Be(111);
    }

    [Fact]
    public async Task Create_Fails_On_Existing_Stream()
    {
        EasyEvsDependencyInjectionConfiguration configuration =
            new() { DefaultStreamResolver = true, };
        ServiceCollection services = new();
        ICounter counter = Mock.Of<ICounter>();

        services.ConfigureEventStoreDb().AddEasyEvs(configuration).AddSingleton(counter);

        ServiceProvider provider = services.BuildServiceProvider();
        IEventStore eventStore = provider.GetRequiredService<IEventStore>();
        Guid userId = Guid.NewGuid();

        User user = new();
        user.Create(userId);
        User user1 = new();
        user1.Create(userId);
        await eventStore.Create(user, CancellationToken.None);
        Func<Task> act = async () => await eventStore.Create(user1, CancellationToken.None);
        await act.Should().ThrowAsync<StreamAlreadyExists>();
    }

    [Fact]
    public async Task Save_And_Load_WorkAsExpected()
    {
        EasyEvsDependencyInjectionConfiguration configuration =
            new() { DefaultStreamResolver = true, };
        ServiceCollection services = new();
        ICounter counter = Mock.Of<ICounter>();

        services.ConfigureEventStoreDb().AddEasyEvs(configuration).AddSingleton(counter);

        ServiceProvider provider = services.BuildServiceProvider();
        IEventStore eventStore = provider.GetRequiredService<IEventStore>();
        Guid userId = Guid.NewGuid();
        User user = new();
        user.Create(userId);
        user.Update();
        user.Deactivate();
        await user.Save(eventStore, CancellationToken.None);
        User user1 = new(user.Id);
        await user1.Load(eventStore, CancellationToken.None);
        user1.Should().BeEquivalentTo(user);
    }
}
