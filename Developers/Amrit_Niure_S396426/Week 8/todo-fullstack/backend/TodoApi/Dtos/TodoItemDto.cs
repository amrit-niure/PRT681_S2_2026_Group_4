using TodoApi.Models;

namespace TodoApi.Dtos;

/// <summary>
/// Shape returned to the client. Keeps the API contract separate from the EF entity.
/// </summary>
public record TodoItemDto(int Id, string Title, bool IsComplete, DateTime CreatedAt)
{
    public static TodoItemDto FromEntity(TodoItem item) =>
        new(item.Id, item.Title, item.IsComplete, item.CreatedAt);
}
