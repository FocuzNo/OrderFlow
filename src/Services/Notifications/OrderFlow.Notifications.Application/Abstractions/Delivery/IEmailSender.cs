namespace OrderFlow.Notifications.Application.Abstractions.Delivery;

public interface IEmailSender
{
    Task SendAsync(
        string recipient,
        string subject,
        string body,
        CancellationToken cancellationToken
    );
}
