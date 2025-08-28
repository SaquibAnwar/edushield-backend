using Microsoft.AspNetCore.Authorization;

namespace EduShield.Api.Auth.Requirements;

/// <summary>
/// Authorization requirement for student fee access control
/// </summary>
public class StudentFeeAccessRequirement : IAuthorizationRequirement
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
    /// Whether to allow faculty access to their assigned students' fees
    /// </summary>
    public bool AllowFacultyAccess { get; set; } = true;

    /// <summary>
    /// Whether to allow students to access their own fees
    /// </summary>
    public bool AllowSelfAccess { get; set; } = true;

    /// <summary>
    /// Whether to allow parents to access their children's fees
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

    /// <summary>
    /// Whether to allow payment operations
    /// </summary>
    public bool AllowPayment { get; set; } = true;
}