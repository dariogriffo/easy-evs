namespace Subscriber.Actions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using EasyEvs.Contracts;
    using Events;

    public class UserMetricsPipeline
        : IPipelineHandlesEventAction<UserRegistered>,
            IPipelineHandlesEventAction<UserUpdated>,
            IPipelineHandlesEventAction<UserDeleted>
    {
        public async Task<OperationResult> Execute(
            UserRegistered @event,
            IConsumerContext context,
            Func<Task<OperationResult>> next,
            CancellationToken cancellationToken
        )
        {
            var time = new Stopwatch();
            time.Start();
            var result = await next();
            time.Stop();
            Console.WriteLine($"UserRegistered processed in {time.ElapsedMilliseconds} ms");
            return result;
        }

        public async Task<OperationResult> Execute(
            UserUpdated @event,
            IConsumerContext context,
            Func<Task<OperationResult>> next,
            CancellationToken cancellationToken
        )
        {
            var time = new Stopwatch();
            time.Start();
            var result = await next();
            time.Stop();
            Console.WriteLine($"UserUpdated processed in {time.ElapsedMilliseconds} ms");
            return result;
        }

        public async Task<OperationResult> Execute(
            UserDeleted @event,
            IConsumerContext context,
            Func<Task<OperationResult>> next,
            CancellationToken cancellationToken
        )
        {
            var time = new Stopwatch();
            time.Start();
            var result = await next();
            time.Stop();
            Console.WriteLine($"UserDeleted processed in {time.ElapsedMilliseconds} ms");
            return result;
        }
    }
}
