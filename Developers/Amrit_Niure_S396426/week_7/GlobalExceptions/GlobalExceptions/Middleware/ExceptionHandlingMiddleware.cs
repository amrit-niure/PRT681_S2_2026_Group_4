using System.Net;
using System.Text.Json;

namespace GlobalExceptions.Middleware
{
    public sealed class ExceptionHandlingMiddleware 
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception for {Path}", context.Request.Path);
                await WriteProblemDetailsAsync(context, ex);
            }
        }

        private async Task WriteProblemDetailsAsync(HttpContext context, Exception ex)
        {
            var statusCode = ex switch
            {
                KeyNotFoundException => HttpStatusCode.NotFound,
                ArgumentException => HttpStatusCode.BadRequest,
                UnauthorizedAccessException => HttpStatusCode.Unauthorized,
                _ => HttpStatusCode.InternalServerError
            };

            var problem = new {
            type = $"https://httpstatuses.io/{(int)statusCode}",
                title = statusCode == HttpStatusCode.InternalServerError ? " An unexpected error occoured." : ex.Message,
            status = (int)statusCode,
            traceId = context.TraceIdentifier
            };

            context.Response.Clear();
            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(problem));

        }
    }
}
