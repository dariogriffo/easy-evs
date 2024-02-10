namespace EasyEvs.Contracts;

using System;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// The interface that gives access to write to https://github.com/EventStore/EventStore
/// </summary>
public interface IReconnectionStrategy
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="func"></param>
    /// <returns></returns>
    void Retry(Action func);
}
