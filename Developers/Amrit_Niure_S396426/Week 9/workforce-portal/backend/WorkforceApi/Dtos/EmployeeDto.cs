using WorkforceApi.Models;

namespace WorkforceApi.Dtos;

public class EmployeeDto
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string JobTitle { get; set; } = string.Empty;

    public int DepartmentId { get; set; }

    public string DepartmentName { get; set; } = string.Empty;

    /// <summary>Serialized as a plain date (yyyy-MM-dd).</summary>
    public DateOnly HireDate { get; set; }

    public decimal Salary { get; set; }

    public bool IsActive { get; set; }

    public static EmployeeDto FromEntity(Employee employee, string departmentName) => new()
    {
        Id = employee.Id,
        FirstName = employee.FirstName,
        LastName = employee.LastName,
        Email = employee.Email,
        JobTitle = employee.JobTitle,
        DepartmentId = employee.DepartmentId,
        DepartmentName = departmentName,
        HireDate = employee.HireDate,
        Salary = employee.Salary,
        IsActive = employee.IsActive
    };
}
