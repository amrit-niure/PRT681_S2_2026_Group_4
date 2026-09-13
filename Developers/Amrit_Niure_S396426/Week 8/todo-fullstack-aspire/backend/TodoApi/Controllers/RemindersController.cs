using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoApi.Notifications;

namespace TodoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RemindersController : ControllerBase
{
    private readonly OverdueReminderScanner _scanner;

    public RemindersController(OverdueReminderScanner scanner)
    {
        _scanner = scanner;
    }

    // POST: api/reminders/run
    // Emails the signed-in user their overdue tasks now, instead of waiting for the timer.
    [HttpPost("run")]
    public async Task<IActionResult> Run(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var remindedCount = await _scanner.RunForUserAsync(userId, cancellationToken);
        return Ok(new { remindedCount });
    }
}
