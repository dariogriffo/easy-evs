namespace EasyEvs
{
    /// <summary>
    /// An interface required to be implemented in order for the framework to work
    /// This interface has to decide where the event must be sent.
    /// </summary>
    public interface IStreamResolver
    {
        /// <summary>
        /// The name of the stream this specific event is stored in.
        /// </summary>
        /// <typeparam name="T"><see cref="IEvent"/>.</typeparam>
        /// <param name="event">The event.</param>
        /// <returns>The name of the stream.</returns>
        string StreamNameFor<T>(T @event) where T : IEvent;
    }
}
