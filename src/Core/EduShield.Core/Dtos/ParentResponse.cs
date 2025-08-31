using EduShield.Core.Enums;

namespace EduShield.Core.Dtos;

/// <summary>
/// Response DTO for parent data
/// </summary>
public class ParentResponse
{
    public Guid Id { get; set; }
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
    public string? Occupation { get; set; }
    public string? Employer { get; set; }
    public string? WorkPhone { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? EmergencyContactRelationship { get; set; }
    public ParentType ParentType { get; set; }
    public bool IsEmergencyContact { get; set; }
    public bool IsAuthorizedToPickup { get; set; }
    public Guid? UserId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Computed properties
    public string FullName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string FullAddress { get; set; } = string.Empty;
    public int ChildrenCount { get; set; }
    public bool IsPrimaryParent { get; set; }
    
    // Children information (for parent portal)
    public List<ParentChildInfo> Children { get; set; } = [];
}

/// <summary>
/// Simplified child information for parent responses
/// </summary>
public class ParentChildInfo
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string RollNumber { get; set; } = string.Empty;
    public string? Grade { get; set; }
    public string? Section { get; set; }
    public StudentStatus Status { get; set; }
    public DateTime EnrollmentDate { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int Age { get; set; }
    public bool IsEnrolled { get; set; }
}


