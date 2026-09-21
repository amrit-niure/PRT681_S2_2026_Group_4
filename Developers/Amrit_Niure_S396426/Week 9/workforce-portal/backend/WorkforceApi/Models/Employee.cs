namespace WorkforceApi.Models;

public class Employee
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string JobTitle { get; set; } = string.Empty;

    public int DepartmentId { get; set; }

    public Department? Department { get; set; }

    public DateOnly HireDate { get; set; }

    public decimal Salary { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>When the record was created (UTC).</summary>
    public DateTime CreatedAt { get; set; }

    public List<Shift> Shifts { get; set; } = [];
}
