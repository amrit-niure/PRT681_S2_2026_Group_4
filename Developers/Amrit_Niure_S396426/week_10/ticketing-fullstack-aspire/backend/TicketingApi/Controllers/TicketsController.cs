using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketingApi.Data;
using TicketingApi.Dtos;
using TicketingApi.Models;
using TicketingApi.Workflows;

namespace TicketingApi.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class TicketsController : ControllerBase
{
    private readonly TicketingDbContext _db;
    private readonly TicketEmailDispatcher _emails;
    private readonly ILogger<TicketsController> _logger;

    public TicketsController(TicketingDbContext db, TicketEmailDispatcher emails, ILogger<TicketsController> logger)
    {
        _db = db;
        _emails = emails;
        _logger = logger;
    }

    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TicketDto>>> GetAll(TicketStatus? status, bool mine = false)
    {
        var query = _db.Tickets.AsNoTracking();
        if (status is not null)
        {
            query = query.Where(t => t.Status == status);
        }
        if (mine)
        {
            query = query.Where(t => t.CreatedByUserId == CurrentUserId);
        }

        var tickets = await query
            .OrderBy(t => t.Status == TicketStatus.Closed || t.Status == TicketStatus.Resolved)
            .ThenByDescending(t => t.Priority)
            .ThenByDescending(t => t.CreatedAt)
            .ToListAsync();

        return Ok(await ToDtosAsync(tickets));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TicketDetailDto>> GetById(int id)
    {
        var ticket = await _db.Tickets.AsNoTracking()
            .Include(t => t.Comments.OrderBy(c => c.CreatedAt))
            .FirstOrDefaultAsync(t => t.Id == id);
        if (ticket is null)
        {
            return NotFound();
        }

        var dto = (await ToDtosAsync([ticket]))[0];
        var emails = await EmailsAsync(ticket.Comments.Select(c => c.AuthorUserId));
        var comments = ticket.Comments
            .Select(c => new TicketCommentDto(c.Id, emails.GetValueOrDefault(c.AuthorUserId, "unknown"), c.Body, c.CreatedAt))
            .ToList();

        return Ok(new TicketDetailDto(dto, comments));
    }

    [HttpPost]
    public async Task<ActionResult<TicketDto>> Create(CreateTicketRequest request)
    {
        var now = DateTime.UtcNow;
        var ticket = new Ticket
        {
            CreatedByUserId = CurrentUserId,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Priority = request.Priority,
            CreatedAt = now,
            UpdatedAt = now
        };

        _db.Tickets.Add(ticket);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Ticket {TicketId} created by {UserId} with priority {Priority}.",
            ticket.Id, ticket.CreatedByUserId, ticket.Priority);

        var dto = (await ToDtosAsync([ticket]))[0];

        await NotifyCreatorAsync(ticket, dto.CreatedBy,
            $"[Ticket #{ticket.Id}] We received your request: {ticket.Title}",
            $"Hi,\n\nYour ticket #{ticket.Id} \"{ticket.Title}\" has been logged with {ticket.Priority} priority.\n" +
            "We will let you know when its status changes.\n\n— Ticketing");

        return CreatedAtAction(nameof(GetById), new { id = ticket.Id }, dto);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateTicketRequest request)
    {
        var ticket = await _db.Tickets.FirstOrDefaultAsync(t => t.Id == id);
        if (ticket is null)
        {
            return NotFound();
        }

        if (request.AssignedToUserId is not null
            && !await _db.Users.AnyAsync(u => u.Id == request.AssignedToUserId))
        {
            return BadRequest("AssignedToUserId does not match an existing user.");
        }

        var previousStatus = ticket.Status;

        ticket.Title = request.Title.Trim();
        ticket.Description = request.Description.Trim();
        ticket.Priority = request.Priority;
        ticket.Status = request.Status;
        ticket.AssignedToUserId = request.AssignedToUserId;
        ticket.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        if (previousStatus != ticket.Status)
        {
            _logger.LogInformation("Ticket {TicketId} moved from {PreviousStatus} to {Status} by {UserId}.",
                ticket.Id, previousStatus, ticket.Status, CurrentUserId);

            var creatorEmail = (await EmailsAsync([ticket.CreatedByUserId])).GetValueOrDefault(ticket.CreatedByUserId);
            await NotifyCreatorAsync(ticket, creatorEmail,
                $"[Ticket #{ticket.Id}] Status changed to {ticket.Status}",
                $"Hi,\n\nYour ticket #{ticket.Id} \"{ticket.Title}\" moved from {previousStatus} to {ticket.Status}.\n\n— Ticketing");
        }

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ticket = await _db.Tickets.FirstOrDefaultAsync(t => t.Id == id);
        if (ticket is null)
        {
            return NotFound();
        }

        if (ticket.CreatedByUserId != CurrentUserId)
        {
            return Forbid();
        }

        _db.Tickets.Remove(ticket);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Ticket {TicketId} deleted by {UserId}.", id, CurrentUserId);
        return NoContent();
    }

    [HttpPost("{id:int}/comments")]
    public async Task<ActionResult<TicketCommentDto>> AddComment(int id, AddCommentRequest request)
    {
        var ticket = await _db.Tickets.FirstOrDefaultAsync(t => t.Id == id);
        if (ticket is null)
        {
            return NotFound();
        }

        var comment = new TicketComment
        {
            TicketId = id,
            AuthorUserId = CurrentUserId,
            Body = request.Body.Trim(),
            CreatedAt = DateTime.UtcNow
        };
        ticket.UpdatedAt = comment.CreatedAt;
        _db.TicketComments.Add(comment);
        await _db.SaveChangesAsync();

        var author = User.FindFirstValue(ClaimTypes.Email) ?? User.Identity?.Name ?? "unknown";
        return Created($"/api/tickets/{id}", new TicketCommentDto(comment.Id, author, comment.Body, comment.CreatedAt));
    }

    [HttpGet("stats")]
    public async Task<ActionResult<TicketStatsDto>> GetStats()
    {
        var userId = CurrentUserId;
        var tickets = _db.Tickets.AsNoTracking();

        var statusCounts = await tickets
            .GroupBy(t => t.Status)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToListAsync();
        var priorityCounts = await tickets
            .GroupBy(t => t.Priority)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToListAsync();

        var byStatus = new int[Enum.GetValues<TicketStatus>().Length];
        foreach (var row in statusCounts) byStatus[(int)row.Key] = row.Count;
        var byPriority = new int[Enum.GetValues<TicketPriority>().Length];
        foreach (var row in priorityCounts) byPriority[(int)row.Key] = row.Count;

        var mine = await tickets.CountAsync(t => t.CreatedByUserId == userId);
        var unassigned = await tickets.CountAsync(t =>
            t.AssignedToUserId == null && t.Status != TicketStatus.Closed && t.Status != TicketStatus.Resolved);

        return Ok(new TicketStatsDto(byStatus.Sum(), mine, unassigned, byStatus, byPriority));
    }

    [HttpGet("assignees")]
    public async Task<ActionResult<IEnumerable<object>>> GetAssignees()
    {
        var users = await _db.Users.AsNoTracking()
            .OrderBy(u => u.Email)
            .Select(u => new { u.Id, u.Email })
            .ToListAsync();
        return Ok(users);
    }

    private Task NotifyCreatorAsync(Ticket ticket, string? to, string subject, string body) =>
        string.IsNullOrWhiteSpace(to) || to == "unknown"
            ? Task.CompletedTask
            : _emails.DispatchAsync(new TicketEmailRequest(ticket.Id, to, subject, body));

    private async Task<Dictionary<string, string>> EmailsAsync(IEnumerable<string?> userIds)
    {
        var ids = userIds.Where(id => id is not null).Distinct().ToList();
        return await _db.Users.AsNoTracking()
            .Where(u => ids.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.Email ?? u.UserName ?? "unknown");
    }

    private async Task<List<TicketDto>> ToDtosAsync(List<Ticket> tickets)
    {
        var emails = await EmailsAsync(tickets.SelectMany(t => new[] { t.CreatedByUserId, t.AssignedToUserId }));
        var ids = tickets.Select(t => t.Id).ToList();
        var commentCounts = await _db.TicketComments.AsNoTracking()
            .Where(c => ids.Contains(c.TicketId))
            .GroupBy(c => c.TicketId)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.Key, g => g.Count);

        return tickets.Select(t => new TicketDto(
            t.Id,
            t.Title,
            t.Description,
            t.Status,
            t.Priority,
            emails.GetValueOrDefault(t.CreatedByUserId, "unknown"),
            t.AssignedToUserId,
            t.AssignedToUserId is null ? null : emails.GetValueOrDefault(t.AssignedToUserId),
            t.CreatedAt,
            t.UpdatedAt,
            commentCounts.GetValueOrDefault(t.Id))).ToList();
    }
}
