namespace TodoApi.Models;

/// <summary>
/// A single task in the to-do list. This is the EF Core entity that maps to the TodoItems table.
/// </summary>
public class TodoItem
{
    public int Id { get; set; }

    /// <summary>Id of the Identity user that owns this task (AspNetUsers.Id).</summary>
    public string UserId { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public bool IsComplete { get; set; }

    /// <summary>Optional date/time the task is due. Null means no deadline.</summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// How long before <see cref="DueDate"/> to send the reminder email, e.g. 30 for
    /// "30 minutes before". Null means no reminder for this task, even with a due date.
    /// Meaningless when <see cref="DueDate"/> is null.
    /// </summary>
    public int? ReminderMinutesBefore { get; set; }

    /// <summary>
    /// True once the reminder email has been sent for this task. Reset when the due
    /// date, reminder lead time, or completion state changes, so a fresh reminder can go out.
    /// </summary>
    public bool ReminderSent { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
