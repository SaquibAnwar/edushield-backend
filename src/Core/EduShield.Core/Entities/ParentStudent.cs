using EduShield.Core.Enums;

namespace EduShield.Core.Entities;

/// <summary>
/// Represents the many-to-many relationship between Parent and Student entities
/// </summary>
public class ParentStudent : AuditableEntity
{
    public Guid ParentId { get; set; }
    public Guid StudentId { get; set; }
    
    /// <summary>
    /// Relationship type (Father, Mother, Guardian, etc.)
    /// </summary>
    public string Relationship { get; set; } = string.Empty;
    
    /// <summary>
    /// Indicates if this parent is the primary contact for the student
    /// </summary>
    public bool IsPrimaryContact { get; set; } = false;
    
    /// <summary>
    /// Indicates if this parent is authorized to pick up the student
    /// </summary>
    public bool IsAuthorizedToPickup { get; set; } = true;
    
    /// <summary>
    /// Indicates if this parent should receive emergency notifications
    /// </summary>
    public bool IsEmergencyContact { get; set; } = true;
    
    /// <summary>
    /// Indicates if this relationship is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Optional notes about this parent-student relationship
    /// </summary>
    public string? Notes { get; set; }
    
    // Navigation properties
    public Parent Parent { get; set; } = null!;
    public Student Student { get; set; } = null!;
    
    // Computed properties
    public string RelationshipDescription => $"{Parent?.FullName} is {Relationship} of {Student?.FullName}";
    
    // Helper methods
    public void MakePrimaryContact()
    {
        IsPrimaryContact = true;
        IsEmergencyContact = true;
        IsAuthorizedToPickup = true;
    }
    
    public void RemovePrimaryContact()
    {
        IsPrimaryContact = false;
    }
    
    public void ActivateRelationship()
    {
        IsActive = true;
    }
    
    public void DeactivateRelationship()
    {
        IsActive = false;
    }
    
    public bool CanPickupStudent => IsActive && IsAuthorizedToPickup;
    public bool CanReceiveEmergencyNotifications => IsActive && IsEmergencyContact;
    public bool IsPrimary => IsActive && IsPrimaryContact;
}