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
    public Parent? Parent { get; set; } // Keep for backward compatibility
    public ICollection<ParentStudent> ParentStudents { get; set; } = [];
    
    // Academic relationships - Many-to-many with Faculty
    public ICollection<StudentFaculty> StudentFaculties { get; set; } = [];
    
    // Computed properties
    public string FullName => $"{FirstName} {LastName}".Trim();
    public int Age => DateTime.Today.Year - DateOfBirth.Year - (DateTime.Today < DateOfBirth.AddYears(DateTime.Today.Year - DateOfBirth.Year) ? 1 : 0);
    public bool IsEnrolled => Status == StudentStatus.Active;
    
    // Helper methods
    public void AssignFaculty(Guid facultyId, string? notes = null)
    {
        if (!StudentFaculties.Any(sf => sf.FacultyId == facultyId))
        {
            StudentFaculties.Add(new StudentFaculty
            {
                StudentId = Id,
                FacultyId = facultyId,
                AssignedDate = DateTime.UtcNow,
                IsActive = true,
                Notes = notes
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
    
    public void DeactivateFacultyAssignment(Guid facultyId)
    {
        var assignment = StudentFaculties.FirstOrDefault(sf => sf.FacultyId == facultyId);
        if (assignment != null)
        {
            assignment.IsActive = false;
        }
    }
    
    public void ActivateFacultyAssignment(Guid facultyId)
    {
        var assignment = StudentFaculties.FirstOrDefault(sf => sf.FacultyId == facultyId);
        if (assignment != null)
        {
            assignment.IsActive = true;
        }
    }
    
    public bool IsAssignedToFaculty(Guid facultyId)
    {
        return StudentFaculties.Any(sf => sf.FacultyId == facultyId);
    }
    
    // Helper methods for ParentStudent relationships
    public void AssignParent(Guid parentId, string relationship, bool isPrimaryContact = false, string? notes = null)
    {
        if (!ParentStudents.Any(ps => ps.ParentId == parentId))
        {
            ParentStudents.Add(new ParentStudent
            {
                ParentId = parentId,
                StudentId = Id,
                Relationship = relationship,
                IsPrimaryContact = isPrimaryContact,
                IsAuthorizedToPickup = true,
                IsEmergencyContact = true,
                IsActive = true,
                Notes = notes
            });
            
            // Update the legacy ParentId field for backward compatibility (especially for primary contact)
            if (isPrimaryContact || ParentId == null)
            {
                ParentId = parentId;
            }
        }
    }
    
    public void RemoveParent(Guid parentId)
    {
        var relationship = ParentStudents.FirstOrDefault(ps => ps.ParentId == parentId);
        if (relationship != null)
        {
            var wasPrimaryContact = relationship.IsPrimaryContact;
            ParentStudents.Remove(relationship);
            
            // Update the legacy ParentId field for backward compatibility
            if (wasPrimaryContact && ParentId == parentId)
            {
                // Find another active parent to set as primary
                var newPrimaryParent = ParentStudents.FirstOrDefault(ps => ps.IsActive);
                if (newPrimaryParent != null)
                {
                    ParentId = newPrimaryParent.ParentId;
                    newPrimaryParent.IsPrimaryContact = true;
                }
                else
                {
                    ParentId = null;
                }
            }
        }
    }
    
    public bool HasParent(Guid parentId)
    {
        return ParentStudents.Any(ps => ps.ParentId == parentId && ps.IsActive);
    }
    
    public Parent? GetPrimaryParent()
    {
        return ParentStudents.FirstOrDefault(ps => ps.IsPrimaryContact && ps.IsActive)?.Parent;
    }
    
    public IEnumerable<Parent> GetAllParents()
    {
        return ParentStudents.Where(ps => ps.IsActive).Select(ps => ps.Parent).Where(p => p != null);
    }
    
    public int ParentsCount => ParentStudents.Count(ps => ps.IsActive);
}


