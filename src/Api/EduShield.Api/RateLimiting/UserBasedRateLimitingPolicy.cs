using System.Security.Claims;
using System.Threading.RateLimiting;
using EduShield.Core.Enums;

namespace EduShield.Api.RateLimiting;

/// <summary>
/// Custom rate limiting policy that provides different limits based on user roles
/// </summary>
public class UserBasedRateLimitingPolicy
{
    /// <summary>
    /// Creates a rate limiter based on user role and authentication status
    /// </summary>
    public static RateLimitPartition<string> GetUserBasedRateLimiter(HttpContext httpContext)
    {
        var user = httpContext.User;
        var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        
        // If user is not authenticated, use IP-based limiting
        if (!user.Identity?.IsAuthenticated == true)
        {
            return RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: clientIp,
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 10, // Very restrictive for unauthenticated users
                    Window = TimeSpan.FromMinutes(1),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 2
                });
        }

        // Get user role from claims
        var userRole = user.FindFirst(ClaimTypes.Role)?.Value;
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? clientIp;

        return userRole switch
        {
            nameof(UserRole.Admin) => RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: userId,
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 500, // High limit for admins
                    Window = TimeSpan.FromMinutes(1),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 50
                }),

            nameof(UserRole.Faculty) => RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: userId,
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 100, // Moderate limit for teachers
                    Window = TimeSpan.FromMinutes(1),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 10
                }),

            nameof(UserRole.Parent) => RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: userId,
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 50, // Lower limit for parents
                    Window = TimeSpan.FromMinutes(1),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 5
                }),

            nameof(UserRole.Student) => RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: userId,
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 30, // Lower limit for students
                    Window = TimeSpan.FromMinutes(1),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 3
                }),

            _ => RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: userId,
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 20, // Default limit for authenticated users without specific role
                    Window = TimeSpan.FromMinutes(1),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 2
                })
        };
    }

    /// <summary>
    /// Creates a rate limiter specifically for authentication endpoints
    /// </summary>
    public static RateLimitPartition<string> GetAuthRateLimiter(HttpContext httpContext)
    {
        var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: clientIp,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5, // Very restrictive for auth endpoints
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 1
            });
    }

    /// <summary>
    /// Creates a rate limiter for sensitive operations (create, update, delete)
    /// </summary>
    public static RateLimitPartition<string> GetSensitiveOperationRateLimiter(HttpContext httpContext)
    {
        var user = httpContext.User;
        var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        
        if (!user.Identity?.IsAuthenticated == true)
        {
            return RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: clientIp,
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 1, // Very restrictive for unauthenticated users
                    Window = TimeSpan.FromMinutes(1),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0
                });
        }

        var userRole = user.FindFirst(ClaimTypes.Role)?.Value;
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? clientIp;

        return userRole switch
        {
            nameof(UserRole.Admin) => RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: userId,
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 50, // High limit for admins
                    Window = TimeSpan.FromMinutes(1),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 10
                }),

            nameof(UserRole.Faculty) => RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: userId,
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 20, // Moderate limit for teachers
                    Window = TimeSpan.FromMinutes(1),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 3
                }),

            _ => RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: userId,
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 5, // Lower limit for other roles
                    Window = TimeSpan.FromMinutes(1),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 1
                })
        };
    }
}
