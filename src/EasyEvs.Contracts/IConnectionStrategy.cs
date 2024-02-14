namespace EasyEvs.Contracts;

using System;
using System.Threading;
using System.Threading.Tasks;
using Exceptions;

/// <summary>
/// The interface that gives access to write to https://github.com/EventStore/EventStore
/// <see cref="ConnectionFailureException"/> is thrown on disconnection or failure to connect
/// </summary>
public interface IConnectionStrategy
{
    /// <summary>
    /// The strategy of execution when a connection is broken
    /// </summary>
    /// <param name="func"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task Execute(Func<CancellationToken, Task> func, CancellationToken cancellationToken = default);
}
