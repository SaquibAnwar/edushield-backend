namespace EduShield.Api.Auth;

/// <summary>
/// Constants for authorization policy names
/// </summary>
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