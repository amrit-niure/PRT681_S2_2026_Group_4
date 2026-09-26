namespace TodoApi.Notifications;

/// <summary>
/// Settings for the task reminder job, bound from the "Reminders" section. Each user
/// is emailed their own due tasks (per each task's own reminder lead time) at their
/// account email address.
/// </summary>
public class ReminderOptions
{
    public const string SectionName = "Reminders";

    /// <summary>Master switch. When false the background job stays idle.</summary>
    public bool Enabled { get; set; }

    /// <summary>How often to scan for due reminders. Default 5 minutes.</summary>
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromMinutes(5);
}
