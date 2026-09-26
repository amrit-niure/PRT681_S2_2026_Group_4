namespace TicketingApi.Models;

/// <summary>A reply on a ticket's conversation thread.</summary>
public class TicketComment
{
    public int Id { get; set; }

    public int TicketId { get; set; }

    public Ticket? Ticket { get; set; }

    /// <summary>Id of the Identity user who wrote the comment (AspNetUsers.Id).</summary>
    public string AuthorUserId { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
