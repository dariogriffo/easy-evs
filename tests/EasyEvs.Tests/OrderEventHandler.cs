﻿namespace EasyEvs.Tests
{
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts;
    using Events.Orders;
    using Events.Orders.v2;
    using OrderRefundRequested = Events.Orders.v2.OrderRefundRequested;

    public class OrderEventHandler(ICounter counter, IStreamNames streamNames)
        : IHandlesEvent<OrderCreated>,
            IHandlesEvent<OrderCancelled>,
            IHandlesEvent<Events.Orders.OrderRefundRequested>,
            IHandlesEvent<OrderRefundRequested>,
            IHandlesEvent<OrderEventCancelled>,
            IHandlesEvent<OrderDelivered>,
            IHandlesEvent<OrderAbandoned>
    {
        public Task<OperationResult> Handle(
            OrderCreated @event,
            IConsumerContext context,
            CancellationToken cancellationToken
        )
        {
            streamNames.Add(context.StreamName);
            counter.Touch();
            return Task.FromResult(OperationResult.Ok);
        }

        public Task<OperationResult> Handle(
            OrderCancelled @event,
            IConsumerContext context,
            CancellationToken cancellationToken
        )
        {
            streamNames.Add(context.StreamName);
            counter.Touch();
            return Task.FromResult(OperationResult.Ok);
        }

        public Task<OperationResult> Handle(
            Events.Orders.OrderRefundRequested @event,
            IConsumerContext context,
            CancellationToken cancellationToken
        )
        {
            streamNames.Add(context.StreamName);
            counter.Touch();
            return Task.FromResult(OperationResult.Ok);
        }

        public Task<OperationResult> Handle(
            OrderRefundRequested @event,
            IConsumerContext context,
            CancellationToken cancellationToken
        )
        {
            streamNames.Add(context.StreamName);
            counter.Touch();
            return Task.FromResult(OperationResult.Ok);
        }

        public Task<OperationResult> Handle(
            OrderEventCancelled @event,
            IConsumerContext context,
            CancellationToken cancellationToken
        )
        {
            streamNames.Add(context.StreamName);
            counter.Touch();
            return Task.FromResult(OperationResult.Ok);
        }

        public Task<OperationResult> Handle(
            OrderDelivered @event,
            IConsumerContext context,
            CancellationToken cancellationToken
        )
        {
            streamNames.Add(context.StreamName);
            counter.Touch();
            return Task.FromResult(OperationResult.Ok);
        }

        public Task<OperationResult> Handle(
            OrderAbandoned @event,
            IConsumerContext context,
            CancellationToken cancellationToken
        )
        {
            streamNames.Add(context.StreamName);
            counter.Touch();
            return Task.FromResult(OperationResult.Ok);
        }
    }
}
