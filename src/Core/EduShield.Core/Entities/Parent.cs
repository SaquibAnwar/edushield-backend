using EduShield.Core.Enums;

namespace EduShield.Core.Entities;

/// <summary>
/// Represents a parent entity in the EduShield system
/// </summary>
public class Parent : AuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    // Basic Information
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? AlternatePhoneNumber { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Address { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; } = "USA";
    public Gender Gender { get; set; }
    
    // Professional Information
    public string? Occupation { get; set; }
    public string? Employer { get; set; }
    public string? WorkPhone { get; set; }
    
    // Emergency Contact Information
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? EmergencyContactRelationship { get; set; }
    
    // Relationship Information
    public Guid? UserId { get; set; } // Authentication relationship
    public ParentType ParentType { get; set; } = ParentType.Primary;
    public bool IsEmergencyContact { get; set; } = false;
    public bool IsAuthorizedToPickup { get; set; } = true;
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public User? User { get; set; }
    public ICollection<Student> Children { get; set; } = []; // Keep for backward compatibility
    public ICollection<ParentStudent> ParentStudents { get; set; } = [];
    
    // Computed properties
    public string FullName => $"{FirstName} {LastName}".Trim();
    public int Age => DateTime.Today.Year - DateOfBirth.Year - (DateTime.Today < DateOfBirth.AddYears(DateTime.Today.Year - DateOfBirth.Year) ? 1 : 0);
    public string FullAddress => $"{Address}, {City}, {State} {PostalCode}, {Country}".Replace(", ,", ",").Replace("  ", " ").Trim(',', ' ');
    
    // Helper methods for ParentStudent relationships
    public void AssignStudent(Guid studentId, string relationship, bool isPrimaryContact = false, string? notes = null)
    {
        if (!ParentStudents.Any(ps => ps.StudentId == studentId))
        {
            ParentStudents.Add(new ParentStudent
            {
                ParentId = Id,
                StudentId = studentId,
                Relationship = relationship,
                IsPrimaryContact = isPrimaryContact,
                IsAuthorizedToPickup = true,
                IsEmergencyContact = true,
                IsActive = true,
                Notes = notes
            });
        }
    }
    
    public void RemoveStudent(Guid studentId)
    {
        var relationship = ParentStudents.FirstOrDefault(ps => ps.StudentId == studentId);
        if (relationship != null)
        {
            ParentStudents.Remove(relationship);
        }
    }
    
    public void DeactivateStudentRelationship(Guid studentId)
    {
        var relationship = ParentStudents.FirstOrDefault(ps => ps.StudentId == studentId);
        if (relationship != null)
        {
            relationship.DeactivateRelationship();
        }
    }
    
    public void ActivateStudentRelationship(Guid studentId)
    {
        var relationship = ParentStudents.FirstOrDefault(ps => ps.StudentId == studentId);
        if (relationship != null)
        {
            relationship.ActivateRelationship();
        }
    }
    
    public bool HasStudent(Guid studentId)
    {
        return ParentStudents.Any(ps => ps.StudentId == studentId && ps.IsActive);
    }
    
    public bool IsPrimaryContactFor(Guid studentId)
    {
        return ParentStudents.Any(ps => ps.StudentId == studentId && ps.IsPrimaryContact && ps.IsActive);
    }
    
    // Legacy methods for backward compatibility
    public void AddChild(Student child)
    {
        if (!Children.Any(c => c.Id == child.Id))
        {
            child.ParentId = Id;
            Children.Add(child);
            
            // Also create the ParentStudent relationship for consistency
            AssignStudent(child.Id, "Parent", true, "Legacy assignment");
        }
    }
    
    public void RemoveChild(Guid childId)
    {
        var child = Children.FirstOrDefault(c => c.Id == childId);
        if (child != null)
        {
            child.ParentId = null;
            Children.Remove(child);
            
            // Also remove the ParentStudent relationship for consistency
            RemoveStudent(childId);
        }
    }
    
    public bool HasChild(Guid childId)
    {
        return Children.Any(c => c.Id == childId) || HasStudent(childId);
    }
    
    public int ChildrenCount => ParentStudents.Count(ps => ps.IsActive);
    
    public bool IsPrimaryParent => ParentType == ParentType.Primary;
    
    public bool CanPickupChild => IsAuthorizedToPickup;
}
