using Temporalio.Client;

namespace TicketingApi.Workflows;

/// <summary>
/// Starts <see cref="TicketEmailWorkflow"/> runs. Email is a side effect of the request, not
/// part of it, so a Temporal outage is logged and swallowed rather than failing the API call.
/// </summary>
public class TicketEmailDispatcher
{
    private readonly ITemporalClient _client;
    private readonly TemporalOptions _options;
    private readonly ILogger<TicketEmailDispatcher> _logger;

    public TicketEmailDispatcher(
        ITemporalClient client, Microsoft.Extensions.Options.IOptions<TemporalOptions> options,
        ILogger<TicketEmailDispatcher> logger)
    {
        _client = client;
        _options = options.Value;
        _logger = logger;
    }

    public async Task DispatchAsync(TicketEmailRequest request)
    {
        try
        {
            var workflowId = $"ticket-email-{request.TicketId}-{Guid.NewGuid():N}";
            await _client.StartWorkflowAsync(
                (TicketEmailWorkflow wf) => wf.RunAsync(request),
                new WorkflowOptions(workflowId, _options.TaskQueue));

            _logger.LogInformation("Started workflow {WorkflowId} for ticket {TicketId}.", workflowId, request.TicketId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not start the email workflow for ticket {TicketId}.", request.TicketId);
        }
    }
}
