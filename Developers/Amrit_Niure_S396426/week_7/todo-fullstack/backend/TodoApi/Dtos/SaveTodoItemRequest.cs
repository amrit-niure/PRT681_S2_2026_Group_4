using System.ComponentModel.DataAnnotations;

namespace TodoApi.Dtos;

/// <summary>
/// Payload for creating or updating a to-do item. Validated automatically by [ApiController].
/// </summary>
public class SaveTodoItemRequest
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    public bool IsComplete { get; set; }
}
