namespace TodoApi.Models;

/// <summary>
/// A single task in the to-do list. This is the EF Core entity that maps to the TodoItems table.
/// </summary>
public class TodoItem
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public bool IsComplete { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
