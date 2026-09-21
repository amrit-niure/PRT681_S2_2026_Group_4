namespace WorkforceApi.Models;

/// <summary>A scheduled block of work for one employee; shown on the roster scheduler.</summary>
public class Shift
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public Employee? Employee { get; set; }

    public string Title { get; set; } = string.Empty;

    /// <summary>Start of the shift (UTC).</summary>
    public DateTime Start { get; set; }

    /// <summary>End of the shift (UTC).</summary>
    public DateTime End { get; set; }

    public bool IsAllDay { get; set; }

    public string? Notes { get; set; }
}
