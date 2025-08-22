using System.ComponentModel.DataAnnotations;
using EduShield.Core.Enums;

namespace EduShield.Core.Dtos;

public class CreateStudentRequest
{
    [Required(ErrorMessage = "First name is required")]
    [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
    public string FirstName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Last name is required")]
    [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
    public string LastName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Phone number is required")]
    [Phone(ErrorMessage = "Invalid phone number format")]
    [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
    public string PhoneNumber { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Date of birth is required")]
    public DateTime DateOfBirth { get; set; }
    
    [Required(ErrorMessage = "Address is required")]
    [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
    public string Address { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Gender is required")]
    public Gender Gender { get; set; }
    
    [Required(ErrorMessage = "Enrollment date is required")]
    public DateTime EnrollmentDate { get; set; }
    
    [StringLength(20, ErrorMessage = "Grade cannot exceed 20 characters")]
    public string? Grade { get; set; }
    
    [StringLength(10, ErrorMessage = "Section cannot exceed 10 characters")]
    public string? Section { get; set; }
    
    public Guid? ParentId { get; set; }
    
    public List<Guid> FacultyIds { get; set; } = [];
}
