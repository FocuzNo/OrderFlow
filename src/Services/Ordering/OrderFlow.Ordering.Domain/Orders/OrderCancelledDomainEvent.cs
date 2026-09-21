using OrderFlow.Ordering.Domain.Common;

namespace OrderFlow.Ordering.Domain.Orders;

public sealed record OrderCancelledDomainEvent(
    Guid EventId,
    DateTimeOffset OccurredOnUtc,
    Guid OrderId,
    string CustomerEmail,
    string Reason
) : IDomainEvent;
