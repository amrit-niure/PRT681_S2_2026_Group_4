using System.ComponentModel.DataAnnotations;

namespace WorkforceApi.Dtos;

/// <summary>Payload for creating or updating a shift. Validated automatically by [ApiController].</summary>
public class SaveShiftRequest : IValidatableObject
{
    [Range(1, int.MaxValue, ErrorMessage = "Select an employee.")]
    public int EmployeeId { get; set; }

    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    /// <summary>Send an ISO 8601 timestamp with an offset (e.g. the browser's toISOString()).</summary>
    [Required]
    public DateTimeOffset? Start { get; set; }

    [Required]
    public DateTimeOffset? End { get; set; }

    public bool IsAllDay { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Start is { } start && End is { } end && end <= start)
        {
            yield return new ValidationResult("End must be after the start.", [nameof(End)]);
        }
    }
}
