using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkforceApi.Data;
using WorkforceApi.Dtos;

namespace WorkforceApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private const int ShiftWindowDays = 7;

    private readonly WorkforceDbContext _db;

    public DashboardController(WorkforceDbContext db)
    {
        _db = db;
    }

    // GET: api/dashboard/summary?utcOffsetMinutes=570
    // The offset (minutes ahead of UTC, e.g. 570 for Darwin) decides which local calendar day a shift
    // counts towards, so a Monday-morning shift isn't charted under Sunday.
    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummaryDto>> GetSummary([FromQuery] int utcOffsetMinutes = 0)
    {
        var offset = TimeSpan.FromMinutes(Math.Clamp(utcOffsetMinutes, -14 * 60, 14 * 60));

        var employees = _db.Employees.AsNoTracking();

        var totalEmployees = await employees.CountAsync();
        var activeEmployees = await employees.CountAsync(e => e.IsActive);
        var departmentCount = await _db.Departments.CountAsync();
        var averageSalary = totalEmployees == 0
            ? 0
            : await employees.Where(e => e.IsActive).AverageAsync(e => (decimal?)e.Salary) ?? 0;

        var headcount = await _db.Departments.AsNoTracking()
            .OrderBy(d => d.Name)
            .Select(d => new HeadcountByDepartment(d.Name, d.Employees.Count(e => e.IsActive)))
            .ToListAsync();

        var hireCounts = await employees
            .GroupBy(e => e.HireDate.Year)
            .OrderBy(g => g.Key)
            .Select(g => new { Year = g.Key, Count = g.Count() })
            .ToListAsync();
        var hires = hireCounts.Select(h => new HiresByYear(h.Year, h.Count)).ToList();

        // Next seven days of scheduled hours, one point per day so the chart has no gaps.
        var firstLocalDay = (DateTime.UtcNow + offset).Date;
        var windowStart = DateTime.SpecifyKind(firstLocalDay - offset, DateTimeKind.Utc);
        var windowEnd = windowStart.AddDays(ShiftWindowDays);
        var shifts = await _db.Shifts.AsNoTracking()
            .Where(s => s.Start >= windowStart && s.Start < windowEnd)
            .Select(s => new { s.Start, s.End })
            .ToListAsync();

        var hoursByDay = Enumerable.Range(0, ShiftWindowDays)
            .Select(dayIndex =>
            {
                var day = firstLocalDay.AddDays(dayIndex);
                var hours = shifts
                    .Where(s => (s.Start + offset).Date == day)
                    .Sum(s => (s.End - s.Start).TotalHours);
                return new ShiftHoursByDay(DateOnly.FromDateTime(day), Math.Round(hours, 1));
            })
            .ToList();

        return Ok(new DashboardSummaryDto
        {
            TotalEmployees = totalEmployees,
            ActiveEmployees = activeEmployees,
            DepartmentCount = departmentCount,
            UpcomingShifts = shifts.Count,
            AverageSalary = Math.Round(averageSalary, 2),
            HeadcountByDepartment = headcount,
            HiresByYear = hires,
            ShiftHoursByDay = hoursByDay
        });
    }
}
