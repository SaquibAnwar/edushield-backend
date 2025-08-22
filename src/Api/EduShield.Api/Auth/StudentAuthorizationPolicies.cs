using Microsoft.AspNetCore.Authorization;

namespace EduShield.Api.Auth;

public static class StudentAuthorizationPolicies
{
    public const string AdminOrDevOnly = "AdminOrDevOnly";
    public const string StudentAccess = "StudentAccess";
    public const string FacultyAccess = "FacultyAccess";
    public const string ParentAccess = "ParentAccess";
    public const string ReadOnlyAccess = "ReadOnlyAccess";
}
