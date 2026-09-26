using MailKit.Security;

namespace TicketingApi.Notifications;

/// <summary>
/// SMTP server settings, bound from the "Smtp" configuration section. Defaults target the
/// Resend SMTP relay: user "resend", password = your Resend API key, and a From address on
/// a domain verified in Resend. Supply secrets as environment variables, e.g. Smtp__Password.
/// </summary>
public class SmtpOptions
{
    public const string SectionName = "Smtp";

    /// <summary>SMTP host. Sending is skipped (and logged) until both this and <see cref="Password"/> are set.</summary>
    public string Host { get; set; } = string.Empty;

    public int Port { get; set; } = 465;

    /// <summary>For Resend this is the literal string "resend".</summary>
    public string User { get; set; } = "resend";

    /// <summary>For Resend this is the API key (re_...).</summary>
    public string Password { get; set; } = string.Empty;

    public string FromAddress { get; set; } = string.Empty;

    public string FromName { get; set; } = "Ticketing";

    /// <summary>
    /// Connection security: <c>SslOnConnect</c> (port 465, default), <c>StartTls</c> (port 587),
    /// <c>None</c> (plaintext, local testing only) or <c>Auto</c>.
    /// </summary>
    public string Security { get; set; } = nameof(SecureSocketOptions.SslOnConnect);

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Host) && !string.IsNullOrWhiteSpace(Password);

    public SecureSocketOptions SocketOptions =>
        Enum.TryParse<SecureSocketOptions>(Security, ignoreCase: true, out var value)
            ? value
            : SecureSocketOptions.SslOnConnect;
}
