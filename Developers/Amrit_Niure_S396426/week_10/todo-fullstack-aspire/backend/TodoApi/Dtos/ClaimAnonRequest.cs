using System.ComponentModel.DataAnnotations;

namespace TodoApi.Dtos;

/// <summary>Payload for reassigning a guest's anonymous todos to their new account.</summary>
public class ClaimAnonRequest
{
    [Required]
    public string AnonId { get; set; } = string.Empty;
}
