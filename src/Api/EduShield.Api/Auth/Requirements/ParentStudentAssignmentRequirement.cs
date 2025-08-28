using Microsoft.AspNetCore.Authorization;

namespace EduShield.Api.Auth.Requirements;

/// <summary>
/// Authorization requirement for parent-student assignment operations
/// Only Admin and DevAuth roles can manage parent-student assignments
/// </summary>
public class ParentStudentAssignmentRequirement : IAuthorizationRequirement
{
    public string RequiredRole { get; }

    public ParentStudentAssignmentRequirement(string requiredRole = "Admin,DevAuth")
    {
        RequiredRole = requiredRole;
    }
}

/// <summary>
/// Authorization handler for parent-student assignment operations
/// </summary>
public class ParentStudentAssignmentHandler : AuthorizationHandler<ParentStudentAssignmentRequirement>
{
    private readonly ILogger<ParentStudentAssignmentHandler> _logger;

    public ParentStudentAssignmentHandler(ILogger<ParentStudentAssignmentHandler> logger)
    {
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ParentStudentAssignmentRequirement requirement)
    {
        try
        {
            // Check if user is authenticated
            if (!context.User.Identity?.IsAuthenticated ?? true)
            {
                _logger.LogWarning("User is not authenticated for parent-student assignment operation");
                context.Fail();
                return Task.CompletedTask;
            }

            // Get user role from claims
            var roleClaim = context.User.FindFirst("role") ?? context.User.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role");
            
            if (roleClaim == null)
            {
                _logger.LogWarning("No role claim found for user {UserId}", context.User.Identity.Name);
                context.Fail();
                return Task.CompletedTask;
            }

            var userRole = roleClaim.Value;
            var allowedRoles = requirement.RequiredRole.Split(',').Select(r => r.Trim()).ToList();

            // Check if user has required role
            if (allowedRoles.Contains(userRole, StringComparer.OrdinalIgnoreCase))
            {
                _logger.LogInformation("User {UserId} with role {UserRole} authorized for parent-student assignment operation", 
                    context.User.Identity.Name, userRole);
                context.Succeed(requirement);
            }
            else
            {
                _logger.LogWarning("User {UserId} with role {UserRole} not authorized for parent-student assignment operation. Required roles: {RequiredRoles}", 
                    context.User.Identity.Name, userRole, requirement.RequiredRole);
                context.Fail();
            }

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in parent-student assignment authorization handler");
            context.Fail();
            return Task.CompletedTask;
        }
    }
}