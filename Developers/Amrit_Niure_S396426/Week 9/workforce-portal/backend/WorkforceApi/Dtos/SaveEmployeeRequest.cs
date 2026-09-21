using System.ComponentModel.DataAnnotations;

namespace WorkforceApi.Dtos;

/// <summary>
/// Payload for creating or updating an employee. Validated automatically by [ApiController];
/// the frontend form mirrors these rules so users get the same feedback before submitting.
/// </summary>
public class SaveEmployeeRequest : IValidatableObject
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(254)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string JobTitle { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Select a department.")]
    public int DepartmentId { get; set; }

    [Required]
    public DateOnly? HireDate { get; set; }

    [Range(0, 1_000_000)]
    public decimal Salary { get; set; }

    public bool IsActive { get; set; } = true;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (HireDate is { } hireDate && hireDate > DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)))
        {
            yield return new ValidationResult(
                "Hire date can't be in the future.",
                [nameof(HireDate)]);
        }
    }
}
