using Microsoft.AspNetCore.Mvc;

namespace TicketingApi.Controllers;

/// <summary>
/// Deliberately throws so the exception-tracking pipeline (Serilog/Seq, ELMAH, Exceptionless)
/// can be verified end to end. Disabled unless Diagnostics:Enabled is true.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DiagnosticsController : ControllerBase
{
    private readonly bool _enabled;

    public DiagnosticsController(IConfiguration configuration)
    {
        _enabled = configuration.GetValue<bool>("Diagnostics:Enabled");
    }

    // GET: api/diagnostics/throw
    [HttpGet("throw")]
    public IActionResult Throw()
    {
        if (!_enabled)
        {
            return NotFound();
        }

        throw new InvalidOperationException("Test exception from /api/diagnostics/throw.");
    }
}
