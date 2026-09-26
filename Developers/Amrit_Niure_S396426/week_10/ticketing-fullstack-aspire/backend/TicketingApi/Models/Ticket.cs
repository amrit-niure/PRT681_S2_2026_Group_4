namespace TicketingApi.Models;

public class Ticket
{
    public int Id { get; set; }

    public string CreatedByUserId { get; set; } = string.Empty;

    public string? AssignedToUserId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public TicketStatus Status { get; set; } = TicketStatus.Open;

    public TicketPriority Priority { get; set; } = TicketPriority.Medium;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<TicketComment> Comments { get; set; } = [];
}
