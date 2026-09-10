using System.Text;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;

namespace TodoApi.Notifications;

/// <summary>
/// Finds tasks that are due (today or earlier) and still not done, then emails each
/// owner a reminder digest at their account email address and marks those tasks so
/// they are not reported again. Used by the background job and the manual trigger.
/// </summary>
public class OverdueReminderScanner
{
    private readonly TodoDbContext _db;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<OverdueReminderScanner> _logger;

    public OverdueReminderScanner(
        TodoDbContext db,
        IEmailSender emailSender,
        ILogger<OverdueReminderScanner> logger)
    {
        _db = db;
        _emailSender = emailSender;
        _logger = logger;
    }

    /// <summary>Scans every user's tasks. Returns the total number of tasks reminded.</summary>
    public async Task<int> RunAsync(CancellationToken cancellationToken = default)
    {
        var dueTasks = await DueQuery().ToListAsync(cancellationToken);
        if (dueTasks.Count == 0)
        {
            return 0;
        }

        var groups = dueTasks.GroupBy(t => t.UserId).ToList();
        var userIds = groups.Select(g => g.Key).ToList();
        var emailsById = await _db.Users
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new { u.Id, u.Email })
            .ToDictionaryAsync(u => u.Id, u => u.Email, cancellationToken);

        var total = 0;
        foreach (var group in groups)
        {
            emailsById.TryGetValue(group.Key, out var email);
            total += await SendDigestAsync(group.Key, email, group.ToList(), cancellationToken);
        }

        return total;
    }

    /// <summary>Scans one user's tasks. Returns the number of tasks reminded.</summary>
    public async Task<int> RunForUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var dueTasks = await DueQuery()
            .Where(t => t.UserId == userId)
            .ToListAsync(cancellationToken);
        if (dueTasks.Count == 0)
        {
            return 0;
        }

        var email = await _db.Users
            .Where(u => u.Id == userId)
            .Select(u => u.Email)
            .FirstOrDefaultAsync(cancellationToken);

        return await SendDigestAsync(userId, email, dueTasks, cancellationToken);
    }

    /// <summary>Incomplete tasks whose due day is today or earlier and not yet reminded.</summary>
    private IQueryable<TodoItem> DueQuery()
    {
        var cutoff = DateTime.UtcNow.Date.AddDays(1);
        return _db.TodoItems
            .Where(t => !t.IsComplete && t.DueDate != null && t.DueDate < cutoff && !t.ReminderSent)
            .OrderBy(t => t.DueDate);
    }

    private async Task<int> SendDigestAsync(
        string userId, string? email, List<TodoItem> tasks, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            _logger.LogWarning("User {UserId} has {Count} overdue task(s) but no email address; skipping.",
                userId, tasks.Count);
            return 0;
        }

        var body = new StringBuilder();
        body.AppendLine("The following task(s) are due and not done yet:");
        body.AppendLine();
        foreach (var task in tasks)
        {
            body.AppendLine($"  - {task.Title} (due {task.DueDate!.Value:d})");
        }
        body.AppendLine();
        body.AppendLine("— Todo App");

        var subject = tasks.Count == 1
            ? $"Reminder: \"{tasks[0].Title}\" is due"
            : $"Reminder: {tasks.Count} tasks are due";

        await _emailSender.SendAsync(email, subject, body.ToString(), cancellationToken);

        foreach (var task in tasks)
        {
            task.ReminderSent = true;
        }
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Sent overdue reminder to {Email} for {Count} task(s).", email, tasks.Count);
        return tasks.Count;
    }
}
