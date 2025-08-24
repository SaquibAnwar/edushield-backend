using EduShield.Core.Enums;

namespace EduShield.Core.Dtos;

/// <summary>
/// Data transfer object for faculty information
/// </summary>
public class FacultyDto
{
    /// <summary>
    /// Unique identifier for the faculty
    /// </summary>
    /// <example>443abd4f-9e56-4adc-9eb7-7a0e2522dd2b</example>
    public Guid Id { get; set; }

    /// <summary>
    /// Faculty's first name
    /// </summary>
    /// <example>John</example>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Faculty's last name
    /// </summary>
    /// <example>Doe</example>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Faculty's email address
    /// </summary>
    /// <example>john.doe@university.edu</example>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Faculty's phone number
    /// </summary>
    /// <example>+1-555-123-4567</example>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Faculty's date of birth
    /// </summary>
    /// <example>1985-03-15</example>
    public DateTime DateOfBirth { get; set; }

    /// <summary>
    /// Faculty's address
    /// </summary>
    /// <example>123 University Ave, City, State 12345</example>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Faculty's gender
    /// </summary>
    /// <example>Male</example>
    public Gender Gender { get; set; }

    /// <summary>
    /// Faculty's department
    /// </summary>
    /// <example>Computer Science</example>
    public string Department { get; set; } = string.Empty;

    /// <summary>
    /// Faculty's subject/specialty
    /// </summary>
    /// <example>Software Engineering</example>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Faculty's employee ID
    /// </summary>
    /// <example>faculty_001</example>
    public string? EmployeeId { get; set; }

    /// <summary>
    /// Faculty's hire date
    /// </summary>
    /// <example>2020-08-15</example>
    public DateTime HireDate { get; set; }

    /// <summary>
    /// Indicates whether the faculty member is active
    /// </summary>
    /// <example>true</example>
    public bool IsActive { get; set; }

    /// <summary>
    /// Associated user account ID for authentication
    /// </summary>
    /// <example>443abd4f-9e56-4adc-9eb7-7a0e2522dd2b</example>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Date and time when the faculty record was created
    /// </summary>
    /// <example>2025-08-21T15:36:32.965405Z</example>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Date and time when the faculty record was last updated
    /// </summary>
    /// <example>2025-08-21T15:36:32.965405Z</example>
    public DateTime UpdatedAt { get; set; }

    // Computed properties
    /// <summary>
    /// Faculty's full name (computed from FirstName and LastName)
    /// </summary>
    /// <example>John Doe</example>
    public string FullName => $"{FirstName} {LastName}".Trim();

    /// <summary>
    /// Faculty's age (computed from DateOfBirth)
    /// </summary>
    /// <example>38</example>
    public int Age => DateTime.Today.Year - DateOfBirth.Year - (DateTime.Today < DateOfBirth.AddYears(DateTime.Today.Year - DateOfBirth.Year) ? 1 : 0);

    /// <summary>
    /// Faculty's years of service (computed from HireDate)
    /// </summary>
    /// <example>3</example>
    public int YearsOfService => DateTime.Today.Year - HireDate.Year - (DateTime.Today < HireDate.AddYears(DateTime.Today.Year - HireDate.Year) ? 1 : 0);

    /// <summary>
    /// Indicates whether the faculty member is currently employed
    /// </summary>
    /// <example>true</example>
    public bool IsEmployed => IsActive && HireDate <= DateTime.Today;
}
