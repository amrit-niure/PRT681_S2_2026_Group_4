using Microsoft.AspNetCore.Mvc;
using TodoApi.Notifications;

namespace TodoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RemindersController : OwnedResourceController
{
    private readonly OverdueReminderScanner _scanner;

    public RemindersController(OverdueReminderScanner scanner)
    {
        _scanner = scanner;
    }

    // POST: api/reminders/run
    // Emails the signed-in user their due tasks now, instead of waiting for the timer.
    // Anonymous callers have no account/email on file, so the scanner just reports 0.
    [HttpPost("run")]
    public async Task<IActionResult> Run(CancellationToken cancellationToken)
    {
        var remindedCount = await _scanner.RunForUserAsync(OwnerId, cancellationToken);
        return Ok(new { remindedCount });
    }
}
