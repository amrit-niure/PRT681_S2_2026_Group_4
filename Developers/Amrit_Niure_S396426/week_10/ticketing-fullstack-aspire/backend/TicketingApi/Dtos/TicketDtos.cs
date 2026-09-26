using TicketingApi.Models;

namespace TicketingApi.Dtos;

/// <summary>Shape returned to the client. Keeps the API contract separate from the EF entity.</summary>
public record TicketDto(
    int Id,
    string Title,
    string Description,
    TicketStatus Status,
    TicketPriority Priority,
    string CreatedBy,
    string? AssignedToUserId,
    string? AssignedTo,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int CommentCount);

public record TicketCommentDto(int Id, string Author, string Body, DateTime CreatedAt);

public record TicketDetailDto(TicketDto Ticket, IReadOnlyList<TicketCommentDto> Comments);
