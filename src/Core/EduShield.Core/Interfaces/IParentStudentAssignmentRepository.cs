using EduShield.Core.Entities;

namespace EduShield.Core.Interfaces;

/// <summary>
/// Repository interface for managing parent-student assignments
/// </summary>
public interface IParentStudentAssignmentRepository
{
    // Basic CRUD operations
    Task<ParentStudent?> GetByIdAsync(Guid parentId, Guid studentId);
    Task<IEnumerable<ParentStudent>> GetAllAsync();
    Task<ParentStudent> CreateAsync(ParentStudent assignment);
    Task<ParentStudent> UpdateAsync(ParentStudent assignment);
    Task<bool> DeleteAsync(Guid parentId, Guid studentId);
    Task<bool> ExistsAsync(Guid parentId, Guid studentId);
    
    // Parent-specific queries
    Task<IEnumerable<ParentStudent>> GetByParentIdAsync(Guid parentId);
    Task<IEnumerable<ParentStudent>> GetActiveByParentIdAsync(Guid parentId);
    Task<int> GetStudentCountByParentIdAsync(Guid parentId);
    Task<bool> IsParentAssignedToStudentAsync(Guid parentId, Guid studentId);
    
    // Student-specific queries
    Task<IEnumerable<ParentStudent>> GetByStudentIdAsync(Guid studentId);
    Task<IEnumerable<ParentStudent>> GetActiveByStudentIdAsync(Guid studentId);
    Task<int> GetParentCountByStudentIdAsync(Guid studentId);
    Task<ParentStudent?> GetPrimaryParentByStudentIdAsync(Guid studentId);
    Task<bool> IsStudentAssignedToParentAsync(Guid studentId, Guid parentId);
    
    // Relationship management
    Task<bool> SetPrimaryContactAsync(Guid parentId, Guid studentId);
    Task<bool> RemovePrimaryContactAsync(Guid parentId, Guid studentId);
    Task<bool> ActivateAssignmentAsync(Guid parentId, Guid studentId);
    Task<bool> DeactivateAssignmentAsync(Guid parentId, Guid studentId);
    
    // Bulk operations
    Task<IEnumerable<ParentStudent>> CreateBulkAsync(IEnumerable<ParentStudent> assignments);
    Task<bool> DeleteBulkByParentIdAsync(Guid parentId);
    Task<bool> DeleteBulkByStudentIdAsync(Guid studentId);
    
    // Validation queries
    Task<bool> CanAssignParentToStudentAsync(Guid parentId, Guid studentId);
    Task<IEnumerable<ParentStudent>> GetConflictingAssignmentsAsync(Guid parentId, Guid studentId);
    
    // Statistics and reporting
    Task<int> GetTotalAssignmentsCountAsync();
    Task<int> GetActiveAssignmentsCountAsync();
    Task<Dictionary<string, int>> GetAssignmentsByRelationshipTypeAsync();
    Task<IEnumerable<ParentStudent>> GetOrphanedStudentsAsync(); // Students without any parent assignments
    Task<IEnumerable<ParentStudent>> GetParentsWithoutStudentsAsync(); // Parents without any student assignments
}