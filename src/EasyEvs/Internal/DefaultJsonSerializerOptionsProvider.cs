namespace EasyEvs.Internal;

using System.Text.Json;
using System.Text.Json.Serialization;
using Contracts;

internal sealed class DefaultJsonSerializerOptionsProvider : IJsonSerializerOptionsProvider
{
    public JsonSerializerOptions Options =>
        new()
        {
            Converters = { new JsonStringEnumConverter() },
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        };
}
