using OrderFlow.Payments.Domain.Common;
namespace OrderFlow.Payments.Domain.Payments;
public sealed record PaymentFailedDomainEvent(Guid EventId, DateTimeOffset OccurredOnUtc, Guid PaymentId, Guid OrderId, string Reason) : IDomainEvent;
