namespace EasyEvs
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Context with all the information necessary besides the <see cref="IEvent"/> to be handled
    /// </summary>
    public class ConsumerContext
    {
        internal ConsumerContext(Guid correlationId,
            IReadOnlyDictionary<string, string> metadata,
            int? retryCount = null)
        {
            Metadata = metadata;
            RetryCount = retryCount;
            CorrelationId = correlationId;
        }

        /// <summary>
        /// The metadata associated to the event.
        /// </summary>
        public IReadOnlyDictionary<string, string> Metadata { get; }

        /// <summary>
        /// The amount of times the event has been processed
        /// </summary>
        public int? RetryCount { get; }

        /// <summary>
        /// A unique correlation id. Useful to identify correlated operations across different contexts.
        /// </summary>
        public Guid CorrelationId { get; }
    }
}
