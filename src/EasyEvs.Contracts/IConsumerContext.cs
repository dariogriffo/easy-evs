namespace EasyEvs.Contracts
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Context with all the information necessary besides the <see cref="IEvent"/> to be handled
    /// </summary>
    public interface IConsumerContext
    {
        /// <summary>
        /// The metadata associated to the event.
        /// </summary>
        IReadOnlyDictionary<string, string> Metadata { get; }

        /// <summary>
        /// The amount of times the event has been processed
        /// </summary>
        int? RetryCount { get; }

        /// <summary>
        /// A unique correlation id. Useful to identify correlated operations across different contexts.
        /// </summary>
        Guid CorrelationId { get; }
    }
}
