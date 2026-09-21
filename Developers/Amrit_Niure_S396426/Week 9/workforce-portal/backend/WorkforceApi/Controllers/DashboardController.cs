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

    // GET: api/dashboard/summary
    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummaryDto>> GetSummary()
    {
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

        var hires = await employees
            .GroupBy(e => e.HireDate.Year)
            .Select(g => new HiresByYear(g.Key, g.Count()))
            .OrderBy(h => h.Year)
            .ToListAsync();

        // Next seven days of scheduled hours, one point per day so the chart has no gaps.
        var windowStart = DateTime.UtcNow.Date;
        var windowEnd = windowStart.AddDays(ShiftWindowDays);
        var shifts = await _db.Shifts.AsNoTracking()
            .Where(s => s.Start >= windowStart && s.Start < windowEnd)
            .Select(s => new { s.Start, s.End })
            .ToListAsync();

        var hoursByDay = Enumerable.Range(0, ShiftWindowDays)
            .Select(offset =>
            {
                var day = windowStart.AddDays(offset);
                var hours = shifts
                    .Where(s => s.Start.Date == day)
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
