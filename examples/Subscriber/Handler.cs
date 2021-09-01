namespace Subscriber
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using EasyEvs.Contracts;
    using Events;

    public class Handler : IHandlesEvent<UserDeleted>, IHandlesEvent<UserRegistered>, IHandlesEvent<UserUpdated>
    {
        public Task<OperationResult> Handle(UserDeleted @event, IConsumerContext _, CancellationToken cancellationToken)
        {
            Console.WriteLine($"User Deleted {@event.UserId}");
            return Task.FromResult(OperationResult.Ok);
        }
        
        public Task<OperationResult> Handle(UserRegistered @event, IConsumerContext _, CancellationToken cancellationToken)
        {
            Console.WriteLine($"User Registered {@event.UserId}");
            return Task.FromResult(OperationResult.Ok);
        }

        public Task<OperationResult> Handle(UserUpdated @event, IConsumerContext _, CancellationToken cancellationToken)
        {
            Console.WriteLine($"User Updated {@event.UserId}");
            return Task.FromResult(OperationResult.Ok);
        }
    }
}
