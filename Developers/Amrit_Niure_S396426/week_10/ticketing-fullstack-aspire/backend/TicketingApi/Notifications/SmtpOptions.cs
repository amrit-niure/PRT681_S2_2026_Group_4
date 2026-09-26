using MailKit.Security;

namespace TicketingApi.Notifications;

public class SmtpOptions
{
    public const string SectionName = "Smtp";

    public string Host { get; set; } = string.Empty;

    public int Port { get; set; } = 465;

    public string User { get; set; } = "resend";

    public string Password { get; set; } = string.Empty;

    public string FromAddress { get; set; } = string.Empty;

    public string FromName { get; set; } = "Ticketing";

    public string Security { get; set; } = nameof(SecureSocketOptions.SslOnConnect);

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Host) && !string.IsNullOrWhiteSpace(Password);

    public SecureSocketOptions SocketOptions =>
        Enum.TryParse<SecureSocketOptions>(Security, ignoreCase: true, out var value)
            ? value
            : SecureSocketOptions.SslOnConnect;
}
