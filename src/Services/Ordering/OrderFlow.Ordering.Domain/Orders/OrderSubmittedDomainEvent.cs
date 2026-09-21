using OrderFlow.Ordering.Domain.Common;

namespace OrderFlow.Ordering.Domain.Orders;

public sealed record OrderSubmittedDomainEvent(
    Guid EventId,
    DateTimeOffset OccurredOnUtc,
    Guid OrderId,
    Guid CustomerId,
    string CustomerEmail,
    IReadOnlyList<OrderItemSnapshot> Items,
    decimal TotalAmount
) : IDomainEvent;
