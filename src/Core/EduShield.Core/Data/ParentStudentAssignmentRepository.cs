using EduShield.Core.Entities;
using EduShield.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EduShield.Core.Data;

/// <summary>
/// Repository implementation for managing parent-student assignments
/// </summary>
public class ParentStudentAssignmentRepository : IParentStudentAssignmentRepository
{
    private readonly EduShieldDbContext _context;
    private readonly ILogger<ParentStudentAssignmentRepository> _logger;

    public ParentStudentAssignmentRepository(EduShieldDbContext context, ILogger<ParentStudentAssignmentRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    // Basic CRUD operations
    public async Task<ParentStudent?> GetByIdAsync(Guid parentId, Guid studentId)
    {
        try
        {
            return await _context.ParentStudents
                .Include(ps => ps.Parent)
                .Include(ps => ps.Student)
                .FirstOrDefaultAsync(ps => ps.ParentId == parentId && ps.StudentId == studentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving parent-student assignment for ParentId: {ParentId}, StudentId: {StudentId}", parentId, studentId);
            throw;
        }
    }

    public async Task<IEnumerable<ParentStudent>> GetAllAsync()
    {
        try
        {
            return await _context.ParentStudents
                .Include(ps => ps.Parent)
                .Include(ps => ps.Student)
                .OrderBy(ps => ps.Parent.LastName)
                .ThenBy(ps => ps.Parent.FirstName)
                .ThenBy(ps => ps.Student.LastName)
                .ThenBy(ps => ps.Student.FirstName)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all parent-student assignments");
            throw;
        }
    }

    public async Task<ParentStudent> CreateAsync(ParentStudent assignment)
    {
        try
        {
            // Validate that parent and student exist
            var parentExists = await _context.Parents.AnyAsync(p => p.Id == assignment.ParentId);
            var studentExists = await _context.Students.AnyAsync(s => s.Id == assignment.StudentId);

            if (!parentExists)
                throw new ArgumentException($"Parent with ID {assignment.ParentId} does not exist");
            
            if (!studentExists)
                throw new ArgumentException($"Student with ID {assignment.StudentId} does not exist");

            // Check if assignment already exists
            var existingAssignment = await GetByIdAsync(assignment.ParentId, assignment.StudentId);
            if (existingAssignment != null)
                throw new InvalidOperationException($"Assignment already exists between Parent {assignment.ParentId} and Student {assignment.StudentId}");

            assignment.CreatedAt = DateTime.UtcNow;
            assignment.UpdatedAt = DateTime.UtcNow;

            _context.ParentStudents.Add(assignment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created parent-student assignment: ParentId {ParentId}, StudentId {StudentId}, Relationship {Relationship}", 
                assignment.ParentId, assignment.StudentId, assignment.Relationship);

            return assignment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating parent-student assignment for ParentId: {ParentId}, StudentId: {StudentId}", 
                assignment.ParentId, assignment.StudentId);
            throw;
        }
    }

    public async Task<ParentStudent> UpdateAsync(ParentStudent assignment)
    {
        try
        {
            var existingAssignment = await GetByIdAsync(assignment.ParentId, assignment.StudentId);
            if (existingAssignment == null)
                throw new ArgumentException($"Assignment not found for ParentId: {assignment.ParentId}, StudentId: {assignment.StudentId}");

            // Update properties
            existingAssignment.Relationship = assignment.Relationship;
            existingAssignment.IsPrimaryContact = assignment.IsPrimaryContact;
            existingAssignment.IsAuthorizedToPickup = assignment.IsAuthorizedToPickup;
            existingAssignment.IsEmergencyContact = assignment.IsEmergencyContact;
            existingAssignment.IsActive = assignment.IsActive;
            existingAssignment.Notes = assignment.Notes;
            existingAssignment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated parent-student assignment: ParentId {ParentId}, StudentId {StudentId}", 
                assignment.ParentId, assignment.StudentId);

            return existingAssignment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating parent-student assignment for ParentId: {ParentId}, StudentId: {StudentId}", 
                assignment.ParentId, assignment.StudentId);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid parentId, Guid studentId)
    {
        try
        {
            var assignment = await GetByIdAsync(parentId, studentId);
            if (assignment == null)
                return false;

            _context.ParentStudents.Remove(assignment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted parent-student assignment: ParentId {ParentId}, StudentId {StudentId}", parentId, studentId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting parent-student assignment for ParentId: {ParentId}, StudentId: {StudentId}", parentId, studentId);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(Guid parentId, Guid studentId)
    {
        try
        {
            return await _context.ParentStudents
                .AnyAsync(ps => ps.ParentId == parentId && ps.StudentId == studentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if parent-student assignment exists for ParentId: {ParentId}, StudentId: {StudentId}", parentId, studentId);
            throw;
        }
    }

    // Parent-specific queries
    public async Task<IEnumerable<ParentStudent>> GetByParentIdAsync(Guid parentId)
    {
        try
        {
            return await _context.ParentStudents
                .Include(ps => ps.Parent)
                .Include(ps => ps.Student)
                .Where(ps => ps.ParentId == parentId)
                .OrderBy(ps => ps.Student.LastName)
                .ThenBy(ps => ps.Student.FirstName)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving assignments for ParentId: {ParentId}", parentId);
            throw;
        }
    }

    public async Task<IEnumerable<ParentStudent>> GetActiveByParentIdAsync(Guid parentId)
    {
        try
        {
            return await _context.ParentStudents
                .Include(ps => ps.Parent)
                .Include(ps => ps.Student)
                .Where(ps => ps.ParentId == parentId && ps.IsActive)
                .OrderBy(ps => ps.Student.LastName)
                .ThenBy(ps => ps.Student.FirstName)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active assignments for ParentId: {ParentId}", parentId);
            throw;
        }
    }

    public async Task<int> GetStudentCountByParentIdAsync(Guid parentId)
    {
        try
        {
            return await _context.ParentStudents
                .CountAsync(ps => ps.ParentId == parentId && ps.IsActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting student count for ParentId: {ParentId}", parentId);
            throw;
        }
    }

    public async Task<bool> IsParentAssignedToStudentAsync(Guid parentId, Guid studentId)
    {
        try
        {
            return await _context.ParentStudents
                .AnyAsync(ps => ps.ParentId == parentId && ps.StudentId == studentId && ps.IsActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking parent-student assignment for ParentId: {ParentId}, StudentId: {StudentId}", parentId, studentId);
            throw;
        }
    }

    // Student-specific queries
    public async Task<IEnumerable<ParentStudent>> GetByStudentIdAsync(Guid studentId)
    {
        try
        {
            return await _context.ParentStudents
                .Include(ps => ps.Parent)
                .Include(ps => ps.Student)
                .Where(ps => ps.StudentId == studentId)
                .OrderBy(ps => ps.IsPrimaryContact ? 0 : 1) // Primary contact first
                .ThenBy(ps => ps.Parent.LastName)
                .ThenBy(ps => ps.Parent.FirstName)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving assignments for StudentId: {StudentId}", studentId);
            throw;
        }
    }

    public async Task<IEnumerable<ParentStudent>> GetActiveByStudentIdAsync(Guid studentId)
    {
        try
        {
            return await _context.ParentStudents
                .Include(ps => ps.Parent)
                .Include(ps => ps.Student)
                .Where(ps => ps.StudentId == studentId && ps.IsActive)
                .OrderBy(ps => ps.IsPrimaryContact ? 0 : 1) // Primary contact first
                .ThenBy(ps => ps.Parent.LastName)
                .ThenBy(ps => ps.Parent.FirstName)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active assignments for StudentId: {StudentId}", studentId);
            throw;
        }
    }

    public async Task<int> GetParentCountByStudentIdAsync(Guid studentId)
    {
        try
        {
            return await _context.ParentStudents
                .CountAsync(ps => ps.StudentId == studentId && ps.IsActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting parent count for StudentId: {StudentId}", studentId);
            throw;
        }
    }

    public async Task<ParentStudent?> GetPrimaryParentByStudentIdAsync(Guid studentId)
    {
        try
        {
            return await _context.ParentStudents
                .Include(ps => ps.Parent)
                .Include(ps => ps.Student)
                .FirstOrDefaultAsync(ps => ps.StudentId == studentId && ps.IsPrimaryContact && ps.IsActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving primary parent for StudentId: {StudentId}", studentId);
            throw;
        }
    }

    public async Task<bool> IsStudentAssignedToParentAsync(Guid studentId, Guid parentId)
    {
        try
        {
            return await _context.ParentStudents
                .AnyAsync(ps => ps.StudentId == studentId && ps.ParentId == parentId && ps.IsActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking student-parent assignment for StudentId: {StudentId}, ParentId: {ParentId}", studentId, parentId);
            throw;
        }
    }

    // Relationship management
    public async Task<bool> SetPrimaryContactAsync(Guid parentId, Guid studentId)
    {
        try
        {
            // First, remove primary contact from all other parents for this student
            var otherPrimaryContacts = await _context.ParentStudents
                .Where(ps => ps.StudentId == studentId && ps.ParentId != parentId && ps.IsPrimaryContact)
                .ToListAsync();

            foreach (var contact in otherPrimaryContacts)
            {
                contact.IsPrimaryContact = false;
                contact.UpdatedAt = DateTime.UtcNow;
            }

            // Set the specified parent as primary contact
            var assignment = await GetByIdAsync(parentId, studentId);
            if (assignment == null)
                return false;

            assignment.IsPrimaryContact = true;
            assignment.IsEmergencyContact = true; // Primary contact should also be emergency contact
            assignment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Set primary contact: ParentId {ParentId} for StudentId {StudentId}", parentId, studentId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting primary contact for ParentId: {ParentId}, StudentId: {StudentId}", parentId, studentId);
            throw;
        }
    }

    public async Task<bool> RemovePrimaryContactAsync(Guid parentId, Guid studentId)
    {
        try
        {
            var assignment = await GetByIdAsync(parentId, studentId);
            if (assignment == null)
                return false;

            assignment.IsPrimaryContact = false;
            assignment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Removed primary contact: ParentId {ParentId} for StudentId {StudentId}", parentId, studentId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing primary contact for ParentId: {ParentId}, StudentId: {StudentId}", parentId, studentId);
            throw;
        }
    }

    public async Task<bool> ActivateAssignmentAsync(Guid parentId, Guid studentId)
    {
        try
        {
            var assignment = await GetByIdAsync(parentId, studentId);
            if (assignment == null)
                return false;

            assignment.IsActive = true;
            assignment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Activated assignment: ParentId {ParentId}, StudentId {StudentId}", parentId, studentId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating assignment for ParentId: {ParentId}, StudentId: {StudentId}", parentId, studentId);
            throw;
        }
    }

    public async Task<bool> DeactivateAssignmentAsync(Guid parentId, Guid studentId)
    {
        try
        {
            var assignment = await GetByIdAsync(parentId, studentId);
            if (assignment == null)
                return false;

            assignment.IsActive = false;
            assignment.IsPrimaryContact = false; // Deactivated assignments cannot be primary contacts
            assignment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Deactivated assignment: ParentId {ParentId}, StudentId {StudentId}", parentId, studentId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating assignment for ParentId: {ParentId}, StudentId: {StudentId}", parentId, studentId);
            throw;
        }
    }

    // Bulk operations
    public async Task<IEnumerable<ParentStudent>> CreateBulkAsync(IEnumerable<ParentStudent> assignments)
    {
        try
        {
            var assignmentList = assignments.ToList();
            var now = DateTime.UtcNow;

            foreach (var assignment in assignmentList)
            {
                assignment.CreatedAt = now;
                assignment.UpdatedAt = now;
            }

            _context.ParentStudents.AddRange(assignmentList);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created {Count} parent-student assignments in bulk", assignmentList.Count);
            return assignmentList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating bulk parent-student assignments");
            throw;
        }
    }

    public async Task<bool> DeleteBulkByParentIdAsync(Guid parentId)
    {
        try
        {
            var assignments = await _context.ParentStudents
                .Where(ps => ps.ParentId == parentId)
                .ToListAsync();

            if (!assignments.Any())
                return false;

            _context.ParentStudents.RemoveRange(assignments);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted {Count} assignments for ParentId {ParentId}", assignments.Count, parentId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting bulk assignments for ParentId: {ParentId}", parentId);
            throw;
        }
    }

    public async Task<bool> DeleteBulkByStudentIdAsync(Guid studentId)
    {
        try
        {
            var assignments = await _context.ParentStudents
                .Where(ps => ps.StudentId == studentId)
                .ToListAsync();

            if (!assignments.Any())
                return false;

            _context.ParentStudents.RemoveRange(assignments);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted {Count} assignments for StudentId {StudentId}", assignments.Count, studentId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting bulk assignments for StudentId: {StudentId}", studentId);
            throw;
        }
    }

    // Validation queries
    public async Task<bool> CanAssignParentToStudentAsync(Guid parentId, Guid studentId)
    {
        try
        {
            // Check if parent and student exist
            var parentExists = await _context.Parents.AnyAsync(p => p.Id == parentId && p.IsActive);
            var studentExists = await _context.Students.AnyAsync(s => s.Id == studentId && s.Status == Enums.StudentStatus.Active);

            if (!parentExists || !studentExists)
                return false;

            // Check if assignment already exists
            var assignmentExists = await ExistsAsync(parentId, studentId);
            return !assignmentExists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if parent can be assigned to student for ParentId: {ParentId}, StudentId: {StudentId}", parentId, studentId);
            throw;
        }
    }

    public async Task<IEnumerable<ParentStudent>> GetConflictingAssignmentsAsync(Guid parentId, Guid studentId)
    {
        try
        {
            return await _context.ParentStudents
                .Include(ps => ps.Parent)
                .Include(ps => ps.Student)
                .Where(ps => ps.ParentId == parentId && ps.StudentId == studentId)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving conflicting assignments for ParentId: {ParentId}, StudentId: {StudentId}", parentId, studentId);
            throw;
        }
    }

    // Statistics and reporting
    public async Task<int> GetTotalAssignmentsCountAsync()
    {
        try
        {
            return await _context.ParentStudents.CountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total assignments count");
            throw;
        }
    }

    public async Task<int> GetActiveAssignmentsCountAsync()
    {
        try
        {
            return await _context.ParentStudents.CountAsync(ps => ps.IsActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active assignments count");
            throw;
        }
    }

    public async Task<Dictionary<string, int>> GetAssignmentsByRelationshipTypeAsync()
    {
        try
        {
            return await _context.ParentStudents
                .Where(ps => ps.IsActive)
                .GroupBy(ps => ps.Relationship)
                .ToDictionaryAsync(g => g.Key, g => g.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting assignments by relationship type");
            throw;
        }
    }

    public async Task<IEnumerable<ParentStudent>> GetOrphanedStudentsAsync()
    {
        try
        {
            var studentsWithParents = await _context.ParentStudents
                .Where(ps => ps.IsActive)
                .Select(ps => ps.StudentId)
                .Distinct()
                .ToListAsync();

            var allStudents = await _context.Students
                .Where(s => s.Status == Enums.StudentStatus.Active)
                .Select(s => s.Id)
                .ToListAsync();

            var orphanedStudentIds = allStudents.Except(studentsWithParents).ToList();

            return await _context.ParentStudents
                .Include(ps => ps.Student)
                .Where(ps => orphanedStudentIds.Contains(ps.StudentId))
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting orphaned students");
            throw;
        }
    }

    public async Task<IEnumerable<ParentStudent>> GetParentsWithoutStudentsAsync()
    {
        try
        {
            var parentsWithStudents = await _context.ParentStudents
                .Where(ps => ps.IsActive)
                .Select(ps => ps.ParentId)
                .Distinct()
                .ToListAsync();

            var allParents = await _context.Parents
                .Where(p => p.IsActive)
                .Select(p => p.Id)
                .ToListAsync();

            var parentsWithoutStudentIds = allParents.Except(parentsWithStudents).ToList();

            return await _context.ParentStudents
                .Include(ps => ps.Parent)
                .Where(ps => parentsWithoutStudentIds.Contains(ps.ParentId))
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting parents without students");
            throw;
        }
    }
}