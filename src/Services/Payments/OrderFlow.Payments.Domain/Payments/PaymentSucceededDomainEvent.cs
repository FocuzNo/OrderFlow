using OrderFlow.Payments.Domain.Common;
namespace OrderFlow.Payments.Domain.Payments;
public sealed record PaymentSucceededDomainEvent(Guid EventId, DateTimeOffset OccurredOnUtc, Guid PaymentId, Guid OrderId, decimal Amount) : IDomainEvent;
