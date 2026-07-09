using System.Diagnostics;
using System.Text.Json;

namespace DMS_Backend.Middleware
{
    /// <summary>
    /// Global safety net: any unhandled exception is logged (with a correlation id)
    /// and returned as a clean JSON ProblemDetails response — internal details are
    /// never leaked to the client.
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
                var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
                _logger.LogError(ex, "Unhandled exception. TraceId={TraceId} Path={Path}",
                    traceId, context.Request.Path);

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/problem+json";

                var problem = new
                {
                    type = "https://httpstatuses.com/500",
                    title = "An unexpected error occurred.",
                    status = 500,
                    traceId
                };
                await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
            }
        }
    }
}
