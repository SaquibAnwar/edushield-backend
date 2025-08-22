using System.ComponentModel.DataAnnotations;
using EduShield.Core.Enums;

namespace EduShield.Core.Dtos;

public class UpdateStudentRequest
{
    [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
    public string? FirstName { get; set; }
    
    [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
    public string? LastName { get; set; }
    
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    public string? Email { get; set; }
    
    [Phone(ErrorMessage = "Invalid phone number format")]
    [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
    public string? PhoneNumber { get; set; }
    
    public DateTime? DateOfBirth { get; set; }
    
    [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
    public string? Address { get; set; }
    
    public Gender? Gender { get; set; }
    
    public DateTime? EnrollmentDate { get; set; }
    
    [StringLength(20, ErrorMessage = "Grade cannot exceed 20 characters")]
    public string? Grade { get; set; }
    
    [StringLength(10, ErrorMessage = "Section cannot exceed 10 characters")]
    public string? Section { get; set; }
    
    public StudentStatus? Status { get; set; }
    
    public Guid? ParentId { get; set; }
    
    public List<Guid>? FacultyIds { get; set; }
}
