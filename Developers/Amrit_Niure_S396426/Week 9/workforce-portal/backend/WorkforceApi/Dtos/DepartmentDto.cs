using WorkforceApi.Models;

namespace WorkforceApi.Dtos;

public class DepartmentDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int EmployeeCount { get; set; }

    public static DepartmentDto FromEntity(Department department, int employeeCount) => new()
    {
        Id = department.Id,
        Name = department.Name,
        Description = department.Description,
        EmployeeCount = employeeCount
    };
}
