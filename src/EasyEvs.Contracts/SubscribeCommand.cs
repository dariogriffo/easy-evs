namespace EasyEvs.Contracts
{
    /// <summary>
    /// Command to subscribe to persistent a persistent subscription
    /// </summary>
    public class SubscribeCommand
    {
        /// <summary>
        /// The constructor
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="treatMissingHandlersAsErrors"></param>
        public SubscribeCommand(string stream, bool treatMissingHandlersAsErrors)
        {
            Stream = stream;
            TreatMissingHandlersAsErrors = treatMissingHandlersAsErrors;
        }

        /// <summary>
        /// The stream or projection to subscribe to
        /// </summary>
        public string Stream { get; }

        /// <summary>
        /// Flag to indicate what to do in case an event appears and does not have an associated <see cref="IHandlesEvent{T}"/>
        /// Defaults to false
        /// </summary>
        public bool TreatMissingHandlersAsErrors { get; } = false;
    }
}
