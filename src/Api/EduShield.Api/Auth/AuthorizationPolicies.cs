using Microsoft.AspNetCore.Authorization;
using EduShield.Core.Enums;

namespace EduShield.Api.Auth;

public static class AuthorizationPolicies
{
    public const string AdminOnly = "AdminOnly";
    public const string StudentOnly = "StudentOnly";
    public const string FacultyOnly = "FacultyOnly";
    public const string ParentOnly = "ParentOnly";
    public const string DevAuthOnly = "DevAuthOnly";
    public const string AdminOrFaculty = "AdminOrFaculty";
    public const string AdminOrStudent = "AdminOrStudent";
    public const string AdminOrParent = "AdminOrParent";
    public const string AuthenticatedUser = "AuthenticatedUser";
}

public static class PolicyBuilderExtensions
{
    public static AuthorizationPolicyBuilder RequireRole(this AuthorizationPolicyBuilder builder, UserRole role)
    {
        return builder.RequireRole(role.ToString());
    }

    public static AuthorizationPolicyBuilder RequireAnyRole(this AuthorizationPolicyBuilder builder, params UserRole[] roles)
    {
        var roleStrings = roles.Select(r => r.ToString()).ToArray();
        return builder.RequireRole(roleStrings);
    }
}
