namespace EduShield.Core.Enums;

/// <summary>
/// Defines the different user roles in the EduShield system
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Administrator role with full system access
    /// </summary>
    /// <remarks>
    /// Admins can access all endpoints and manage the entire system.
    /// They have the highest level of privileges.
    /// </remarks>
    Admin = 1,

    /// <summary>
    /// Student role with access to student-specific features
    /// </summary>
    /// <remarks>
    /// Students can access courses, view grades, and use student-specific functionality.
    /// Access is limited to their own academic information.
    /// </remarks>
    Student = 2,

    /// <summary>
    /// Faculty role with access to teaching and academic management features
    /// </summary>
    /// <remarks>
    /// Faculty can manage courses, grades, and academic content.
    /// They have access to student information for their courses.
    /// </remarks>
    Faculty = 3,

    /// <summary>
    /// Parent role with access to child monitoring features
    /// </summary>
    /// <remarks>
    /// Parents can monitor their children's academic progress and manage fees.
    /// Access is limited to their children's information.
    /// </remarks>
    Parent = 4,

    /// <summary>
    /// Development authentication role for testing purposes
    /// </summary>
    /// <remarks>
    /// DevAuth is a special role for development and testing.
    /// It bypasses normal authentication for development purposes.
    /// This role should not be used in production.
    /// </remarks>
    DevAuth = 5
}
