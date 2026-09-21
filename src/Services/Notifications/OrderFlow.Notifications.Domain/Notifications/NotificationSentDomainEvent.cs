using OrderFlow.Notifications.Domain.Common;
namespace OrderFlow.Notifications.Domain.Notifications;
public sealed record NotificationSentDomainEvent(Guid EventId, DateTimeOffset OccurredOnUtc, Guid NotificationId, string Recipient) : IDomainEvent;
