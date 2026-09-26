using Microsoft.AspNetCore.Mvc;

namespace TicketingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DiagnosticsController : ControllerBase
{
    private readonly bool _enabled;

    public DiagnosticsController(IConfiguration configuration)
    {
        _enabled = configuration.GetValue<bool>("Diagnostics:Enabled");
    }

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
