using EduShield.Core.Entities;
using EduShield.Core.Dtos;
using EduShield.Core.Enums;

namespace EduShield.Core.Interfaces;

/// <summary>
/// Repository interface for Parent entity operations
/// </summary>
public interface IParentRepository
{
    /// <summary>
    /// Get all parents with optional filtering
    /// </summary>
    Task<IEnumerable<Parent>> GetAllAsync();
    
    /// <summary>
    /// Get parent by ID
    /// </summary>
    Task<Parent?> GetByIdAsync(Guid id);
    
    /// <summary>
    /// Get parent by email
    /// </summary>
    Task<Parent?> GetByEmailAsync(string email);
    
    /// <summary>
    /// Get parent by user ID
    /// </summary>
    Task<Parent?> GetByUserIdAsync(Guid userId);
    
    /// <summary>
    /// Get parents by type
    /// </summary>
    Task<IEnumerable<Parent>> GetByTypeAsync(ParentType parentType);
    
    /// <summary>
    /// Get parents with children
    /// </summary>
    Task<IEnumerable<Parent>> GetWithChildrenAsync();
    
    /// <summary>
    /// Get parent with children by ID
    /// </summary>
    Task<Parent?> GetWithChildrenByIdAsync(Guid id);
    
    /// <summary>
    /// Get parents by city
    /// </summary>
    Task<IEnumerable<Parent>> GetByCityAsync(string city);
    
    /// <summary>
    /// Get parents by state
    /// </summary>
    Task<IEnumerable<Parent>> GetByStateAsync(string state);
    
    /// <summary>
    /// Search parents by name
    /// </summary>
    Task<IEnumerable<Parent>> SearchByNameAsync(string searchTerm);
    
    /// <summary>
    /// Get emergency contacts
    /// </summary>
    Task<IEnumerable<Parent>> GetEmergencyContactsAsync();
    
    /// <summary>
    /// Get parents authorized for pickup
    /// </summary>
    Task<IEnumerable<Parent>> GetAuthorizedForPickupAsync();
    
    /// <summary>
    /// Add new parent
    /// </summary>
    Task<Parent> AddAsync(Parent parent);
    
    /// <summary>
    /// Update existing parent
    /// </summary>
    Task<Parent> UpdateAsync(Parent parent);
    
    /// <summary>
    /// Delete parent
    /// </summary>
    Task<bool> DeleteAsync(Guid id);
    
    /// <summary>
    /// Check if parent exists
    /// </summary>
    Task<bool> ExistsAsync(Guid id);
    
    /// <summary>
    /// Check if email is already in use
    /// </summary>
    Task<bool> EmailExistsAsync(string email, Guid? excludeId = null);
    
    /// <summary>
    /// Get parent statistics
    /// </summary>
    Task<ParentStatistics> GetStatisticsAsync();
}

/// <summary>
/// Parent statistics for reporting
/// </summary>
public class ParentStatistics
{
    public int TotalParents { get; set; }
    public int ActiveParents { get; set; }
    public int PrimaryParents { get; set; }
    public int SecondaryParents { get; set; }
    public int Guardians { get; set; }
    public int EmergencyContacts { get; set; }
    public int AuthorizedForPickup { get; set; }
    public int ParentsWithChildren { get; set; }
    public int AverageChildrenPerParent { get; set; }
    public Dictionary<string, int> ParentsByState { get; set; } = [];
    public Dictionary<string, int> ParentsByCity { get; set; } = [];
}
