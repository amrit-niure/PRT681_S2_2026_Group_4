using MailKit.Security;

namespace TodoApi.Notifications;

/// <summary>
/// SMTP server settings, bound from the "Smtp" configuration section. In production
/// supply these as environment variables / app settings, e.g. Smtp__Host, Smtp__Password.
/// </summary>
public class SmtpOptions
{
    public const string SectionName = "Smtp";

    public string Host { get; set; } = string.Empty;

    public int Port { get; set; } = 587;

    public string User { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    /// <summary>Address the reminder emails are sent from. Falls back to <see cref="User"/> when empty.</summary>
    public string FromAddress { get; set; } = string.Empty;

    public string FromName { get; set; } = "Todo App";

    /// <summary>
    /// Connection security: <c>StartTls</c> (port 587, default), <c>SslOnConnect</c> (port 465),
    /// <c>None</c> (plaintext, local testing only) or <c>Auto</c>.
    /// </summary>
    public string Security { get; set; } = nameof(SecureSocketOptions.StartTls);

    public bool IsConfigured => !string.IsNullOrWhiteSpace(Host);

    public SecureSocketOptions SocketOptions =>
        Enum.TryParse<SecureSocketOptions>(Security, ignoreCase: true, out var value)
            ? value
            : SecureSocketOptions.StartTls;
}
