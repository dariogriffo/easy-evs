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
    /// <param name="stream"></param>
    internal StreamAlreadyExists(string stream)
        : base($"Trying to create stream {stream} but already exists")
    {
        Stream = stream;
    }

    /// <summary>
    /// The name of the stream
    /// </summary>
    public string Stream { get; }
}
