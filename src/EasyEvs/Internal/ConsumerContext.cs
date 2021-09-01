namespace EasyEvs.Internal
{
    using System;
    using System.Collections.Generic;
    using Contracts;

    internal class ConsumerContext : IConsumerContext
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
