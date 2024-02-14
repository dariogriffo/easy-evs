namespace EasyEvs.Contracts.Exceptions;

using System;

/// <summary>
/// An exception representing a failure to create a streamName with a duplicate id
/// </summary>
public class StreamAlreadyExists : Exception
{
    /// <summary>
    /// The constructor
    /// </summary>
    /// <param name="streamName"></param>
    internal StreamAlreadyExists(string streamName)
        : base($"Trying to create stream {streamName} but already exists")
    {
        Stream = streamName;
    }

    /// <summary>
    /// The name of the streamName
    /// </summary>
    public string Stream { get; }
}
