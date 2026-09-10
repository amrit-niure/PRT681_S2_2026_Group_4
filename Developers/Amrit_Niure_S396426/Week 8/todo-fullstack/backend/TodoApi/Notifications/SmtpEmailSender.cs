using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace TodoApi.Notifications;

/// <summary>Sends email through an SMTP server using MailKit.</summary>
public class SmtpEmailSender : IEmailSender
{
    private readonly SmtpOptions _options;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<SmtpOptions> options, ILogger<SmtpEmailSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        if (!_options.IsConfigured)
        {
            _logger.LogWarning("SMTP is not configured (Smtp:Host is empty); skipping email to {To}.", to);
            return;
        }

        var from = string.IsNullOrWhiteSpace(_options.FromAddress) ? _options.User : _options.FromAddress;

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_options.FromName, from));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new TextPart("plain") { Text = body };

        using var client = new SmtpClient();

        await client.ConnectAsync(_options.Host, _options.Port, _options.SocketOptions, cancellationToken);

        if (!string.IsNullOrWhiteSpace(_options.User))
        {
            await client.AuthenticateAsync(_options.User, _options.Password, cancellationToken);
        }

        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(quit: true, cancellationToken);

        _logger.LogInformation("Sent email to {To} with subject \"{Subject}\".", to, subject);
    }
}
