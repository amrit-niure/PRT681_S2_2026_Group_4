namespace WorkforceApi.Dtos;

/// <summary>Headline numbers and chart series for the dashboard page.</summary>
public class DashboardSummaryDto
{
    public int TotalEmployees { get; set; }

    public int ActiveEmployees { get; set; }

    public int DepartmentCount { get; set; }

    /// <summary>Shifts starting in the next seven days.</summary>
    public int UpcomingShifts { get; set; }

    public decimal AverageSalary { get; set; }

    public List<HeadcountByDepartment> HeadcountByDepartment { get; set; } = [];

    public List<HiresByYear> HiresByYear { get; set; } = [];

    public List<ShiftHoursByDay> ShiftHoursByDay { get; set; } = [];
}

public record HeadcountByDepartment(string Department, int Count);

public record HiresByYear(int Year, int Count);

/// <summary>Total scheduled hours for one calendar day (UTC).</summary>
public record ShiftHoursByDay(DateOnly Date, double Hours);
