namespace Subscriber;

using System;
using System.Threading;
using System.Threading.Tasks;
using EasyEvs.Contracts;
using Events;

public class Handler
    : IHandlesEvent<UserDeleted>,
        IHandlesEvent<UserRegistered>,
        IHandlesEvent<UserUpdated>
{
    private readonly IAggregateStore _aggregateStore;

    public Handler(IAggregateStore aggregateStore)
    {
        _aggregateStore = aggregateStore;
    }

    public async Task<OperationResult> Handle(
        UserDeleted @event,
        IConsumerContext _,
        CancellationToken cancellationToken
    )
    {
        var user = await _aggregateStore.GetAggregateFromStream<User>(
            _.StreamName,
            cancellationToken: cancellationToken
        );
        Console.WriteLine($"User Deleted: {user}");
        return OperationResult.Ok;
    }

    public async Task<OperationResult> Handle(
        UserRegistered @event,
        IConsumerContext _,
        CancellationToken cancellationToken
    )
    {
        var user = await _aggregateStore.GetAggregateFromStream<User>(
            _.StreamName,
            cancellationToken: cancellationToken
        );
        Console.WriteLine($"User Registered: {user}");
        return OperationResult.Ok;
    }

    public async Task<OperationResult> Handle(
        UserUpdated @event,
        IConsumerContext _,
        CancellationToken cancellationToken
    )
    {
        var user = await _aggregateStore.GetAggregateFromStream<User>(
            _.StreamName,
            cancellationToken: cancellationToken
        );

        Console.WriteLine($"User Updated: {user}");
        return OperationResult.Ok;
    }
}
