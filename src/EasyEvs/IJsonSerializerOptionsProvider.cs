namespace EasyEvs
{
    using System.Text.Json;

    /// <summary>
    /// Interface to provide the options desired to serialize the events in the Event Store
    /// </summary>
    public interface IJsonSerializerOptionsProvider
    {
        /// <summary>
        /// The options used to serialize the events.
        /// Defaults to <see cref="Internal.JsonSerializerOptionsProvider"/>
        /// </summary>
        JsonSerializerOptions Options { get; }
    }
}
