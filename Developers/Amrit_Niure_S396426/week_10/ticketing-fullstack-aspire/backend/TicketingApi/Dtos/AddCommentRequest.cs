using System.ComponentModel.DataAnnotations;

namespace TicketingApi.Dtos;

public class AddCommentRequest
{
    [Required]
    [StringLength(4000, MinimumLength = 1)]
    public string Body { get; set; } = string.Empty;
}
