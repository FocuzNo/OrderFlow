namespace OrderFlow.IntegrationContracts;

public sealed record IntegrationEventEnvelope(
    Guid EventId,
    string EventType,
    int EventVersion,
    DateTimeOffset OccurredOnUtc,
    string CorrelationId,
    string? CausationId,
    string AggregateId,
    string Payload
);
