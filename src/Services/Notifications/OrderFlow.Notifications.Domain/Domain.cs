namespace OrderFlow.Notifications.Domain;

public sealed record DomainEvent(Guid Id, string Type, Guid AggregateId, DateTimeOffset OccurredAt);
public interface IHasDomainEvents
{
    IReadOnlyCollection<DomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}

public sealed class Notification : IHasDomainEvents
{
    public const int MaxReferenceLength = 200;
    public const int MaxDescriptionLength = 2000;
    private readonly List<DomainEvent> _domainEvents = [];
    private Notification() { }

    public Guid Id { get; private set; }
    public string Reference { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public decimal Value { get; private set; }
    public string Status { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public static Notification Create(string reference, string? description, decimal value)
    {
        Validate(reference, description, value);
        var now = DateTimeOffset.UtcNow;
        var entity = new Notification { Id = Guid.NewGuid(), Reference = reference.Trim(), Description = description?.Trim(), Value = value, Status = "Pending", CreatedAt = now, UpdatedAt = now };
        entity.Raise("notifications.notification-created");
        return entity;
    }

    public void Update(string reference, string? description, decimal value, string status)
    {
        Validate(reference, description, value);
        ArgumentException.ThrowIfNullOrWhiteSpace(status);
        Reference = reference.Trim(); Description = description?.Trim(); Value = value; Status = status.Trim(); UpdatedAt = DateTimeOffset.UtcNow;
        Raise("notifications.notification-updated");
    }

    public void MarkDeleted() => Raise("notifications.notification-deleted");
    public void ClearDomainEvents() => _domainEvents.Clear();
    private void Raise(string type) => _domainEvents.Add(new DomainEvent(Guid.NewGuid(), type, Id, DateTimeOffset.UtcNow));
    private static void Validate(string reference, string? description, decimal value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reference);
        if (reference.Length > MaxReferenceLength) throw new ArgumentException($"Reference cannot exceed {MaxReferenceLength} characters.", nameof(reference));
        if (description?.Length > MaxDescriptionLength) throw new ArgumentException($"Description cannot exceed {MaxDescriptionLength} characters.", nameof(description));
        if (value < 0) throw new ArgumentOutOfRangeException(nameof(value), "Value cannot be negative.");
    }
}
