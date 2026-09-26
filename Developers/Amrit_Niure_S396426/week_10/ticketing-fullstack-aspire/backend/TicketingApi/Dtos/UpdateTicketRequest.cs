using System.ComponentModel.DataAnnotations;
using TicketingApi.Models;

namespace TicketingApi.Dtos;

/// <summary>Payload for editing a ticket's details, status and assignee.</summary>
public class UpdateTicketRequest
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(4000, MinimumLength = 1)]
    public string Description { get; set; } = string.Empty;

    public TicketPriority Priority { get; set; }

    public TicketStatus Status { get; set; }

    /// <summary>Id of the user the ticket is assigned to. Null unassigns it.</summary>
    public string? AssignedToUserId { get; set; }
}
