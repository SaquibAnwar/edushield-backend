# Rate Limiting Implementation

This document describes the rate limiting implementation in the EduShield backend API.

## Overview

Rate limiting is implemented to protect the API from abuse, ensure fair usage, and maintain system stability. The implementation uses ASP.NET Core's built-in rate limiting features with custom policies based on user roles and authentication status.

## Features

- **Role-based Rate Limiting**: Different limits for Admin, Teacher, Parent, and Student users
- **IP-based Limiting**: For unauthenticated users
- **Endpoint-specific Policies**: Different limits for different types of operations
- **Custom Error Handling**: Detailed error responses with retry information
- **Logging**: Comprehensive logging of rate limit violations

## Rate Limiting Policies

### 1. Global Policy
- **Limit**: 100 requests per minute
- **Scope**: All authenticated users
- **Queue**: 10 requests

### 2. AuthPolicy
- **Limit**: 5 requests per minute
- **Scope**: Authentication endpoints (login, register)
- **Queue**: 2 requests
- **Purpose**: Prevent brute force attacks

### 3. User-based Policies
Rate limits vary by user role:

#### Admin Users
- **Read Operations**: 500 requests/minute
- **Sensitive Operations**: 50 requests/minute
- **Queue**: 50 requests

#### Teacher Users
- **Read Operations**: 100 requests/minute
- **Sensitive Operations**: 20 requests/minute
- **Queue**: 10 requests

#### Parent Users
- **Read Operations**: 50 requests/minute
- **Sensitive Operations**: 5 requests/minute
- **Queue**: 5 requests

#### Student Users
- **Read Operations**: 30 requests/minute
- **Sensitive Operations**: 5 requests/minute
- **Queue**: 3 requests

#### Unauthenticated Users
- **All Operations**: 10 requests/minute
- **Queue**: 2 requests

### 4. Sensitive Operation Policy
Applied to create, update, and delete operations:
- More restrictive limits
- Role-based variations
- Prevents abuse of write operations

## Implementation Details

### Files Added/Modified

1. **Program.cs**
   - Added rate limiting configuration
   - Registered custom policies
   - Added middleware to pipeline

2. **RateLimitingMiddleware.cs**
   - Custom middleware for enhanced error handling
   - Detailed logging of rate limit violations
   - Structured error responses

3. **UserBasedRateLimitingPolicy.cs**
   - Custom policy provider
   - Role-based rate limiting logic
   - IP-based fallback for unauthenticated users

4. **Controllers**
   - Added `[EnableRateLimiting]` attributes
   - Applied appropriate policies to each controller

### Controllers with Rate Limiting

- **AuthController**: `AuthPolicy` (5 requests/minute)
- **StudentController**: `StudentPolicy` (role-based)
- **StudentPerformanceController**: `PerformancePolicy` (role-based)
- **UserController**: `AdminPolicy` (role-based)
- **HealthController**: `AdminPolicy` (role-based)

## Error Response Format

When rate limits are exceeded, the API returns a 429 (Too Many Requests) response:

```json
{
  "error": "Rate limit exceeded",
  "message": "Too many requests. Please try again later.",
  "retryAfter": 60,
  "policy": "AuthPolicy",
  "timestamp": "2024-01-01T00:00:00Z"
}
```

## Testing

Use the provided `test-rate-limiting.http` file to test the rate limiting implementation:

1. **Basic Testing**: Make multiple requests to trigger rate limits
2. **Role Testing**: Test with different user roles
3. **Endpoint Testing**: Test different endpoints with their specific policies
4. **Error Response Testing**: Verify error response format

## Configuration

Rate limiting can be configured in `Program.cs` by modifying the policy definitions:

```csharp
options.AddPolicy("CustomPolicy", httpContext =>
    RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: "custom-key",
        factory: _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 100,
            Window = TimeSpan.FromMinutes(1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 10
        }));
```

## Monitoring

Rate limiting events are logged with the following information:
- Client IP address
- Request path
- Policy name
- Retry after time
- User information (if authenticated)

## Best Practices

1. **Gradual Rollout**: Start with higher limits and reduce as needed
2. **Monitor Usage**: Track rate limit violations to adjust policies
3. **User Communication**: Inform users about rate limits in API documentation
4. **Graceful Degradation**: Provide meaningful error messages
5. **Whitelist Support**: Consider whitelisting trusted IPs for higher limits

## Troubleshooting

### Common Issues

1. **Rate Limits Too Restrictive**
   - Adjust policy limits in `Program.cs`
   - Consider user role requirements

2. **Rate Limits Not Working**
   - Verify middleware order in pipeline
   - Check policy names match controller attributes

3. **Inconsistent Behavior**
   - Ensure proper user authentication
   - Verify role claims are correctly set

### Debugging

Enable detailed logging to troubleshoot rate limiting issues:

```csharp
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Debug);
});
```

## Future Enhancements

1. **Redis-based Rate Limiting**: For distributed scenarios
2. **Dynamic Rate Limiting**: Adjust limits based on system load
3. **Rate Limit Headers**: Add rate limit information to response headers
4. **Whitelist Management**: Admin interface for managing whitelisted IPs
5. **Rate Limit Analytics**: Dashboard for monitoring rate limit usage
