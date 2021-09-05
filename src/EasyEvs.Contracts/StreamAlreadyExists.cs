namespace EasyEvs.Contracts
{
    using System;

    /// <summary>
    /// An exception representing the a failed to create a stream with a duplicate id
    /// </summary>
    public class StreamAlreadyExists : Exception
    {
        /// <summary>
        /// The constructor
        /// </summary>
        /// <param name="aggregateRoot"></param>
        /// <param name="stream"></param>
        internal StreamAlreadyExists(AggregateRoot aggregateRoot, string stream)
            :base($"Trying to create stream {stream} for aggregate root {aggregateRoot.GetType()} with id {aggregateRoot.Id}")
        {
            AggregateRoot = aggregateRoot;
        }

        /// <summary>
        /// The aggregate root that triggered the exception
        /// </summary>
        public AggregateRoot AggregateRoot { get; }
    }
}
