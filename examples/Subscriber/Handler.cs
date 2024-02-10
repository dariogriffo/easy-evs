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
    private readonly IAggregateStore _eventStore;

    public Handler(IAggregateStore eventStore)
    {
        _eventStore = eventStore;
    }

    public Task<OperationResult> Handle(
        UserDeleted @event,
        IConsumerContext _,
        CancellationToken cancellationToken
    )
    {
        Console.WriteLine($"User Deleted {@event.UserId}");
        return Task.FromResult(OperationResult.Ok);
    }

    public Task<OperationResult> Handle(
        UserRegistered @event,
        IConsumerContext _,
        CancellationToken cancellationToken
    )
    {
        Console.WriteLine($"User Registered {@event.UserId}");
        return Task.FromResult(OperationResult.Ok);
    }

    public async Task<OperationResult> Handle(
        UserUpdated @event,
        IConsumerContext _,
        CancellationToken cancellationToken
    )
    {
        var user = await _eventStore.Get<User>(_.StreamName, cancellationToken);
        Console.WriteLine($"User with id {user.Id} status: {user.Status}");
        return OperationResult.Ok;
    }
}
