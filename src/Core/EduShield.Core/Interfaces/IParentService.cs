using EduShield.Core.Dtos;
using EduShield.Core.Entities;
using EduShield.Core.Enums;

namespace EduShield.Core.Interfaces;

/// <summary>
/// Service interface for Parent business logic operations
/// </summary>
public interface IParentService
{
    /// <summary>
    /// Get all parents
    /// </summary>
    Task<IEnumerable<ParentResponse>> GetAllAsync();
    
    /// <summary>
    /// Get parent by ID
    /// </summary>
    Task<ParentResponse?> GetByIdAsync(Guid id);
    
    /// <summary>
    /// Get parent by email
    /// </summary>
    Task<ParentResponse?> GetByEmailAsync(string email);
    
    /// <summary>
    /// Get parent by user ID
    /// </summary>
    Task<ParentResponse?> GetByUserIdAsync(Guid userId);
    
    /// <summary>
    /// Get parent with children by ID
    /// </summary>
    Task<ParentResponse?> GetWithChildrenByIdAsync(Guid id);
    
    /// <summary>
    /// Get parents by type
    /// </summary>
    Task<IEnumerable<ParentResponse>> GetByTypeAsync(ParentType parentType);
    
    /// <summary>
    /// Get parents by city
    /// </summary>
    Task<IEnumerable<ParentResponse>> GetByCityAsync(string city);
    
    /// <summary>
    /// Get parents by state
    /// </summary>
    Task<IEnumerable<ParentResponse>> GetByStateAsync(string state);
    
    /// <summary>
    /// Search parents by name
    /// </summary>
    Task<IEnumerable<ParentResponse>> SearchByNameAsync(string searchTerm);
    
    /// <summary>
    /// Get emergency contacts
    /// </summary>
    Task<IEnumerable<ParentResponse>> GetEmergencyContactsAsync();
    
    /// <summary>
    /// Get parents authorized for pickup
    /// </summary>
    Task<IEnumerable<ParentResponse>> GetAuthorizedForPickupAsync();
    
    /// <summary>
    /// Create new parent
    /// </summary>
    Task<ParentResponse> CreateAsync(CreateParentRequest request);
    
    /// <summary>
    /// Update existing parent
    /// </summary>
    Task<ParentResponse> UpdateAsync(Guid id, UpdateParentRequest request);
    
    /// <summary>
    /// Delete parent
    /// </summary>
    Task<bool> DeleteAsync(Guid id);
    
    /// <summary>
    /// Add child to parent
    /// </summary>
    Task<bool> AddChildAsync(Guid parentId, Guid childId);
    
    /// <summary>
    /// Remove child from parent
    /// </summary>
    Task<bool> RemoveChildAsync(Guid parentId, Guid childId);
    
    /// <summary>
    /// Get parent statistics
    /// </summary>
    Task<ParentStatistics> GetStatisticsAsync();
    
    /// <summary>
    /// Validate parent data
    /// </summary>
    Task<(bool IsValid, List<string> Errors)> ValidateAsync(CreateParentRequest request);
    
    /// <summary>
    /// Validate parent update data
    /// </summary>
    Task<(bool IsValid, List<string> Errors)> ValidateUpdateAsync(Guid id, UpdateParentRequest request);
}
