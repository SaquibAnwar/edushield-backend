using EduShield.Api.Auth.Requirements;
using EduShield.Core.Dtos;
using EduShield.Core.Entities;
using EduShield.Core.Enums;
using EduShield.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace EduShield.Api.Auth.Handlers;

/// <summary>
/// Authorization handler for student performance access control
/// </summary>
public class StudentPerformanceAuthorizationHandler : AuthorizationHandler<StudentPerformanceAccessRequirement, StudentPerformanceDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ILogger<StudentPerformanceAuthorizationHandler> _logger;

    public StudentPerformanceAuthorizationHandler(
        IUserRepository userRepository,
        IStudentRepository studentRepository,
        ILogger<StudentPerformanceAuthorizationHandler> logger)
    {
        _userRepository = userRepository;
        _studentRepository = studentRepository;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        StudentPerformanceAccessRequirement requirement,
        StudentPerformanceDto performance)
    {
        var user = context.User;
        if (!user.Identity?.IsAuthenticated == true)
        {
            _logger.LogWarning("Unauthenticated user attempted to access student performance data");
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

        // Student can only access their own performance data
        if (requirement.AllowSelfAccess && userRole == UserRole.Student)
        {
            if (userId.HasValue && performance.StudentId == userId.Value)
            {
                context.Succeed(requirement);
                return;
            }
            _logger.LogWarning("Student {StudentId} attempted to access other student's performance data", userId);
            return;
        }

        // Faculty can access performance of students assigned to them
        if (requirement.AllowFacultyAccess && userRole == UserRole.Faculty)
        {
            if (userId.HasValue)
            {
                var faculty = await _userRepository.GetByIdAsync(userId.Value);
                if (faculty != null)
                {
                    // Check if the faculty is assigned to the student
                    var student = await _studentRepository.GetByIdAsync(performance.StudentId);
                    if (student != null && student.StudentFaculties.Any(sf => sf.FacultyId == faculty.Id && sf.IsActive))
                    {
                        context.Succeed(requirement);
                        return;
                    }
                }
            }
            _logger.LogWarning("Faculty {FacultyId} attempted to access unassigned student's performance data", userId);
            return;
        }

        // Parent can access their children's performance data
        if (requirement.AllowParentAccess && userRole == UserRole.Parent)
        {
            if (userId.HasValue)
            {
                var student = await _studentRepository.GetByIdAsync(performance.StudentId);
                if (student != null && student.ParentId == userId.Value)
                {
                    context.Succeed(requirement);
                    return;
                }
            }
            _logger.LogWarning("Parent {ParentId} attempted to access other student's performance data", userId);
            return;
        }

        _logger.LogWarning("User {UserId} with role {UserRole} denied access to student performance {PerformanceId}", userId, userRole, performance.Id);
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
