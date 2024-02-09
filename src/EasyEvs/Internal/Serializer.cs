namespace EasyEvs.Internal;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Contracts;
using global::EventStore.Client;

internal sealed class Serializer(IJsonSerializerOptionsProvider provider) : ISerializer
{
    private readonly ConcurrentDictionary<string, Type> _cachedTypes = new();
    private readonly JsonSerializerOptions _options = provider.Options;

    public IEvent Deserialize(EventRecord record)
    {
        ReadOnlyMemory<byte> eventData = record.Data;
        Dictionary<string, string>? metadata = JsonSerializer.Deserialize<
            Dictionary<string, string>
        >(record.Metadata.Span, _options)!;
        Type type = _cachedTypes.GetOrAdd(
            metadata["easy.evs.assembly.qualified.name"],
            _ =>
            {
                Type? type1 = Type.GetType(metadata["easy.evs.assembly.qualified.name"]);
                if (type1 is null)
                {
                    string eventFullName = metadata["easy.evs.event.full.name"];
                    string eventAssembly = metadata["easy.evs.event.assembly.name"];
                    type1 = Assembly.Load(eventAssembly).GetType(eventFullName);
                }

                return type1!;
            }
        );

        IEvent? @event = JsonSerializer.Deserialize(eventData.Span, type, _options) as IEvent;
        int nonEvsKeys = metadata.Count(x => x.Key.StartsWith("easy.evs") == false);

        if (nonEvsKeys == 0)
        {
            if (metadata.ContainsKey("easy.evs.empty.metadata"))
            {
                @event!.Metadata = new Dictionary<string, string>();
            }

            return @event!;
        }

        {
            Dictionary<string, string> dictionary = metadata
                .Where(x => x.Key.StartsWith("easy.evs") == false)
                .ToDictionary(x => x.Key, x => x.Value);
            @event!.Metadata = new ReadOnlyDictionary<string, string>(dictionary);
        }

        return @event;
    }

    public EventData Serialize<T>(T @event)
        where T : IEvent
    {
        Type eventType = @event.GetType();

        IReadOnlyDictionary<string, string>? eventMetadata = @event.Metadata;

        Dictionary<string, string> metadata =
            new(eventMetadata?.Count + 4 ?? 4)
            {
                { "easy.evs.event.type", eventType.Name },
                { "easy.evs.event.full.name", eventType.FullName! },
                { "easy.evs.event.assembly.name", eventType.Assembly.GetName().Name! },
                { "easy.evs.assembly.qualified.name", eventType.AssemblyQualifiedName! },
            };

        bool emptyMetadata = false;
        if (eventMetadata is not null)
        {
            emptyMetadata = eventMetadata.Any() == false;
            foreach ((string key, string value) in eventMetadata)
            {
                metadata.Add(key, value);
            }

            @event.Metadata = null;
        }

        if (emptyMetadata)
        {
            metadata.Add("easy.evs.empty.metadata", "true");
        }

        byte[] eventBytes = Encoding.UTF8.GetBytes(
            JsonSerializer.Serialize(@event, @event.GetType(), _options)
        );
        byte[] metadataBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(metadata, _options));
        return new EventData(Uuid.FromGuid(@event.Id), eventType.Name, eventBytes, metadataBytes);
    }
}
