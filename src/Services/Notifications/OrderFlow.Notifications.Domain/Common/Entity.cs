namespace OrderFlow.Notifications.Domain.Common;

public abstract class Entity
{
    public Guid Id { get; protected init; }

    protected Entity(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Entity ID cannot be empty.", nameof(id));
        Id = id;
    }

    protected Entity() { }
}
