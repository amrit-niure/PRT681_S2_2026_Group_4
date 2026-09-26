using Temporalio.Common;
using Temporalio.Workflows;

namespace TicketingApi.Workflows;

/// <summary>
/// Durable, asynchronous email dispatch for ticket confirmations and status alerts. The API
/// only starts the workflow and returns; Temporal owns delivery and retries with backoff,
/// and the run history survives an API restart.
/// </summary>
[Workflow]
public class TicketEmailWorkflow
{
    [WorkflowRun]
    public async Task RunAsync(TicketEmailRequest request)
    {
        await Workflow.ExecuteActivityAsync(
            (TicketEmailActivities activities) => activities.SendTicketEmailAsync(request),
            new ActivityOptions
            {
                StartToCloseTimeout = TimeSpan.FromSeconds(30),
                RetryPolicy = new RetryPolicy
                {
                    InitialInterval = TimeSpan.FromSeconds(2),
                    BackoffCoefficient = 2,
                    MaximumInterval = TimeSpan.FromMinutes(1),
                    MaximumAttempts = 5
                }
            });
    }
}
