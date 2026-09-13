using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TodoApi.Controllers;

/// <summary>
/// Base for controllers whose data belongs to a "user" that may or may not have an
/// account. Signed-in requests are scoped to the Identity user id; anonymous requests
/// are scoped to a client-generated id sent in the <see cref="AnonymousIdHeader"/>
/// header, so people can use the app without registering. Only signing up gets you
/// email reminders, since that requires a real account with an email address.
/// </summary>
public abstract class OwnedResourceController : ControllerBase, IActionFilter
{
    private const string AnonymousIdHeader = "X-Anon-Id";

    protected string OwnerId { get; private set; } = string.Empty;

    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            OwnerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        }
        else if (Request.Headers.TryGetValue(AnonymousIdHeader, out var anonId)
                 && Guid.TryParse(anonId, out _))
        {
            // Prefixed so it can never collide with a real Identity user id.
            OwnerId = $"anon:{anonId}";
        }
        else
        {
            context.Result = BadRequest($"Missing or invalid {AnonymousIdHeader} header.");
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}
