namespace EasyEvs.Internal
{
    using System.Text.Json;
    using System.Text.Json.Serialization;

    internal class JsonSerializerOptionsProvider : IJsonSerializerOptionsProvider
    {
        public JsonSerializerOptions Options => new()
        {
            Converters = { new JsonStringEnumConverter() },
            IgnoreNullValues = true,
        };
    }
}
