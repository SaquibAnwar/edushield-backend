using System.ComponentModel.DataAnnotations;
using EduShield.Core.Enums;

namespace EduShield.Core.Dtos;

/// <summary>
/// Request model for updating an existing faculty member
/// </summary>
public class UpdateFacultyRequest
{
    /// <summary>
    /// Faculty's first name
    /// </summary>
    /// <example>John</example>
    [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
    public string? FirstName { get; set; }
    
    /// <summary>
    /// Faculty's last name
    /// </summary>
    /// <example>Doe</example>
    [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
    public string? LastName { get; set; }
    
    /// <summary>
    /// Faculty's email address
    /// </summary>
    /// <example>john.doe@university.edu</example>
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    public string? Email { get; set; }
    
    /// <summary>
    /// Faculty's phone number
    /// </summary>
    /// <example>+1-555-123-4567</example>
    [Phone(ErrorMessage = "Invalid phone number format")]
    [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
    public string? PhoneNumber { get; set; }
    
    /// <summary>
    /// Faculty's date of birth
    /// </summary>
    /// <example>1985-03-15</example>
    public DateTime? DateOfBirth { get; set; }
    
    /// <summary>
    /// Faculty's address
    /// </summary>
    /// <example>123 University Ave, City, State 12345</example>
    [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
    public string? Address { get; set; }
    
    /// <summary>
    /// Faculty's gender
    /// </summary>
    /// <example>Male</example>
    public Gender? Gender { get; set; }
    
    /// <summary>
    /// Faculty's department
    /// </summary>
    /// <example>Computer Science</example>
    [StringLength(100, ErrorMessage = "Department cannot exceed 100 characters")]
    public string? Department { get; set; }
    
    /// <summary>
    /// Faculty's subject/specialty
    /// </summary>
    /// <example>Software Engineering</example>
    [StringLength(100, ErrorMessage = "Subject cannot exceed 100 characters")]
    public string? Subject { get; set; }
    
    /// <summary>
    /// Faculty's hire date
    /// </summary>
    /// <example>2020-08-15</example>
    public DateTime? HireDate { get; set; }
    
    /// <summary>
    /// Indicates whether the faculty member is active
    /// </summary>
    /// <example>true</example>
    public bool? IsActive { get; set; }
    
    /// <summary>
    /// Associated user account ID for authentication
    /// </summary>
    /// <example>443abd4f-9e56-4adc-9eb7-7a0e2522dd2b</example>
    public Guid? UserId { get; set; }
}
