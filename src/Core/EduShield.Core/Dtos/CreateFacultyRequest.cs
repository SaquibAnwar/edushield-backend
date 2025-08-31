using System.ComponentModel.DataAnnotations;
using EduShield.Core.Enums;

namespace EduShield.Core.Dtos;

/// <summary>
/// Request model for creating a new faculty member
/// </summary>
public class CreateFacultyRequest
{
    /// <summary>
    /// Faculty's first name
    /// </summary>
    /// <example>John</example>
    [Required(ErrorMessage = "First name is required")]
    [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
    public string FirstName { get; set; } = string.Empty;
    
    /// <summary>
    /// Faculty's last name
    /// </summary>
    /// <example>Doe</example>
    [Required(ErrorMessage = "Last name is required")]
    [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
    public string LastName { get; set; } = string.Empty;
    
    /// <summary>
    /// Faculty's email address
    /// </summary>
    /// <example>john.doe@university.edu</example>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Faculty's phone number
    /// </summary>
    /// <example>+1-555-123-4567</example>
    [Required(ErrorMessage = "Phone number is required")]
    [Phone(ErrorMessage = "Invalid phone number format")]
    [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
    public string PhoneNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Faculty's date of birth
    /// </summary>
    /// <example>1985-03-15</example>
    [Required(ErrorMessage = "Date of birth is required")]
    public DateTime DateOfBirth { get; set; }
    
    /// <summary>
    /// Faculty's address
    /// </summary>
    /// <example>123 University Ave, City, State 12345</example>
    [Required(ErrorMessage = "Address is required")]
    [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
    public string Address { get; set; } = string.Empty;
    
    /// <summary>
    /// Faculty's gender
    /// </summary>
    /// <example>Male</example>
    [Required(ErrorMessage = "Gender is required")]
    public Gender Gender { get; set; }
    
    /// <summary>
    /// Faculty's department
    /// </summary>
    /// <example>Computer Science</example>
    [Required(ErrorMessage = "Department is required")]
    [StringLength(100, ErrorMessage = "Department cannot exceed 100 characters")]
    public string Department { get; set; } = string.Empty;
    
    /// <summary>
    /// Faculty's subject/specialty
    /// </summary>
    /// <example>Software Engineering</example>
    [Required(ErrorMessage = "Subject is required")]
    [StringLength(100, ErrorMessage = "Subject cannot exceed 100 characters")]
    public string Subject { get; set; } = string.Empty;
    
    /// <summary>
    /// Faculty's hire date
    /// </summary>
    /// <example>2020-08-15</example>
    [Required(ErrorMessage = "Hire date is required")]
    public DateTime HireDate { get; set; }
}
