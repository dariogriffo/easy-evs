namespace EasyEvs.Contracts;

using System;

/// <summary>
/// An exception representing the a failed to create a stream with a duplicate id
/// </summary>
public class StreamAlreadyExists : Exception
{
    /// <summary>
    /// The constructor
    /// </summary>
    /// <param name="aggregate"></param>
    /// <param name="stream"></param>
    internal StreamAlreadyExists(Aggregate aggregate, string stream)
        : base(
            $"Trying to create stream {stream} for aggregate root {aggregate.GetType()} with id {aggregate.Id}"
        )
    {
        Aggregate = aggregate;
    }

    /// <summary>
    /// The aggregate root that triggered the exception
    /// </summary>
    public Aggregate Aggregate { get; }
}
