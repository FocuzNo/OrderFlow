using OrderFlow.Ordering.Domain.Common;

namespace OrderFlow.Ordering.Domain.Orders;

public sealed record OrderConfirmedDomainEvent(
    Guid EventId,
    DateTimeOffset OccurredOnUtc,
    Guid OrderId,
    Guid CustomerId,
    string CustomerEmail
) : IDomainEvent;
