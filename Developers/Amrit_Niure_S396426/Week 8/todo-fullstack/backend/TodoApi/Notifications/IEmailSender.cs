namespace TodoApi.Notifications;

/// <summary>Sends a plain-text email. Keeps the transport (SMTP) out of the callers.</summary>
public interface IEmailSender
{
    Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
}
