using System.Net;
using System.Text.Json;
using EnglishPractice.Application.DTOs;

namespace EnglishPractice.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try { await _next(context); }
        catch (Exception ex)
        {
            var correlationId = context.Items["CorrelationId"]?.ToString();
            _logger.LogError(ex, "Unhandled exception. CorrelationId={CorrelationId}", correlationId);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = ex switch
            {
                UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
                InvalidOperationException => (int)HttpStatusCode.BadRequest,
                OperationCanceledException or TaskCanceledException => (int)HttpStatusCode.RequestTimeout,
                _ => (int)HttpStatusCode.InternalServerError
            };

            var message = _env.IsDevelopment() ? ex.Message : "An unexpected error occurred";
            var body = JsonSerializer.Serialize(ApiResponse<object>.Fail(message, correlationId));
            await context.Response.WriteAsync(body);
        }
    }
}
