using System.ComponentModel.DataAnnotations;

namespace WorkforceApi.Dtos;

/// <summary>Payload for creating or updating a department. Validated automatically by [ApiController].</summary>
public class SaveDepartmentRequest
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }
}
