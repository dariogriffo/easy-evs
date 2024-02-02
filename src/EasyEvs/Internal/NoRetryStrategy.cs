namespace EasyEvs.Internal;

using System;
using System.Threading.Tasks;
using Contracts;

internal sealed class NoRetryStrategy : IRetryStrategy
{
    public Task Subscribe(Func<Task> func)
    {
        return func();
    }

    public Task Write(Func<Task> func)
    {
        return func();
    }

    public Task Read(Func<Task> func)
    {
        return func();
    }
}
