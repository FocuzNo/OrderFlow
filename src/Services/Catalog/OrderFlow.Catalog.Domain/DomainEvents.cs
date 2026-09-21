namespace OrderFlow.Catalog.Domain;

public sealed record DomainEvent(Guid Id, string Type, Guid AggregateId, DateTimeOffset OccurredAt);

public interface IHasDomainEvents
{
    IReadOnlyCollection<DomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}
