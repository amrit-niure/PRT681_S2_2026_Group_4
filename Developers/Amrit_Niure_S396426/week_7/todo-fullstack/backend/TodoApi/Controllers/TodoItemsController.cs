using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Dtos;
using TodoApi.Models;

namespace TodoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TodoItemsController : ControllerBase
{
    private readonly TodoDbContext _db;

    public TodoItemsController(TodoDbContext db)
    {
        _db = db;
    }

    // GET: api/todoitems
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TodoItemDto>>> GetAll()
    {
        var items = await _db.TodoItems
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => TodoItemDto.FromEntity(t))
            .ToListAsync();

        return Ok(items);
    }

    // GET: api/todoitems/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<TodoItemDto>> GetById(int id)
    {
        var item = await _db.TodoItems.FindAsync(id);
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
            Title = request.Title.Trim(),
            IsComplete = request.IsComplete,
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
        var item = await _db.TodoItems.FindAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        item.Title = request.Title.Trim();
        item.IsComplete = request.IsComplete;
        await _db.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/todoitems/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.TodoItems.FindAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        _db.TodoItems.Remove(item);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
