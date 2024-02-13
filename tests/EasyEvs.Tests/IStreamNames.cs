namespace EasyEvs.Tests;

using System.Collections.Generic;

public interface IStreamNames
{
    void Add(string streamName);

    int Count();

    public IReadOnlyCollection<string> Streams { get; }
}

internal class StreamNames : IStreamNames
{
    private readonly List<string> _streams = new();

    public void Add(string streamName)
    {
        _streams.Add(streamName);
    }

    public int Count()
    {
        return _streams.Count;
    }

    public IReadOnlyCollection<string> Streams => _streams;
}
