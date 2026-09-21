using WorkforceApi.Models;

namespace WorkforceApi.Dtos;

public class ShiftDto
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public string EmployeeName { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    /// <summary>Start of the shift in UTC (serialized with a trailing Z).</summary>
    public DateTime Start { get; set; }

    /// <summary>End of the shift in UTC (serialized with a trailing Z).</summary>
    public DateTime End { get; set; }

    public bool IsAllDay { get; set; }

    public string? Notes { get; set; }

    public static ShiftDto FromEntity(Shift shift, string employeeName) => new()
    {
        Id = shift.Id,
        EmployeeId = shift.EmployeeId,
        EmployeeName = employeeName,
        Title = shift.Title,
        Start = shift.Start,
        End = shift.End,
        IsAllDay = shift.IsAllDay,
        Notes = shift.Notes
    };
}
