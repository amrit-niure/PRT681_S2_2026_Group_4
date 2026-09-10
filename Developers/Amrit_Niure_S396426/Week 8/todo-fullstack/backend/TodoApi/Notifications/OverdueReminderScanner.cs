using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TodoApi.Data;

namespace TodoApi.Notifications;

/// <summary>
/// Finds tasks that are due (today or earlier) and still not done, emails a reminder
/// digest to the configured recipient, and marks those tasks so they are not reported
/// again. Used by the background job and by the manual trigger endpoint.
/// </summary>
public class OverdueReminderScanner
{
    private readonly TodoDbContext _db;
    private readonly IEmailSender _emailSender;
    private readonly ReminderOptions _options;
    private readonly ILogger<OverdueReminderScanner> _logger;

    public OverdueReminderScanner(
        TodoDbContext db,
        IEmailSender emailSender,
        IOptions<ReminderOptions> options,
        ILogger<OverdueReminderScanner> logger)
    {
        _db = db;
        _emailSender = emailSender;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>Runs a single scan. Returns the number of tasks a reminder was sent for.</summary>
    public async Task<int> RunAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.RecipientEmail))
        {
            _logger.LogWarning("Reminders:RecipientEmail is empty; skipping scan.");
            return 0;
        }

        // "Due" means the due day is today or in the past.
        var cutoff = DateTime.UtcNow.Date.AddDays(1);

        var dueTasks = await _db.TodoItems
            .Where(t => !t.IsComplete && t.DueDate != null && t.DueDate < cutoff && !t.ReminderSent)
            .OrderBy(t => t.DueDate)
            .ToListAsync(cancellationToken);

        if (dueTasks.Count == 0)
        {
            return 0;
        }

        var body = new StringBuilder();
        body.AppendLine("The following task(s) are due and not done yet:");
        body.AppendLine();
        foreach (var task in dueTasks)
        {
            body.AppendLine($"  - {task.Title} (due {task.DueDate!.Value:d})");
        }
        body.AppendLine();
        body.AppendLine("— Todo App");

        var subject = dueTasks.Count == 1
            ? $"Reminder: \"{dueTasks[0].Title}\" is due"
            : $"Reminder: {dueTasks.Count} tasks are due";

        await _emailSender.SendAsync(_options.RecipientEmail, subject, body.ToString(), cancellationToken);

        foreach (var task in dueTasks)
        {
            task.ReminderSent = true;
        }
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Sent overdue reminder for {Count} task(s).", dueTasks.Count);
        return dueTasks.Count;
    }
}
