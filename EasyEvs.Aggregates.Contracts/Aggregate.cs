namespace EasyEvs.Aggregates.Contracts;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using EasyEvs.Contracts;
using ReflectionMagic;

/// <summary>
/// A base Aggregate NOT thread safe
/// </summary>
public abstract class Aggregate
{
    private readonly List<IEvent> _events = new();

    private bool _new = true;

    /// <summary>
    /// The id of the Aggregate Root
    /// </summary>
    public string Id { get; protected set; } = string.Empty;

    /// <summary>
    /// All the Events that haven't been stored
    /// </summary>
    public IImmutableList<IEvent> UncommittedChanges => _events.ToImmutableList();

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
    public void LoadFromHistory(IEnumerable<IEvent> history)
    {
        foreach (IEvent e in history)
        {
            ApplyChange(e, false);
        }
    }

    /// <summary>
    /// Loads the Aggregate history from the <see cref="EasyEvs.Contracts.IEventStore"/>
    /// </summary>
    /// <param name="eventStore">The event store</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    public async Task Load(
        IAggregateStore eventStore,
        CancellationToken cancellationToken = default
    )
    {
        await eventStore.Load(this, cancellationToken);
    }

    /// <summary>
    /// Saves the Aggregate history into the <see cref="EasyEvs.Contracts.IEventStore"/>
    /// </summary>
    /// <param name="eventStore">The event store</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <returns>The <see cref="Task"/></returns>
    public async Task Save(
        IAggregateStore eventStore,
        CancellationToken cancellationToken = default
    )
    {
        if (_new)
        {
            await eventStore.Create(this, cancellationToken);
        }

        await eventStore.Save(this, cancellationToken);
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
        _new = _new || !isNew;
        if (isNew)
        {
            _events.Add(@event);
        }
    }
}
