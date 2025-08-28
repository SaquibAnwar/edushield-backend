using EduShield.Core.Enums;

namespace EduShield.Core.Dtos;

/// <summary>
/// Request DTO for updating an existing parent
/// </summary>
public class UpdateParentRequest
{
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
    public ParentType ParentType { get; set; } = ParentType.Primary;
    public bool IsEmergencyContact { get; set; } = false;
    public bool IsAuthorizedToPickup { get; set; } = true;
}

