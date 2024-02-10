namespace EasyEvs.Internal;

using System;
using Contracts;

internal sealed class NoReconnectionStrategy : IReconnectionStrategy
{
    public void Retry(Action func)
    {
        func();
    }
}
