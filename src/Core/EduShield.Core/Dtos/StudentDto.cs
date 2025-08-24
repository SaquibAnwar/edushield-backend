using EduShield.Core.Enums;

namespace EduShield.Core.Dtos;

public class StudentDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Address { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public string RollNumber { get; set; } = string.Empty;
    public DateTime EnrollmentDate { get; set; }
    public StudentStatus Status { get; set; }
    public string? Grade { get; set; }
    public string? Section { get; set; }
    public Guid? UserId { get; set; }
    public Guid? ParentId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Computed properties
    public string FullName => $"{FirstName} {LastName}".Trim();
    public int Age => DateTime.Today.Year - DateOfBirth.Year - (DateTime.Today < DateOfBirth.AddYears(DateTime.Today.Year - DateOfBirth.Year) ? 1 : 0);
    public bool IsEnrolled => Status == StudentStatus.Active;
    
    // Related data
    public List<FacultyDto> AssignedFaculties { get; set; } = [];
    public UserDto? Parent { get; set; }
}

public class FacultyDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public DateTime AssignedDate { get; set; }
    public string FullName => $"{FirstName} {LastName}".Trim();
}
