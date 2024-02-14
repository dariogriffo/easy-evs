namespace EasyEvs.Internal;

using System;
using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Contracts.Exceptions;

internal sealed class NoReconnectionStrategy : IConnectionStrategy
{
    public Task Execute(
        Func<CancellationToken, Task> func,
        CancellationToken cancellationToken = default
    )
    {
        return func(cancellationToken);
    }
}

internal sealed class BasicReconnectionStrategy : IConnectionStrategy
{
    private int _retry = 0;

    public async Task Execute(
        Func<CancellationToken, Task> func,
        CancellationToken cancellationToken = default
    )
    {
        Exception? last = null;
        while (_retry < 4)
        {
            try
            {
                await func(cancellationToken);
                _retry = 0;
                return;
            }
            catch (ConnectionFailureException ex)
            {
                last = ex;
                Console.WriteLine("Retry");
                ++_retry;
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
        }

        if (last is not null)
        {
            throw last;
        }
    }
}
