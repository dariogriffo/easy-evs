namespace EasyEvs.Internal
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Text.Json;
    using Contracts;
    using global::EventStore.Client;

    internal class Serializer : ISerializer
    {
        private static readonly IReadOnlyDictionary<string, string> Empty = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());
        private readonly ConcurrentDictionary<string, Type> _cachedTypes = new();

        private readonly JsonSerializerOptions _options;

        public Serializer(IJsonSerializerOptionsProvider provider)
        {
            _options = provider.Options;
        }

        public IEvent Deserialize(ResolvedEvent resolvedEvent)
        {
            var eventData = resolvedEvent.Event.Data;
            var metadata = JsonSerializer.Deserialize<Dictionary<string, string>>(resolvedEvent.Event.Metadata.Span)!;
            var type = _cachedTypes.GetOrAdd(metadata["easy.evs.assembly.qualified.name"], (s) =>
                Type.GetType(metadata["easy.evs.assembly.qualified.name"])!
            );
            var @event = JsonSerializer.Deserialize(eventData.Span, type, _options) as IEvent;
            var nonEvsKeys = metadata.Count(x => x.Key.StartsWith("easy.evs") == false);
            var correlationId =
                metadata.TryGetValue("correlationId", out var value) ? value :
                metadata.TryGetValue("$correlationId", out value) ? value :
                metadata.TryGetValue("easy.evs.correlation.id", out value) ? value :
                Guid.NewGuid().ToString();

            Trace.CorrelationManager.ActivityId = Guid.Parse(correlationId);
            
            if (nonEvsKeys <= 0)
            {
                return @event!;
            }

            {
                Dictionary<string, string> dictionary = 
                    metadata
                        .Where(x => x.Key.StartsWith("easy.evs") == false)
                        .ToDictionary(x => x.Key, x => x.Value);
                @event!.Metadata = new ReadOnlyDictionary<string, string>(dictionary);
            }

            return @event!;
        }

        public EventData Serialize<T>(T @event) where T : IEvent
        {
            var eventType = @event.GetType();
            var version = @event.Version;
            var eventMetadata = @event.Metadata;

            var metadata = new Dictionary<string, string>(eventMetadata?.Count + 4 ?? 4)
            {
                {"easy.evs.version", version},
                {"easy.evs.event.type", eventType.Name},
                {"easy.evs.assembly.qualified.name", eventType.AssemblyQualifiedName!},
                {"easy.evs.timestamp", @event.Timestamp.ToUniversalTime().ToString("O")}
            };
            
            var hasCorrelationId = false;
            if (eventMetadata != null)
            {
                foreach (var (key, value) in eventMetadata)
                {
                    if (key == "correlationId" || key == "$correlationId")
                    {
                        hasCorrelationId = true;
                    }

                    metadata.Add(key, value);
                }

                @event.Metadata = null;
            }

            if (!hasCorrelationId)
            {
                var correlationId = Trace.CorrelationManager.ActivityId != Guid.Empty
                    ? Trace.CorrelationManager.ActivityId
                    : Guid.NewGuid();

                metadata["easy.evs.correlation.id"] = correlationId.ToString();
            }

            var eventBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event, _options));
            var metadataBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(metadata, _options));
            return new EventData(Uuid.FromGuid(@event.Id), eventType.Name, eventBytes, metadataBytes);
        }
    }
}
