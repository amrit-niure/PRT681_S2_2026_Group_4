using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkforceApi.Data;
using WorkforceApi.Dtos;
using WorkforceApi.Models;

namespace WorkforceApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private const int MaxPageSize = 200;

    private readonly WorkforceDbContext _db;

    public EmployeesController(WorkforceDbContext db)
    {
        _db = db;
    }

    // GET: api/employees?search=&departmentId=&isActive=&sortBy=&sortDir=&skip=&take=
    // Filtering, sorting and paging all happen in the database so the grid can page through large sets.
    [HttpGet]
    public async Task<ActionResult<PagedResult<EmployeeDto>>> GetPage(
        string? search,
        int? departmentId,
        bool? isActive,
        string? sortBy,
        string? sortDir,
        int skip = 0,
        int take = 20)
    {
        skip = Math.Max(skip, 0);
        take = Math.Clamp(take, 1, MaxPageSize);

        var query = _db.Employees.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{EscapeLike(search.Trim())}%";
            query = query.Where(e =>
                EF.Functions.ILike(e.FirstName, pattern) ||
                EF.Functions.ILike(e.LastName, pattern) ||
                EF.Functions.ILike(e.Email, pattern) ||
                EF.Functions.ILike(e.JobTitle, pattern));
        }

        if (departmentId is not null)
        {
            query = query.Where(e => e.DepartmentId == departmentId);
        }

        if (isActive is not null)
        {
            query = query.Where(e => e.IsActive == isActive);
        }

        var total = await query.CountAsync();

        var descending = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
        var items = await ApplySort(query, sortBy, descending)
            .Skip(skip)
            .Take(take)
            .Select(e => new EmployeeDto
            {
                Id = e.Id,
                FirstName = e.FirstName,
                LastName = e.LastName,
                Email = e.Email,
                JobTitle = e.JobTitle,
                DepartmentId = e.DepartmentId,
                DepartmentName = e.Department!.Name,
                HireDate = e.HireDate,
                Salary = e.Salary,
                IsActive = e.IsActive
            })
            .ToListAsync();

        return Ok(new PagedResult<EmployeeDto> { Items = items, Total = total });
    }

    // GET: api/employees/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<EmployeeDto>> GetById(int id)
    {
        var employee = await _db.Employees.AsNoTracking()
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.Id == id);

        return employee is null ? NotFound() : Ok(EmployeeDto.FromEntity(employee, employee.Department!.Name));
    }

    // POST: api/employees
    [HttpPost]
    public async Task<ActionResult<EmployeeDto>> Create(SaveEmployeeRequest request)
    {
        var problem = await ValidateAgainstDatabase(request, existingId: null);
        if (problem is not null)
        {
            return problem;
        }

        var employee = new Employee { CreatedAt = DateTime.UtcNow };
        Apply(employee, request);

        _db.Employees.Add(employee);
        await _db.SaveChangesAsync();

        var departmentName = await _db.Departments
            .Where(d => d.Id == employee.DepartmentId)
            .Select(d => d.Name)
            .FirstAsync();

        return CreatedAtAction(nameof(GetById), new { id = employee.Id }, EmployeeDto.FromEntity(employee, departmentName));
    }

    // PUT: api/employees/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, SaveEmployeeRequest request)
    {
        var employee = await _db.Employees.FindAsync(id);
        if (employee is null)
        {
            return NotFound();
        }

        var problem = await ValidateAgainstDatabase(request, existingId: id);
        if (problem is not null)
        {
            return problem;
        }

        Apply(employee, request);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/employees/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var employee = await _db.Employees.FindAsync(id);
        if (employee is null)
        {
            return NotFound();
        }

        _db.Employees.Remove(employee);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private static void Apply(Employee employee, SaveEmployeeRequest request)
    {
        employee.FirstName = request.FirstName.Trim();
        employee.LastName = request.LastName.Trim();
        employee.Email = request.Email.Trim().ToLowerInvariant();
        employee.JobTitle = request.JobTitle.Trim();
        employee.DepartmentId = request.DepartmentId;
        employee.HireDate = request.HireDate!.Value;
        employee.Salary = request.Salary;
        employee.IsActive = request.IsActive;
    }

    // Rules that need the database: the department must exist and the email must be unique.
    // Reported per field so the client form can show each message under the right input.
    private async Task<ActionResult?> ValidateAgainstDatabase(SaveEmployeeRequest request, int? existingId)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (!await _db.Departments.AnyAsync(d => d.Id == request.DepartmentId))
        {
            ModelState.AddModelError(nameof(SaveEmployeeRequest.DepartmentId), "The selected department does not exist.");
        }

        if (await _db.Employees.AnyAsync(e => e.Email == email && e.Id != existingId))
        {
            ModelState.AddModelError(nameof(SaveEmployeeRequest.Email), "An employee with this email already exists.");
        }

        return ModelState.IsValid ? null : ValidationProblem(ModelState);
    }

    // Sort keys are whitelisted so the query string can never reach arbitrary columns.
    private static IQueryable<Employee> ApplySort(IQueryable<Employee> query, string? sortBy, bool descending)
    {
        IOrderedQueryable<Employee> Order<TKey>(Expression<Func<Employee, TKey>> key) =>
            descending ? query.OrderByDescending(key) : query.OrderBy(key);

        var ordered = (sortBy ?? "name").ToLowerInvariant() switch
        {
            "email" => Order(e => e.Email),
            "jobtitle" => Order(e => e.JobTitle),
            "department" or "departmentname" => Order(e => e.Department!.Name),
            "hiredate" => Order(e => e.HireDate),
            "salary" => Order(e => e.Salary),
            "isactive" => Order(e => e.IsActive),
            _ => Order(e => e.LastName)
        };

        // Stable tie-breakers so paging never repeats or skips rows.
        return ordered.ThenBy(e => e.FirstName).ThenBy(e => e.Id);
    }

    private static string EscapeLike(string value) =>
        value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
}
