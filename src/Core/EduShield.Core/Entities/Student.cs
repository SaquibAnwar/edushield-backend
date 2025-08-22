using EduShield.Core.Enums;

namespace EduShield.Core.Entities;

public class Student : AuditableEntity
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
    public string RollNumber { get; set; } = string.Empty; // Format: student_<monotonically increasing number>
    public DateTime EnrollmentDate { get; set; }
    public StudentStatus Status { get; set; } = StudentStatus.Active;
    public string? Grade { get; set; } // Current grade/class
    public string? Section { get; set; } // Class section
    
    // Relationships
    public Guid? UserId { get; set; } // Authentication relationship
    public Guid? ParentId { get; set; } // Parent relationship
    
    // Navigation properties
    public User? User { get; set; }
    public User? Parent { get; set; }
    
    // Academic relationships - Many-to-many with Faculty
    public ICollection<StudentFaculty> StudentFaculties { get; set; } = [];
    
    // Computed properties
    public string FullName => $"{FirstName} {LastName}".Trim();
    public int Age => DateTime.Today.Year - DateOfBirth.Year - (DateTime.Today < DateOfBirth.AddYears(DateTime.Today.Year - DateOfBirth.Year) ? 1 : 0);
    public bool IsEnrolled => Status == StudentStatus.Active;
    
    // Helper methods
    public void AssignFaculty(Guid facultyId)
    {
        if (!StudentFaculties.Any(sf => sf.FacultyId == facultyId))
        {
            StudentFaculties.Add(new StudentFaculty
            {
                StudentId = Id,
                FacultyId = facultyId,
                AssignedDate = DateTime.UtcNow
            });
        }
    }
    
    public void RemoveFaculty(Guid facultyId)
    {
        var assignment = StudentFaculties.FirstOrDefault(sf => sf.FacultyId == facultyId);
        if (assignment != null)
        {
            StudentFaculties.Remove(assignment);
        }
    }
    
    public bool IsAssignedToFaculty(Guid facultyId)
    {
        return StudentFaculties.Any(sf => sf.FacultyId == facultyId);
    }
}

// Many-to-many relationship between Student and Faculty
public class StudentFaculty : AuditableEntity
{
    public Guid StudentId { get; set; }
    public Guid FacultyId { get; set; }
    public DateTime AssignedDate { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public Student? Student { get; set; }
    public Faculty? Faculty { get; set; }
}
