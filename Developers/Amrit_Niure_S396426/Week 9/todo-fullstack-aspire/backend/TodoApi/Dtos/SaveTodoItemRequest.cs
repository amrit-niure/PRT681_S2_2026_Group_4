using System.ComponentModel.DataAnnotations;

namespace TodoApi.Dtos;

/// <summary>
/// Payload for creating or updating a to-do item. Validated automatically by [ApiController].
/// </summary>
public class SaveTodoItemRequest
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    public bool IsComplete { get; set; }

    /// <summary>Optional due date/time. Omit or send null for no deadline.</summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// Minutes before <see cref="DueDate"/> to send a reminder (0 = at the due time).
    /// Null means no reminder for this task. Ignored when <see cref="DueDate"/> is null.
    /// </summary>
    [Range(0, 43_200)] // up to 30 days
    public int? ReminderMinutesBefore { get; set; }
}
