using Temporalio.Common;
using Temporalio.Workflows;

namespace TicketingApi.Workflows;

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
