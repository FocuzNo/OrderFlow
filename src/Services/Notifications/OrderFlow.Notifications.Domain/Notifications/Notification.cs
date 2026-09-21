using System.Net.Mail;
using OrderFlow.Notifications.Domain.Common;

namespace OrderFlow.Notifications.Domain.Notifications;

public sealed class Notification : AggregateRoot
{
    private readonly List<NotificationDeliveryAttempt> _attempts = [];

    private Notification() { }

    private Notification(
        Guid id,
        string recipient,
        string subject,
        string body,
        NotificationChannel channel
    )
        : base(id)
    {
        Recipient = recipient;
        Subject = subject;
        Body = body;
        Channel = channel;
        Status = NotificationStatus.Pending;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public string Recipient { get; private set; } = string.Empty;
    public string Subject { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    public NotificationChannel Channel { get; private set; } = NotificationChannel.Email;
    public NotificationStatus Status { get; private set; } = NotificationStatus.Pending;
    public IReadOnlyCollection<NotificationDeliveryAttempt> Attempts => _attempts.AsReadOnly();
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? SentAt { get; private set; }

    public static Notification Create(
        string recipient,
        string subject,
        string body,
        NotificationChannel channel
    )
    {
        Validate(recipient, subject, body);
        return new Notification(
            Guid.NewGuid(),
            new MailAddress(recipient.Trim()).Address,
            subject.Trim(),
            body.Trim(),
            channel
        );
    }

    public void StartSending()
    {
        if (Status != NotificationStatus.Pending && Status != NotificationStatus.Failed)
            throw new DomainException("Notification cannot be sent in its current state.");
        Status = NotificationStatus.Sending;
    }

    public void RecordSuccess()
    {
        if (Status != NotificationStatus.Sending)
            throw new DomainException("Notification is not being sent.");
        _attempts.Add(NotificationDeliveryAttempt.Create(true, null));
        Status = NotificationStatus.Sent;
        SentAt = DateTimeOffset.UtcNow;
        Raise(new NotificationSentDomainEvent(Guid.NewGuid(), SentAt.Value, Id, Recipient));
    }

    public void RecordFailure(string error)
    {
        if (Status != NotificationStatus.Sending)
            throw new DomainException("Notification is not being sent.");
        if (string.IsNullOrWhiteSpace(error))
            throw new DomainException("Delivery error is required.");
        _attempts.Add(NotificationDeliveryAttempt.Create(false, error.Trim()));
        Status = NotificationStatus.Failed;
    }

    private static void Validate(string recipient, string subject, string body)
    {
        try
        {
            _ = new MailAddress(recipient);
        }
        catch (FormatException)
        {
            throw new DomainException("Recipient email is invalid.");
        }
        if (string.IsNullOrWhiteSpace(subject) || subject.Length > 250)
            throw new DomainException("Subject must contain 1-250 characters.");
        if (string.IsNullOrWhiteSpace(body) || body.Length > 10000)
            throw new DomainException("Body must contain 1-10000 characters.");
    }
}
