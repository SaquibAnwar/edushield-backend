using EduShield.Api.Auth.Requirements;
using EduShield.Core.Dtos;
using EduShield.Core.Entities;
using EduShield.Core.Enums;
using EduShield.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace EduShield.Api.Auth.Handlers;

public class StudentAuthorizationHandler : AuthorizationHandler<StudentAccessRequirement, StudentDto>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<StudentAuthorizationHandler> _logger;

    public StudentAuthorizationHandler(IUserRepository userRepository, ILogger<StudentAuthorizationHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        StudentAccessRequirement requirement,
        StudentDto student)
    {
        var user = context.User;
        if (!user.Identity?.IsAuthenticated == true)
        {
            _logger.LogWarning("Unauthenticated user attempted to access student data");
            return;
        }

        var userRole = GetUserRole(user);
        var userId = GetUserId(user);

        // Admin and Dev have full access
        if (requirement.AllowAdminAccess && (userRole == UserRole.Admin || userRole == UserRole.DevAuth))
        {
            context.Succeed(requirement);
            return;
        }

        // Student can only access their own data
        if (requirement.AllowSelfAccess && userRole == UserRole.Student)
        {
            if (userId.HasValue && student.UserId == userId.Value)
            {
                context.Succeed(requirement);
                return;
            }
            _logger.LogWarning("Student {StudentId} attempted to access other student data", userId);
            return;
        }

        // Faculty can access students assigned to them
        if (requirement.AllowFacultyAccess && userRole == UserRole.Faculty)
        {
            if (userId.HasValue)
            {
                var faculty = await _userRepository.GetByIdAsync(userId.Value);
                if (faculty != null && student.AssignedFaculties.Any(f => f.Id == faculty.Id))
                {
                    context.Succeed(requirement);
                    return;
                }
            }
            _logger.LogWarning("Faculty {FacultyId} attempted to access unassigned student data", userId);
            return;
        }

        // Parent can access their children's data
        if (requirement.AllowParentAccess && userRole == UserRole.Parent)
        {
            if (userId.HasValue && student.ParentId == userId.Value)
            {
                context.Succeed(requirement);
                return;
            }
            _logger.LogWarning("Parent {ParentId} attempted to access other student data", userId);
            return;
        }

        _logger.LogWarning("User {UserId} with role {UserRole} denied access to student {StudentId}", userId, userRole, student.Id);
    }

    private static UserRole GetUserRole(ClaimsPrincipal user)
    {
        var roleClaim = user.FindFirst(ClaimTypes.Role)?.Value;
        if (Enum.TryParse<UserRole>(roleClaim, out var role))
        {
            return role;
        }
        return UserRole.Student; // Default fallback
    }

    private static Guid? GetUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }
        return null;
    }
}
