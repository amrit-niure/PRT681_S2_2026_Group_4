namespace TicketingApi.Models;

/// <summary>
/// A support ticket. This is the EF Core entity that maps to the Tickets table.
/// </summary>
public class Ticket
{
    public int Id { get; set; }

    /// <summary>Id of the Identity user who raised the ticket (AspNetUsers.Id).</summary>
    public string CreatedByUserId { get; set; } = string.Empty;

    /// <summary>Id of the Identity user working on the ticket. Null means unassigned.</summary>
    public string? AssignedToUserId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public TicketStatus Status { get; set; } = TicketStatus.Open;

    public TicketPriority Priority { get; set; } = TicketPriority.Medium;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<TicketComment> Comments { get; set; } = [];
}
