using Microsoft.AspNetCore.Authorization;

namespace EduShield.Api.Auth.Requirements;

/// <summary>
/// Authorization requirement for student performance access control
/// </summary>
public class StudentPerformanceAccessRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Whether to allow admin access
    /// </summary>
    public bool AllowAdminAccess { get; set; } = true;

    /// <summary>
    /// Whether to allow dev auth access
    /// </summary>
    public bool AllowDevAccess { get; set; } = true;

    /// <summary>
    /// Whether to allow faculty access to their assigned students
    /// </summary>
    public bool AllowFacultyAccess { get; set; } = true;

    /// <summary>
    /// Whether to allow students to access their own performance
    /// </summary>
    public bool AllowSelfAccess { get; set; } = true;

    /// <summary>
    /// Whether to allow parents to access their children's performance
    /// </summary>
    public bool AllowParentAccess { get; set; } = true;

    /// <summary>
    /// Whether the access is read-only
    /// </summary>
    public bool ReadOnly { get; set; } = false;

    /// <summary>
    /// Whether to allow create/update operations
    /// </summary>
    public bool AllowModify { get; set; } = true;
}
