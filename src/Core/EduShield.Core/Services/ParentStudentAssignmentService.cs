using EduShield.Core.Dtos;
using EduShield.Core.Entities;
using EduShield.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace EduShield.Core.Services;

/// <summary>
/// Service implementation for managing parent-student assignments
/// </summary>
public class ParentStudentAssignmentService : IParentStudentAssignmentService
{
    private readonly IParentStudentAssignmentRepository _assignmentRepository;
    private readonly IParentRepository _parentRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ILogger<ParentStudentAssignmentService> _logger;

    public ParentStudentAssignmentService(
        IParentStudentAssignmentRepository assignmentRepository,
        IParentRepository parentRepository,
        IStudentRepository studentRepository,
        ILogger<ParentStudentAssignmentService> logger)
    {
        _assignmentRepository = assignmentRepository;
        _parentRepository = parentRepository;
        _studentRepository = studentRepository;
        _logger = logger;
    }

    // Basic CRUD operations
    public async Task<ParentStudentAssignmentDto?> GetAssignmentAsync(Guid parentId, Guid studentId)
    {
        try
        {
            var assignment = await _assignmentRepository.GetByIdAsync(parentId, studentId);
            return assignment != null ? MapToDto(assignment) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving assignment for ParentId: {ParentId}, StudentId: {StudentId}", parentId, studentId);
            throw;
        }
    }

    public async Task<IEnumerable<ParentStudentAssignmentDto>> GetAllAssignmentsAsync()
    {
        try
        {
            var assignments = await _assignmentRepository.GetAllAsync();
            return assignments.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all assignments");
            throw;
        }
    }

    public async Task<ParentStudentAssignmentDto> CreateAssignmentAsync(CreateParentStudentAssignmentDto createDto)
    {
        try
        {
            // Validate that parent and student exist
            var parent = await _parentRepository.GetByIdAsync(createDto.ParentId);
            if (parent == null)
                throw new ArgumentException($"Parent with ID {createDto.ParentId} not found");

            var student = await _studentRepository.GetByIdAsync(createDto.StudentId);
            if (student == null)
                throw new ArgumentException($"Student with ID {createDto.StudentId} not found");

            // Check if assignment already exists
            var existingAssignment = await _assignmentRepository.GetByIdAsync(createDto.ParentId, createDto.StudentId);
            if (existingAssignment != null)
                throw new InvalidOperationException($"Assignment already exists between parent {parent.FullName} and student {student.FullName}");

            // If this is set as primary contact, remove primary contact from other parents for this student
            if (createDto.IsPrimaryContact)
            {
                var existingPrimary = await _assignmentRepository.GetPrimaryParentByStudentIdAsync(createDto.StudentId);
                if (existingPrimary != null)
                {
                    await _assignmentRepository.RemovePrimaryContactAsync(existingPrimary.ParentId, createDto.StudentId);
                    _logger.LogInformation("Removed primary contact from ParentId {ParentId} for StudentId {StudentId}", 
                        existingPrimary.ParentId, createDto.StudentId);
                }
            }

            var assignment = new ParentStudent
            {
                ParentId = createDto.ParentId,
                StudentId = createDto.StudentId,
                Relationship = createDto.Relationship,
                IsPrimaryContact = createDto.IsPrimaryContact,
                IsAuthorizedToPickup = createDto.IsAuthorizedToPickup,
                IsEmergencyContact = createDto.IsEmergencyContact,
                IsActive = true,
                Notes = createDto.Notes
            };

            var createdAssignment = await _assignmentRepository.CreateAsync(assignment);
            
            // Update the student's ParentId field for backward compatibility (especially for primary contact)
            if (createDto.IsPrimaryContact || student.ParentId == null)
            {
                student.ParentId = createDto.ParentId;
                await _studentRepository.UpdateAsync(student);
                _logger.LogInformation("Updated Student {StudentId} ParentId to {ParentId} for backward compatibility", 
                    createDto.StudentId, createDto.ParentId);
            }
            
            // Load the full assignment with navigation properties
            var fullAssignment = await _assignmentRepository.GetByIdAsync(createdAssignment.ParentId, createdAssignment.StudentId);
            
            _logger.LogInformation("Created assignment between {ParentName} and {StudentName} with relationship {Relationship}", 
                parent.FullName, student.FullName, createDto.Relationship);

            return MapToDto(fullAssignment!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating assignment for ParentId: {ParentId}, StudentId: {StudentId}", 
                createDto.ParentId, createDto.StudentId);
            throw;
        }
    }

    public async Task<ParentStudentAssignmentDto> UpdateAssignmentAsync(Guid parentId, Guid studentId, UpdateParentStudentAssignmentDto updateDto)
    {
        try
        {
            var existingAssignment = await _assignmentRepository.GetByIdAsync(parentId, studentId);
            if (existingAssignment == null)
                throw new ArgumentException($"Assignment not found for ParentId: {parentId}, StudentId: {studentId}");

            // If setting as primary contact, remove primary contact from other parents for this student
            if (updateDto.IsPrimaryContact == true && !existingAssignment.IsPrimaryContact)
            {
                var existingPrimary = await _assignmentRepository.GetPrimaryParentByStudentIdAsync(studentId);
                if (existingPrimary != null && existingPrimary.ParentId != parentId)
                {
                    await _assignmentRepository.RemovePrimaryContactAsync(existingPrimary.ParentId, studentId);
                    _logger.LogInformation("Removed primary contact from ParentId {ParentId} for StudentId {StudentId}", 
                        existingPrimary.ParentId, studentId);
                }
                
                // Update the student's ParentId field for backward compatibility when setting primary contact
                var student = await _studentRepository.GetByIdAsync(studentId);
                if (student != null)
                {
                    student.ParentId = parentId;
                    await _studentRepository.UpdateAsync(student);
                    _logger.LogInformation("Updated Student {StudentId} ParentId to {ParentId} for primary contact", 
                        studentId, parentId);
                }
            }

            // Update properties if provided
            if (!string.IsNullOrEmpty(updateDto.Relationship))
                existingAssignment.Relationship = updateDto.Relationship;
            
            if (updateDto.IsPrimaryContact.HasValue)
                existingAssignment.IsPrimaryContact = updateDto.IsPrimaryContact.Value;
            
            if (updateDto.IsAuthorizedToPickup.HasValue)
                existingAssignment.IsAuthorizedToPickup = updateDto.IsAuthorizedToPickup.Value;
            
            if (updateDto.IsEmergencyContact.HasValue)
                existingAssignment.IsEmergencyContact = updateDto.IsEmergencyContact.Value;
            
            if (updateDto.IsActive.HasValue)
            {
                existingAssignment.IsActive = updateDto.IsActive.Value;
                // If deactivating, also remove primary contact status and update student's ParentId
                if (!updateDto.IsActive.Value)
                {
                    existingAssignment.IsPrimaryContact = false;
                    
                    // If this was the primary parent, clear the student's ParentId and find a new primary
                    if (existingAssignment.IsPrimaryContact)
                    {
                        var student = await _studentRepository.GetByIdAsync(studentId);
                        if (student != null && student.ParentId == parentId)
                        {
                            // Find another active parent to set as primary
                            var otherActiveAssignments = await _assignmentRepository.GetActiveByStudentIdAsync(studentId);
                            var newPrimaryParent = otherActiveAssignments.FirstOrDefault(a => a.ParentId != parentId);
                            
                            if (newPrimaryParent != null)
                            {
                                student.ParentId = newPrimaryParent.ParentId;
                                newPrimaryParent.IsPrimaryContact = true;
                                await _assignmentRepository.UpdateAsync(newPrimaryParent);
                                _logger.LogInformation("Set ParentId {ParentId} as new primary contact for StudentId {StudentId}", 
                                    newPrimaryParent.ParentId, studentId);
                            }
                            else
                            {
                                student.ParentId = null;
                                _logger.LogInformation("Cleared ParentId for StudentId {StudentId} as no other active parents found", studentId);
                            }
                            
                            await _studentRepository.UpdateAsync(student);
                        }
                    }
                }
            }
            
            if (updateDto.Notes != null)
                existingAssignment.Notes = updateDto.Notes;

            var updatedAssignment = await _assignmentRepository.UpdateAsync(existingAssignment);
            
            _logger.LogInformation("Updated assignment for ParentId: {ParentId}, StudentId: {StudentId}", parentId, studentId);

            return MapToDto(updatedAssignment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating assignment for ParentId: {ParentId}, StudentId: {StudentId}", parentId, studentId);
            throw;
        }
    }

    public async Task<bool> DeleteAssignmentAsync(Guid parentId, Guid studentId)
    {
        try
        {
            // Check if this is the primary parent before deletion
            var existingAssignment = await _assignmentRepository.GetByIdAsync(parentId, studentId);
            var wasPrimaryContact = existingAssignment?.IsPrimaryContact ?? false;
            
            var result = await _assignmentRepository.DeleteAsync(parentId, studentId);
            
            if (result)
            {
                // If the deleted assignment was the primary contact, update student's ParentId
                if (wasPrimaryContact)
                {
                    var student = await _studentRepository.GetByIdAsync(studentId);
                    if (student != null && student.ParentId == parentId)
                    {
                        // Find another active parent to set as primary
                        var otherActiveAssignments = await _assignmentRepository.GetActiveByStudentIdAsync(studentId);
                        var newPrimaryParent = otherActiveAssignments.FirstOrDefault();
                        
                        if (newPrimaryParent != null)
                        {
                            student.ParentId = newPrimaryParent.ParentId;
                            newPrimaryParent.IsPrimaryContact = true;
                            await _assignmentRepository.UpdateAsync(newPrimaryParent);
                            await _studentRepository.UpdateAsync(student);
                            _logger.LogInformation("Set ParentId {ParentId} as new primary contact for StudentId {StudentId} after deletion", 
                                newPrimaryParent.ParentId, studentId);
                        }
                        else
                        {
                            student.ParentId = null;
                            await _studentRepository.UpdateAsync(student);
                            _logger.LogInformation("Cleared ParentId for StudentId {StudentId} as no other active parents found after deletion", studentId);
                        }
                    }
                }
                
                _logger.LogInformation("Deleted assignment for ParentId: {ParentId}, StudentId: {StudentId}", parentId, studentId);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting assignment for ParentId: {ParentId}, StudentId: {StudentId}", parentId, studentId);
            throw;
        }
    }

    // Parent-specific operations
    public async Task<ParentWithStudentsDto?> GetParentWithStudentsAsync(Guid parentId)
    {
        try
        {
            var parent = await _parentRepository.GetByIdAsync(parentId);
            if (parent == null)
                return null;

            var assignments = await _assignmentRepository.GetByParentIdAsync(parentId);
            
            return new ParentWithStudentsDto
            {
                ParentId = parent.Id,
                ParentFirstName = parent.FirstName,
                ParentLastName = parent.LastName,
                ParentFullName = parent.FullName,
                ParentEmail = parent.Email,
                ParentPhoneNumber = parent.PhoneNumber,
                AssignedStudents = assignments.Select(a => new StudentAssignmentDto
                {
                    StudentId = a.StudentId,
                    StudentFirstName = a.Student?.FirstName ?? "",
                    StudentLastName = a.Student?.LastName ?? "",
                    StudentFullName = a.Student?.FullName ?? "",
                    StudentEmail = a.Student?.Email ?? "",
                    StudentRollNumber = a.Student?.RollNumber ?? "",
                    StudentGrade = a.Student?.Grade,
                    StudentSection = a.Student?.Section,
                    Relationship = a.Relationship,
                    IsPrimaryContact = a.IsPrimaryContact,
                    IsAuthorizedToPickup = a.IsAuthorizedToPickup,
                    IsEmergencyContact = a.IsEmergencyContact,
                    IsActive = a.IsActive,
                    Notes = a.Notes,
                    AssignedDate = a.CreatedAt
                }).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving parent with students for ParentId: {ParentId}", parentId);
            throw;
        }
    }

    public async Task<IEnumerable<ParentStudentAssignmentDto>> GetAssignmentsByParentIdAsync(Guid parentId)
    {
        try
        {
            var assignments = await _assignmentRepository.GetByParentIdAsync(parentId);
            return assignments.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving assignments for ParentId: {ParentId}", parentId);
            throw;
        }
    }

    public async Task<IEnumerable<ParentStudentAssignmentDto>> GetActiveAssignmentsByParentIdAsync(Guid parentId)
    {
        try
        {
            var assignments = await _assignmentRepository.GetActiveByParentIdAsync(parentId);
            return assignments.Select(MapToDto);
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
            return await _assignmentRepository.GetStudentCountByParentIdAsync(parentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting student count for ParentId: {ParentId}", parentId);
            throw;
        }
    }

    // Student-specific operations
    public async Task<StudentWithParentsDto?> GetStudentWithParentsAsync(Guid studentId)
    {
        try
        {
            var student = await _studentRepository.GetByIdAsync(studentId);
            if (student == null)
                return null;

            var assignments = await _assignmentRepository.GetByStudentIdAsync(studentId);
            
            return new StudentWithParentsDto
            {
                StudentId = student.Id,
                StudentFirstName = student.FirstName,
                StudentLastName = student.LastName,
                StudentFullName = student.FullName,
                StudentEmail = student.Email,
                StudentRollNumber = student.RollNumber,
                AssignedParents = assignments.Select(a => new ParentAssignmentDto
                {
                    ParentId = a.ParentId,
                    ParentFirstName = a.Parent?.FirstName ?? "",
                    ParentLastName = a.Parent?.LastName ?? "",
                    ParentFullName = a.Parent?.FullName ?? "",
                    ParentEmail = a.Parent?.Email ?? "",
                    ParentPhoneNumber = a.Parent?.PhoneNumber ?? "",
                    ParentOccupation = a.Parent?.Occupation,
                    Relationship = a.Relationship,
                    IsPrimaryContact = a.IsPrimaryContact,
                    IsAuthorizedToPickup = a.IsAuthorizedToPickup,
                    IsEmergencyContact = a.IsEmergencyContact,
                    IsActive = a.IsActive,
                    Notes = a.Notes,
                    AssignedDate = a.CreatedAt
                }).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving student with parents for StudentId: {StudentId}", studentId);
            throw;
        }
    }

    public async Task<IEnumerable<ParentStudentAssignmentDto>> GetAssignmentsByStudentIdAsync(Guid studentId)
    {
        try
        {
            var assignments = await _assignmentRepository.GetByStudentIdAsync(studentId);
            return assignments.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving assignments for StudentId: {StudentId}", studentId);
            throw;
        }
    }

    public async Task<IEnumerable<ParentStudentAssignmentDto>> GetActiveAssignmentsByStudentIdAsync(Guid studentId)
    {
        try
        {
            var assignments = await _assignmentRepository.GetActiveByStudentIdAsync(studentId);
            return assignments.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active assignments for StudentId: {StudentId}", studentId);
            throw;
        }
    }

    public async Task<ParentStudentAssignmentDto?> GetPrimaryParentByStudentIdAsync(Guid studentId)
    {
        try
        {
            var assignment = await _assignmentRepository.GetPrimaryParentByStudentIdAsync(studentId);
            return assignment != null ? MapToDto(assignment) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving primary parent for StudentId: {StudentId}", studentId);
            throw;
        }
    }

    public async Task<int> GetParentCountByStudentIdAsync(Guid studentId)
    {
        try
        {
            return await _assignmentRepository.GetParentCountByStudentIdAsync(studentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting parent count for StudentId: {StudentId}", studentId);
            throw;
        }
    }

    // Relationship management
    public async Task<bool> SetPrimaryContactAsync(Guid parentId, Guid studentId)
    {
        try
        {
            var result = await _assignmentRepository.SetPrimaryContactAsync(parentId, studentId);
            
            if (result)
            {
                // Update the student's ParentId field for backward compatibility
                var student = await _studentRepository.GetByIdAsync(studentId);
                if (student != null)
                {
                    student.ParentId = parentId;
                    await _studentRepository.UpdateAsync(student);
                    _logger.LogInformation("Updated Student {StudentId} ParentId to {ParentId} when setting primary contact", 
                        studentId, parentId);
                }
            }
            
            return result;
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
            var result = await _assignmentRepository.RemovePrimaryContactAsync(parentId, studentId);
            
            if (result)
            {
                // Update the student's ParentId field for backward compatibility
                var student = await _studentRepository.GetByIdAsync(studentId);
                if (student != null && student.ParentId == parentId)
                {
                    // Find another active parent to set as primary
                    var otherActiveAssignments = await _assignmentRepository.GetActiveByStudentIdAsync(studentId);
                    var newPrimaryParent = otherActiveAssignments.FirstOrDefault(a => a.ParentId != parentId);
                    
                    if (newPrimaryParent != null)
                    {
                        student.ParentId = newPrimaryParent.ParentId;
                        newPrimaryParent.IsPrimaryContact = true;
                        await _assignmentRepository.UpdateAsync(newPrimaryParent);
                        _logger.LogInformation("Set ParentId {ParentId} as new primary contact for StudentId {StudentId}", 
                            newPrimaryParent.ParentId, studentId);
                    }
                    else
                    {
                        student.ParentId = null;
                        _logger.LogInformation("Cleared ParentId for StudentId {StudentId} as no other active parents found", studentId);
                    }
                    
                    await _studentRepository.UpdateAsync(student);
                }
            }
            
            return result;
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
            return await _assignmentRepository.ActivateAssignmentAsync(parentId, studentId);
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
            return await _assignmentRepository.DeactivateAssignmentAsync(parentId, studentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating assignment for ParentId: {ParentId}, StudentId: {StudentId}", parentId, studentId);
            throw;
        }
    }

    // Bulk operations
    public async Task<IEnumerable<ParentStudentAssignmentDto>> CreateBulkAssignmentsAsync(BulkParentStudentAssignmentDto bulkDto)
    {
        try
        {
            // Validate parent exists
            var parent = await _parentRepository.GetByIdAsync(bulkDto.ParentId);
            if (parent == null)
                throw new ArgumentException($"Parent with ID {bulkDto.ParentId} not found");

            var assignments = new List<ParentStudent>();
            
            foreach (var studentId in bulkDto.StudentIds)
            {
                // Validate student exists
                var student = await _studentRepository.GetByIdAsync(studentId);
                if (student == null)
                {
                    _logger.LogWarning("Student with ID {StudentId} not found, skipping assignment", studentId);
                    continue;
                }

                // Check if assignment already exists
                var existingAssignment = await _assignmentRepository.GetByIdAsync(bulkDto.ParentId, studentId);
                if (existingAssignment != null)
                {
                    _logger.LogWarning("Assignment already exists between ParentId {ParentId} and StudentId {StudentId}, skipping", 
                        bulkDto.ParentId, studentId);
                    continue;
                }

                assignments.Add(new ParentStudent
                {
                    ParentId = bulkDto.ParentId,
                    StudentId = studentId,
                    Relationship = bulkDto.Relationship,
                    IsPrimaryContact = bulkDto.IsPrimaryContact,
                    IsAuthorizedToPickup = bulkDto.IsAuthorizedToPickup,
                    IsEmergencyContact = bulkDto.IsEmergencyContact,
                    IsActive = true,
                    Notes = bulkDto.Notes
                });
            }

            if (!assignments.Any())
                throw new InvalidOperationException("No valid assignments to create");

            var createdAssignments = await _assignmentRepository.CreateBulkAsync(assignments);
            
            // Update student ParentId fields for backward compatibility (especially for primary contacts)
            if (bulkDto.IsPrimaryContact)
            {
                foreach (var assignment in createdAssignments)
                {
                    var student = await _studentRepository.GetByIdAsync(assignment.StudentId);
                    if (student != null && (student.ParentId == null || bulkDto.IsPrimaryContact))
                    {
                        student.ParentId = bulkDto.ParentId;
                        await _studentRepository.UpdateAsync(student);
                        _logger.LogInformation("Updated Student {StudentId} ParentId to {ParentId} for bulk assignment", 
                            assignment.StudentId, bulkDto.ParentId);
                    }
                }
            }
            
            // Load full assignments with navigation properties
            var result = new List<ParentStudentAssignmentDto>();
            foreach (var assignment in createdAssignments)
            {
                var fullAssignment = await _assignmentRepository.GetByIdAsync(assignment.ParentId, assignment.StudentId);
                if (fullAssignment != null)
                    result.Add(MapToDto(fullAssignment));
            }

            _logger.LogInformation("Created {Count} bulk assignments for ParentId: {ParentId}", result.Count, bulkDto.ParentId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating bulk assignments for ParentId: {ParentId}", bulkDto.ParentId);
            throw;
        }
    }

    public async Task<bool> DeleteAllAssignmentsByParentIdAsync(Guid parentId)
    {
        try
        {
            return await _assignmentRepository.DeleteBulkByParentIdAsync(parentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting all assignments for ParentId: {ParentId}", parentId);
            throw;
        }
    }

    public async Task<bool> DeleteAllAssignmentsByStudentIdAsync(Guid studentId)
    {
        try
        {
            return await _assignmentRepository.DeleteBulkByStudentIdAsync(studentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting all assignments for StudentId: {StudentId}", studentId);
            throw;
        }
    }

    // Validation
    public async Task<bool> CanAssignParentToStudentAsync(Guid parentId, Guid studentId)
    {
        try
        {
            return await _assignmentRepository.CanAssignParentToStudentAsync(parentId, studentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if parent can be assigned to student for ParentId: {ParentId}, StudentId: {StudentId}", parentId, studentId);
            throw;
        }
    }

    public async Task<bool> IsParentAssignedToStudentAsync(Guid parentId, Guid studentId)
    {
        try
        {
            return await _assignmentRepository.IsParentAssignedToStudentAsync(parentId, studentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking parent assignment for ParentId: {ParentId}, StudentId: {StudentId}", parentId, studentId);
            throw;
        }
    }

    public async Task<bool> IsStudentAssignedToParentAsync(Guid studentId, Guid parentId)
    {
        try
        {
            return await _assignmentRepository.IsStudentAssignedToParentAsync(studentId, parentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking student assignment for StudentId: {StudentId}, ParentId: {ParentId}", studentId, parentId);
            throw;
        }
    }

    // Statistics and reporting
    public async Task<int> GetTotalAssignmentsCountAsync()
    {
        try
        {
            return await _assignmentRepository.GetTotalAssignmentsCountAsync();
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
            return await _assignmentRepository.GetActiveAssignmentsCountAsync();
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
            return await _assignmentRepository.GetAssignmentsByRelationshipTypeAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting assignments by relationship type");
            throw;
        }
    }

    public async Task<IEnumerable<StudentWithParentsDto>> GetOrphanedStudentsAsync()
    {
        try
        {
            var orphanedAssignments = await _assignmentRepository.GetOrphanedStudentsAsync();
            var studentIds = orphanedAssignments.Select(a => a.StudentId).Distinct();
            
            var result = new List<StudentWithParentsDto>();
            foreach (var studentId in studentIds)
            {
                var studentWithParents = await GetStudentWithParentsAsync(studentId);
                if (studentWithParents != null)
                    result.Add(studentWithParents);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting orphaned students");
            throw;
        }
    }

    public async Task<IEnumerable<ParentWithStudentsDto>> GetParentsWithoutStudentsAsync()
    {
        try
        {
            var parentAssignments = await _assignmentRepository.GetParentsWithoutStudentsAsync();
            var parentIds = parentAssignments.Select(a => a.ParentId).Distinct();
            
            var result = new List<ParentWithStudentsDto>();
            foreach (var parentId in parentIds)
            {
                var parentWithStudents = await GetParentWithStudentsAsync(parentId);
                if (parentWithStudents != null)
                    result.Add(parentWithStudents);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting parents without students");
            throw;
        }
    }

    // Data consistency methods
    public async Task<int> SyncLegacyParentStudentDataAsync()
    {
        try
        {
            var syncCount = 0;
            
            // Get all students with ParentId but no corresponding ParentStudent relationship
            var studentsWithParents = await _studentRepository.GetAllAsync();
            
            foreach (var student in studentsWithParents.Where(s => s.ParentId.HasValue))
            {
                var existingAssignment = await _assignmentRepository.GetByIdAsync(student.ParentId!.Value, student.Id);
                if (existingAssignment == null)
                {
                    // Create the missing ParentStudent relationship
                    var assignment = new ParentStudent
                    {
                        ParentId = student.ParentId.Value,
                        StudentId = student.Id,
                        Relationship = "Parent",
                        IsPrimaryContact = true,
                        IsAuthorizedToPickup = true,
                        IsEmergencyContact = true,
                        IsActive = true,
                        Notes = "Synced from legacy parent relationship"
                    };
                    
                    await _assignmentRepository.CreateAsync(assignment);
                    syncCount++;
                    
                    _logger.LogInformation("Synced legacy parent relationship for StudentId {StudentId} and ParentId {ParentId}", 
                        student.Id, student.ParentId.Value);
                }
            }
            
            // Also sync the reverse - update Student.ParentId for primary contacts that don't have it set
            var primaryAssignments = await _assignmentRepository.GetAllAsync();
            foreach (var assignment in primaryAssignments.Where(a => a.IsPrimaryContact && a.IsActive))
            {
                var student = await _studentRepository.GetByIdAsync(assignment.StudentId);
                if (student != null && student.ParentId != assignment.ParentId)
                {
                    student.ParentId = assignment.ParentId;
                    await _studentRepository.UpdateAsync(student);
                    syncCount++;
                    
                    _logger.LogInformation("Updated Student {StudentId} ParentId to {ParentId} for consistency", 
                        assignment.StudentId, assignment.ParentId);
                }
            }
            
            _logger.LogInformation("Synced {Count} legacy parent-student relationships", syncCount);
            return syncCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing legacy parent-student data");
            throw;
        }
    }

    // Helper method to map entity to DTO
    private static ParentStudentAssignmentDto MapToDto(ParentStudent assignment)
    {
        return new ParentStudentAssignmentDto
        {
            ParentId = assignment.ParentId,
            ParentFirstName = assignment.Parent?.FirstName ?? "",
            ParentLastName = assignment.Parent?.LastName ?? "",
            ParentFullName = assignment.Parent?.FullName ?? "",
            ParentEmail = assignment.Parent?.Email ?? "",
            ParentPhoneNumber = assignment.Parent?.PhoneNumber ?? "",
            
            StudentId = assignment.StudentId,
            StudentFirstName = assignment.Student?.FirstName ?? "",
            StudentLastName = assignment.Student?.LastName ?? "",
            StudentFullName = assignment.Student?.FullName ?? "",
            StudentEmail = assignment.Student?.Email ?? "",
            StudentRollNumber = assignment.Student?.RollNumber ?? "",
            
            Relationship = assignment.Relationship,
            IsPrimaryContact = assignment.IsPrimaryContact,
            IsAuthorizedToPickup = assignment.IsAuthorizedToPickup,
            IsEmergencyContact = assignment.IsEmergencyContact,
            IsActive = assignment.IsActive,
            Notes = assignment.Notes,
            
            CreatedAt = assignment.CreatedAt,
            UpdatedAt = assignment.UpdatedAt
        };
    }
}