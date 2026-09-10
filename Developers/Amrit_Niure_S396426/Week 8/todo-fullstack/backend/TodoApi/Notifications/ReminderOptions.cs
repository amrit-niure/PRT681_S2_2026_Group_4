namespace TodoApi.Notifications;

/// <summary>
/// Settings for the overdue-task reminder job, bound from the "Reminders" section.
/// Each user is emailed their own overdue tasks at their account email address.
/// </summary>
public class ReminderOptions
{
    public const string SectionName = "Reminders";

    /// <summary>Master switch. When false the background job stays idle.</summary>
    public bool Enabled { get; set; }

    /// <summary>How often to scan for overdue tasks. Default 5 minutes.</summary>
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromMinutes(5);
}
