using Microsoft.AspNetCore.Mvc;
using TodoApi.Notifications;

namespace TodoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RemindersController : ControllerBase
{
    private readonly OverdueReminderScanner _scanner;
    private readonly IWebHostEnvironment _environment;

    public RemindersController(OverdueReminderScanner scanner, IWebHostEnvironment environment)
    {
        _scanner = scanner;
        _environment = environment;
    }

    // POST: api/reminders/run
    // Runs one overdue-task scan immediately instead of waiting for the timer.
    // Available in Development only, so it can't be hit in production.
    [HttpPost("run")]
    public async Task<IActionResult> Run(CancellationToken cancellationToken)
    {
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        var remindedCount = await _scanner.RunAsync(cancellationToken);
        return Ok(new { remindedCount });
    }
}
