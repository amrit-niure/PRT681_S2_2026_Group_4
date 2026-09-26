using TicketingApi.Models;

namespace TicketingApi.Dtos;

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

public record TicketStatsDto(
    int Total,
    int Mine,
    int Unassigned,
    int[] ByStatus,
    int[] ByPriority);
