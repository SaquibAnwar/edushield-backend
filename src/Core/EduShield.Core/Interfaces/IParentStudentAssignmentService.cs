using EduShield.Core.Dtos;

namespace EduShield.Core.Interfaces;

/// <summary>
/// Service interface for managing parent-student assignments
/// </summary>
public interface IParentStudentAssignmentService
{
    // Basic CRUD operations
    Task<ParentStudentAssignmentDto?> GetAssignmentAsync(Guid parentId, Guid studentId);
    Task<IEnumerable<ParentStudentAssignmentDto>> GetAllAssignmentsAsync();
    Task<ParentStudentAssignmentDto> CreateAssignmentAsync(CreateParentStudentAssignmentDto createDto);
    Task<ParentStudentAssignmentDto> UpdateAssignmentAsync(Guid parentId, Guid studentId, UpdateParentStudentAssignmentDto updateDto);
    Task<bool> DeleteAssignmentAsync(Guid parentId, Guid studentId);
    
    // Parent-specific operations
    Task<ParentWithStudentsDto?> GetParentWithStudentsAsync(Guid parentId);
    Task<IEnumerable<ParentStudentAssignmentDto>> GetAssignmentsByParentIdAsync(Guid parentId);
    Task<IEnumerable<ParentStudentAssignmentDto>> GetActiveAssignmentsByParentIdAsync(Guid parentId);
    Task<int> GetStudentCountByParentIdAsync(Guid parentId);
    
    // Student-specific operations
    Task<StudentWithParentsDto?> GetStudentWithParentsAsync(Guid studentId);
    Task<IEnumerable<ParentStudentAssignmentDto>> GetAssignmentsByStudentIdAsync(Guid studentId);
    Task<IEnumerable<ParentStudentAssignmentDto>> GetActiveAssignmentsByStudentIdAsync(Guid studentId);
    Task<ParentStudentAssignmentDto?> GetPrimaryParentByStudentIdAsync(Guid studentId);
    Task<int> GetParentCountByStudentIdAsync(Guid studentId);
    
    // Relationship management
    Task<bool> SetPrimaryContactAsync(Guid parentId, Guid studentId);
    Task<bool> RemovePrimaryContactAsync(Guid parentId, Guid studentId);
    Task<bool> ActivateAssignmentAsync(Guid parentId, Guid studentId);
    Task<bool> DeactivateAssignmentAsync(Guid parentId, Guid studentId);
    
    // Bulk operations
    Task<IEnumerable<ParentStudentAssignmentDto>> CreateBulkAssignmentsAsync(BulkParentStudentAssignmentDto bulkDto);
    Task<bool> DeleteAllAssignmentsByParentIdAsync(Guid parentId);
    Task<bool> DeleteAllAssignmentsByStudentIdAsync(Guid studentId);
    
    // Validation
    Task<bool> CanAssignParentToStudentAsync(Guid parentId, Guid studentId);
    Task<bool> IsParentAssignedToStudentAsync(Guid parentId, Guid studentId);
    Task<bool> IsStudentAssignedToParentAsync(Guid studentId, Guid parentId);
    
    // Statistics and reporting
    Task<int> GetTotalAssignmentsCountAsync();
    Task<int> GetActiveAssignmentsCountAsync();
    Task<Dictionary<string, int>> GetAssignmentsByRelationshipTypeAsync();
    Task<IEnumerable<StudentWithParentsDto>> GetOrphanedStudentsAsync();
    Task<IEnumerable<ParentWithStudentsDto>> GetParentsWithoutStudentsAsync();
    
    // Data consistency
    Task<int> SyncLegacyParentStudentDataAsync();
}