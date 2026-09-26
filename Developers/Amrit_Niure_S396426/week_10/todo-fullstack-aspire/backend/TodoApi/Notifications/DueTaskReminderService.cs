using Microsoft.Extensions.Options;

namespace TodoApi.Notifications;

/// <summary>
/// Background job that periodically runs <see cref="OverdueReminderScanner"/> to email
/// reminders for tasks that are due and still not done.
/// </summary>
public class DueTaskReminderService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ReminderOptions _options;
    private readonly ILogger<DueTaskReminderService> _logger;

    public DueTaskReminderService(
        IServiceScopeFactory scopeFactory,
        IOptions<ReminderOptions> options,
        ILogger<DueTaskReminderService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Due-task reminder job is disabled (Reminders:Enabled = false).");
            return;
        }

        _logger.LogInformation(
            "Due-task reminder job started; scanning every {Interval}.", _options.PollInterval);

        // Give the app a moment to finish starting before the first scan.
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        using var timer = new PeriodicTimer(_options.PollInterval);
        do
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var scanner = scope.ServiceProvider.GetRequiredService<OverdueReminderScanner>();
                await scanner.RunAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Reminder scan failed; will retry next interval.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
