namespace EasyEvs
{
    using System;
    using System.Collections.Generic;

    public class ConsumerContext
    {
        internal ConsumerContext(Guid correlationId,
            IReadOnlyDictionary<string, string> metadata = null,
            int? retryCount = null)
        {
            Metadata = metadata;
            RetryCount = retryCount;
            CorrelationId = correlationId;
        }

        public IReadOnlyDictionary<string, string>? Metadata { get; }

        public int? RetryCount { get; }

        public Guid CorrelationId { get; }
    }
}
