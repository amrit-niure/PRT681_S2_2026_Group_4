using Temporalio.Activities;
using TicketingApi.Notifications;

namespace TicketingApi.Workflows;

public class TicketEmailActivities
{
    private readonly IEmailSender _emailSender;
    private readonly ILogger<TicketEmailActivities> _logger;

    public TicketEmailActivities(IEmailSender emailSender, ILogger<TicketEmailActivities> logger)
    {
        _emailSender = emailSender;
        _logger = logger;
    }

    [Activity]
    public async Task SendTicketEmailAsync(TicketEmailRequest request)
    {
        _logger.LogInformation(
            "Dispatching email for ticket {TicketId} to {To} (attempt {Attempt}).",
            request.TicketId, request.To, ActivityExecutionContext.Current.Info.Attempt);

        await _emailSender.SendAsync(
            request.To, request.Subject, request.Body, ActivityExecutionContext.Current.CancellationToken);
    }
}
