using EduShield.Core.Enums;

namespace EduShield.Core.Entities;

public class User : AuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? GoogleId { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
    
    // Navigation properties for related entities (will be added when Student and Faculty entities are created)
    // public virtual Student? Student { get; set; }
    // public virtual Faculty? Faculty { get; set; }
}
