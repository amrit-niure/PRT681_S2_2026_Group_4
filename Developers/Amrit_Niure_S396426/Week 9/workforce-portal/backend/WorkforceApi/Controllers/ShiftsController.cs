using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkforceApi.Data;
using WorkforceApi.Dtos;
using WorkforceApi.Models;

namespace WorkforceApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShiftsController : ControllerBase
{
    private readonly WorkforceDbContext _db;

    public ShiftsController(WorkforceDbContext db)
    {
        _db = db;
    }

    // GET: api/shifts?from=2026-09-21T00:00:00Z&to=2026-09-28T00:00:00Z&employeeId=3
    // Returns every shift that overlaps the [from, to) window, which is what the scheduler asks for.
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ShiftDto>>> GetRange(
        DateTimeOffset? from,
        DateTimeOffset? to,
        int? employeeId)
    {
        var query = _db.Shifts.AsNoTracking().AsQueryable();

        if (from is { } fromValue)
        {
            var fromUtc = fromValue.UtcDateTime;
            query = query.Where(s => s.End > fromUtc);
        }

        if (to is { } toValue)
        {
            var toUtc = toValue.UtcDateTime;
            query = query.Where(s => s.Start < toUtc);
        }

        if (employeeId is not null)
        {
            query = query.Where(s => s.EmployeeId == employeeId);
        }

        var shifts = await query
            .OrderBy(s => s.Start)
            .ThenBy(s => s.Id)
            .Select(s => new ShiftDto
            {
                Id = s.Id,
                EmployeeId = s.EmployeeId,
                EmployeeName = s.Employee!.FirstName + " " + s.Employee.LastName,
                Title = s.Title,
                Start = s.Start,
                End = s.End,
                IsAllDay = s.IsAllDay,
                Notes = s.Notes
            })
            .ToListAsync();

        return Ok(shifts);
    }

    // GET: api/shifts/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ShiftDto>> GetById(int id)
    {
        var shift = await _db.Shifts.AsNoTracking()
            .Include(s => s.Employee)
            .FirstOrDefaultAsync(s => s.Id == id);

        return shift is null ? NotFound() : Ok(ShiftDto.FromEntity(shift, EmployeeName(shift.Employee!)));
    }

    // POST: api/shifts
    [HttpPost]
    public async Task<ActionResult<ShiftDto>> Create(SaveShiftRequest request)
    {
        var problem = await ValidateAgainstDatabase(request, existingId: null);
        if (problem is not null)
        {
            return problem;
        }

        var shift = new Shift();
        Apply(shift, request);

        _db.Shifts.Add(shift);
        await _db.SaveChangesAsync();

        var employee = await _db.Employees.AsNoTracking().FirstAsync(e => e.Id == shift.EmployeeId);

        return CreatedAtAction(nameof(GetById), new { id = shift.Id }, ShiftDto.FromEntity(shift, EmployeeName(employee)));
    }

    // PUT: api/shifts/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, SaveShiftRequest request)
    {
        var shift = await _db.Shifts.FindAsync(id);
        if (shift is null)
        {
            return NotFound();
        }

        var problem = await ValidateAgainstDatabase(request, existingId: id);
        if (problem is not null)
        {
            return problem;
        }

        Apply(shift, request);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/shifts/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var shift = await _db.Shifts.FindAsync(id);
        if (shift is null)
        {
            return NotFound();
        }

        _db.Shifts.Remove(shift);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private static void Apply(Shift shift, SaveShiftRequest request)
    {
        shift.EmployeeId = request.EmployeeId;
        shift.Title = request.Title.Trim();
        shift.Start = request.Start!.Value.UtcDateTime;
        shift.End = request.End!.Value.UtcDateTime;
        shift.IsAllDay = request.IsAllDay;
        shift.Notes = request.Notes?.Trim();
    }

    // Rules that need the database: the employee must exist and an employee can't be double-booked.
    private async Task<ActionResult?> ValidateAgainstDatabase(SaveShiftRequest request, int? existingId)
    {
        if (!await _db.Employees.AnyAsync(e => e.Id == request.EmployeeId))
        {
            ModelState.AddModelError(nameof(SaveShiftRequest.EmployeeId), "The selected employee does not exist.");
        }
        else
        {
            var start = request.Start!.Value.UtcDateTime;
            var end = request.End!.Value.UtcDateTime;

            var overlaps = await _db.Shifts.AnyAsync(s =>
                s.EmployeeId == request.EmployeeId &&
                s.Id != existingId &&
                s.Start < end &&
                s.End > start);

            if (overlaps)
            {
                ModelState.AddModelError(nameof(SaveShiftRequest.Start), "This employee already has a shift that overlaps this time.");
            }
        }

        return ModelState.IsValid ? null : ValidationProblem(ModelState);
    }

    private static string EmployeeName(Employee employee) => $"{employee.FirstName} {employee.LastName}";
}
