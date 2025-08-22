using Microsoft.AspNetCore.Authorization;

namespace EduShield.Api.Auth.Requirements;

public class StudentAccessRequirement : IAuthorizationRequirement
{
    public bool AllowSelfAccess { get; set; } = true;
    public bool AllowAdminAccess { get; set; } = true;
    public bool AllowDevAccess { get; set; } = true;
    public bool AllowFacultyAccess { get; set; } = true;
    public bool AllowParentAccess { get; set; } = true;
    public bool ReadOnly { get; set; } = false;
}
