using System.ComponentModel.DataAnnotations;
using TicketingApi.Models;

namespace TicketingApi.Dtos;

public class CreateTicketRequest
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(4000, MinimumLength = 1)]
    public string Description { get; set; } = string.Empty;

    public TicketPriority Priority { get; set; } = TicketPriority.Medium;
}
