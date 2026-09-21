using OrderFlow.Ordering.Domain.Common;
namespace OrderFlow.Ordering.Domain.Orders;
public sealed record PaymentRequestedDomainEvent(Guid EventId, DateTimeOffset OccurredOnUtc, Guid OrderId, decimal Amount) : IDomainEvent;
