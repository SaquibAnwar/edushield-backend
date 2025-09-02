using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using System.Net;

namespace EduShield.Api.Middleware;

/// <summary>
/// Custom rate limiting middleware for enhanced logging and error handling
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;

    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
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
        catch (Exception ex) when (ex.Message.Contains("rate limit") || ex.Message.Contains("Rate limit"))
        {
            _logger.LogWarning("Rate limit exceeded for {ClientIP} on {Path}. Error: {Error}",
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                context.Request.Path,
                ex.Message);

            await HandleRateLimitExceeded(context);
        }
    }

    private static async Task HandleRateLimitExceeded(HttpContext context)
    {
        context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
        context.Response.ContentType = "application/json";

        var response = new
        {
            error = "Rate limit exceeded",
            message = "Too many requests. Please try again later.",
            retryAfter = 60,
            policy = "unknown",
            timestamp = DateTime.UtcNow
        };

        // Add retry-after header
        context.Response.Headers["Retry-After"] = "60";

        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
    }
}

/// <summary>
/// Extension methods for registering the rate limiting middleware
/// </summary>
public static class RateLimitingMiddlewareExtensions
{
    public static IApplicationBuilder UseCustomRateLimiting(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RateLimitingMiddleware>();
    }
}
