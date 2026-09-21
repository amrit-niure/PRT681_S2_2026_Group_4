using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkforceApi.Data;
using WorkforceApi.Dtos;
using WorkforceApi.Models;

namespace WorkforceApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DepartmentsController : ControllerBase
{
    private readonly WorkforceDbContext _db;

    public DepartmentsController(WorkforceDbContext db)
    {
        _db = db;
    }

    // GET: api/departments
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DepartmentDto>>> GetAll()
    {
        var departments = await _db.Departments
            .OrderBy(d => d.Name)
            .Select(d => new DepartmentDto
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Description,
                EmployeeCount = d.Employees.Count
            })
            .ToListAsync();

        return Ok(departments);
    }

    // GET: api/departments/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<DepartmentDto>> GetById(int id)
    {
        var department = await _db.Departments
            .Where(d => d.Id == id)
            .Select(d => new DepartmentDto
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Description,
                EmployeeCount = d.Employees.Count
            })
            .FirstOrDefaultAsync();

        return department is null ? NotFound() : Ok(department);
    }

    // POST: api/departments
    [HttpPost]
    public async Task<ActionResult<DepartmentDto>> Create(SaveDepartmentRequest request)
    {
        var name = request.Name.Trim();
        if (await _db.Departments.AnyAsync(d => d.Name == name))
        {
            return NameTaken();
        }

        var department = new Department { Name = name, Description = request.Description?.Trim() };
        _db.Departments.Add(department);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = department.Id }, DepartmentDto.FromEntity(department, 0));
    }

    // PUT: api/departments/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, SaveDepartmentRequest request)
    {
        var department = await _db.Departments.FindAsync(id);
        if (department is null)
        {
            return NotFound();
        }

        var name = request.Name.Trim();
        if (await _db.Departments.AnyAsync(d => d.Name == name && d.Id != id))
        {
            return NameTaken();
        }

        department.Name = name;
        department.Description = request.Description?.Trim();
        await _db.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/departments/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var department = await _db.Departments.FindAsync(id);
        if (department is null)
        {
            return NotFound();
        }

        if (await _db.Employees.AnyAsync(e => e.DepartmentId == id))
        {
            return Conflict(new ProblemDetails
            {
                Title = "Department is in use",
                Detail = "Move or remove the employees in this department before deleting it.",
                Status = StatusCodes.Status409Conflict
            });
        }

        _db.Departments.Remove(department);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    // Reported as a field-level validation error so the client form can show it under "Name".
    private ActionResult NameTaken()
    {
        ModelState.AddModelError(nameof(SaveDepartmentRequest.Name), "A department with this name already exists.");
        return ValidationProblem(ModelState);
    }
}
