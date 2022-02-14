namespace EasyEvs.Contracts
{
    using System;
    using System.Collections.Generic;
    using ReflectionMagic;

    /// <summary>
    /// A base Aggregate Root
    /// </summary>
    public abstract class AggregateRoot
    {
        private readonly List<IEvent> _events = new List<IEvent>();

        /// <summary>
        /// The id of the Aggregate Root
        /// </summary>
        public string Id { get; protected set; } = string.Empty;

        /// <summary>
        /// All the Events that haven't been stored
        /// </summary>
        public IReadOnlyCollection<IEvent> UncommittedChanges => _events;

        /// <summary>
        /// Clear the uncommitted changes 
        /// </summary>
        public void MarkChangesAsCommitted()
        {
            _events.Clear();
        }

        /// <summary>
        /// Loads an Aggregate Root to its last know state from the history
        /// </summary>
        /// <param name="history">The history of events</param>
        public void LoadFromHistory(IReadOnlyCollection<IEvent> history)
        {
            foreach (var e in history)
            {
                ApplyChange(e, false);
            }
        }

        /// <summary>
        /// Applies a new change of state to the aggregate root, adding the event to the <see cref="UncommittedChanges"/> list.
        /// </summary>
        /// <param name="event"></param>
        protected void ApplyChange(IEvent @event)
        {
            ApplyChange(@event, true);
        }


        /// <summary>
        /// Push atomic aggregate changes to local history for further processing (EventStore.SaveEvents)
        /// </summary>
        /// <param name="event"></param>
        /// <param name="isNew"></param>
        private void ApplyChange(IEvent @event, bool isNew)
        {
            this.AsDynamic().Apply(@event);
            if (isNew)
            {
                _events.Add(@event);
            }
        }
    }
}
