namespace EasyEvs.Contracts;

using System;

/// <summary>
/// An exception representing that the streamName was not found
/// </summary>
public class StreamNotFound : Exception
{
    /// <summary>
    /// The constructor
    /// </summary>
    /// <param name="streamName"></param>
    internal StreamNotFound(string streamName)
        : base($"Stream {streamName} was not found")
    {
        Stream = streamName;
    }

    /// <summary>
    /// The name of the streamName
    /// </summary>
    public string Stream { get; }
}
