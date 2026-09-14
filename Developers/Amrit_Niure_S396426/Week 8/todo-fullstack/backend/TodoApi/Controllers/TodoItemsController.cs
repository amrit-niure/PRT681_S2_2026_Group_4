using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Dtos;
using TodoApi.Models;

namespace TodoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TodoItemsController : ControllerBase
{
    private readonly TodoDbContext _db;

    public TodoItemsController(TodoDbContext db)
    {
        _db = db;
    }

    /// <summary>Id of the signed-in user; every query is scoped to this.</summary>
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    // GET: api/todoitems
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TodoItemDto>>> GetAll()
    {
        // Unfinished tasks first, then soonest due date (tasks with no due date last),
        // and newest first as a tie-breaker.
        var items = await _db.TodoItems
            .Where(t => t.UserId == UserId)
            .OrderBy(t => t.IsComplete)
            .ThenBy(t => t.DueDate == null)
            .ThenBy(t => t.DueDate)
            .ThenByDescending(t => t.CreatedAt)
            .Select(t => TodoItemDto.FromEntity(t))
            .ToListAsync();

        return Ok(items);
    }

    // GET: api/todoitems/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<TodoItemDto>> GetById(int id)
    {
        var item = await _db.TodoItems.FirstOrDefaultAsync(t => t.Id == id && t.UserId == UserId);
        if (item is null)
        {
            return NotFound();
        }

        return Ok(TodoItemDto.FromEntity(item));
    }

    // POST: api/todoitems
    [HttpPost]
    public async Task<ActionResult<TodoItemDto>> Create(SaveTodoItemRequest request)
    {
        var item = new TodoItem
        {
            UserId = UserId,
            Title = request.Title.Trim(),
            IsComplete = request.IsComplete,
            DueDate = request.DueDate,
            CreatedAt = DateTime.UtcNow
        };

        _db.TodoItems.Add(item);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = item.Id }, TodoItemDto.FromEntity(item));
    }

    // PUT: api/todoitems/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, SaveTodoItemRequest request)
    {
        var item = await _db.TodoItems.FirstOrDefaultAsync(t => t.Id == id && t.UserId == UserId);
        if (item is null)
        {
            return NotFound();
        }

        // A moved deadline or a reopened task should trigger a fresh reminder.
        if (item.DueDate != request.DueDate || (item.IsComplete && !request.IsComplete))
        {
            item.ReminderSent = false;
        }

        item.Title = request.Title.Trim();
        item.IsComplete = request.IsComplete;
        item.DueDate = request.DueDate;
        await _db.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/todoitems/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.TodoItems.FirstOrDefaultAsync(t => t.Id == id && t.UserId == UserId);
        if (item is null)
        {
            return NotFound();
        }

        _db.TodoItems.Remove(item);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
