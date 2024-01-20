namespace EasyEvs.Internal;

using System.Text.Json;
using System.Text.Json.Serialization;
using Contracts;

internal class JsonSerializerOptionsProvider : IJsonSerializerOptionsProvider
{
    public JsonSerializerOptions Options =>
        new()
        {
            Converters = { new JsonStringEnumConverter() },
#if NET5_0
            IgnoreNullValues = true,
#else
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
#endif
        };
}
