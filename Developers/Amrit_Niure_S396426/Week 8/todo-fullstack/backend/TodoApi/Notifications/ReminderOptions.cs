namespace TodoApi.Notifications;

/// <summary>
/// Settings for the overdue-task reminder job, bound from the "Reminders" section.
/// </summary>
public class ReminderOptions
{
    public const string SectionName = "Reminders";

    /// <summary>Master switch. When false the background job stays idle.</summary>
    public bool Enabled { get; set; }

    /// <summary>Where the overdue reminder emails are sent.</summary>
    public string RecipientEmail { get; set; } = string.Empty;

    /// <summary>How often to scan for overdue tasks. Default 5 minutes.</summary>
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromMinutes(5);

    public bool IsConfigured => Enabled && !string.IsNullOrWhiteSpace(RecipientEmail);
}
