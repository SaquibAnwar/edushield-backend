using EduShield.Core.Enums;

namespace EduShield.Core.Entities;

public class Faculty : AuditableEntity
{
    public Guid Id { get; set; }
    
    // Basic Information
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Address { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    
    // Academic Information
    public string Department { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string? EmployeeId { get; set; }
    public DateTime HireDate { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Relationships
    public Guid? UserId { get; set; } // Authentication relationship
    
    // Navigation properties
    public User? User { get; set; }
    public ICollection<StudentFaculty> StudentFaculties { get; set; } = [];
    
    // Computed properties
    public string FullName => $"{FirstName} {LastName}".Trim();
    public int Age => DateTime.Today.Year - DateOfBirth.Year - (DateTime.Today < DateOfBirth.AddYears(DateTime.Today.Year - DateOfBirth.Year) ? 1 : 0);
    public int YearsOfService => DateTime.Today.Year - HireDate.Year - (DateTime.Today < HireDate.AddYears(DateTime.Today.Year - HireDate.Year) ? 1 : 0);
}
