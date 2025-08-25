using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EduShield.Core.Enums;

namespace EduShield.Api.Tests.Helpers;

/// <summary>
/// Helper class for setting up authentication and authorization in tests
/// </summary>
public static class TestAuthenticationHelper
{
    /// <summary>
    /// Creates a claims principal with the specified role
    /// </summary>
    /// <param name="role">User role</param>
    /// <param name="userId">Optional user ID</param>
    /// <param name="email">Optional email</param>
    /// <returns>Claims principal for testing</returns>
    public static ClaimsPrincipal CreateUserWithRole(UserRole role, Guid? userId = null, string? email = null)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Role, role.ToString()),
            new(ClaimTypes.NameIdentifier, (userId ?? Guid.NewGuid()).ToString()),
            new(ClaimTypes.Email, email ?? $"test-{role.ToString().ToLower()}@example.com"),
            new(ClaimTypes.Name, $"Test {role}")
        };

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
    }

    /// <summary>
    /// Creates a controller context with the specified user
    /// </summary>
    /// <param name="user">Claims principal</param>
    /// <returns>Controller context for testing</returns>
    public static ControllerContext CreateControllerContext(ClaimsPrincipal user)
    {
        var httpContext = new DefaultHttpContext
        {
            User = user
        };

        return new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    /// <summary>
    /// Creates a controller context with a user having the specified role
    /// </summary>
    /// <param name="role">User role</param>
    /// <param name="userId">Optional user ID</param>
    /// <returns>Controller context for testing</returns>
    public static ControllerContext CreateControllerContextWithRole(UserRole role, Guid? userId = null)
    {
        var user = CreateUserWithRole(role, userId);
        return CreateControllerContext(user);
    }

    /// <summary>
    /// Sets up the controller context for a controller
    /// </summary>
    /// <param name="controller">Controller instance</param>
    /// <param name="role">User role</param>
    /// <param name="userId">Optional user ID</param>
    public static void SetupControllerContext(ControllerBase controller, UserRole role, Guid? userId = null)
    {
        controller.ControllerContext = CreateControllerContextWithRole(role, userId);
    }

    /// <summary>
    /// Creates a test user ID for consistent testing
    /// </summary>
    /// <param name="seed">Seed value for consistent ID generation</param>
    /// <returns>Deterministic GUID for testing</returns>
    public static Guid CreateTestUserId(int seed = 1)
    {
        var bytes = new byte[16];
        new Random(seed).NextBytes(bytes);
        return new Guid(bytes);
    }

    /// <summary>
    /// Creates a test student ID for consistent testing
    /// </summary>
    /// <param name="seed">Seed value for consistent ID generation</param>
    /// <returns>Deterministic GUID for testing</returns>
    public static Guid CreateTestStudentId(int seed = 2)
    {
        var bytes = new byte[16];
        new Random(seed).NextBytes(bytes);
        return new Guid(bytes);
    }

    /// <summary>
    /// Creates a test faculty ID for consistent testing
    /// </summary>
    /// <param name="seed">Seed value for consistent ID generation</param>
    /// <returns>Deterministic GUID for testing</returns>
    public static Guid CreateTestFacultyId(int seed = 3)
    {
        var bytes = new byte[16];
        new Random(seed).NextBytes(bytes);
        return new Guid(bytes);
    }

    /// <summary>
    /// Creates a test performance ID for consistent testing
    /// </summary>
    /// <param name="seed">Seed value for consistent ID generation</param>
    /// <returns>Deterministic GUID for testing</returns>
    public static Guid CreateTestPerformanceId(int seed = 4)
    {
        var bytes = new byte[16];
        new Random(seed).NextBytes(bytes);
        return new Guid(bytes);
    }
}
