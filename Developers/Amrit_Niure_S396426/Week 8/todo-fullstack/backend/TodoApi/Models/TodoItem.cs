namespace TodoApi.Models;

/// <summary>
/// A single task in the to-do list. This is the EF Core entity that maps to the TodoItems table.
/// </summary>
public class TodoItem
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public bool IsComplete { get; set; }

    /// <summary>Optional date the task is due. Null means no deadline.</summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// True once an overdue reminder email has been sent for this task. Reset when the
    /// due date moves or the task is reopened, so a fresh reminder can go out.
    /// </summary>
    public bool ReminderSent { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
