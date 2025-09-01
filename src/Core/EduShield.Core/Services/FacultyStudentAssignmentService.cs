using EduShield.Core.Dtos;
using EduShield.Core.Entities;
using EduShield.Core.Interfaces;

namespace EduShield.Core.Services;

/// <summary>
/// Service implementation for managing faculty-student assignments
/// </summary>
public class FacultyStudentAssignmentService : IFacultyStudentAssignmentService
{
    private readonly IFacultyStudentAssignmentRepository _assignmentRepository;
    private readonly IFacultyRepository _facultyRepository;
    private readonly IStudentRepository _studentRepository;

    public FacultyStudentAssignmentService(
        IFacultyStudentAssignmentRepository assignmentRepository,
        IFacultyRepository facultyRepository,
        IStudentRepository studentRepository)
    {
        _assignmentRepository = assignmentRepository;
        _facultyRepository = facultyRepository;
        _studentRepository = studentRepository;
    }

    public async Task<ServiceResult<FacultyStudentAssignmentDto>> AssignStudentToFacultyAsync(CreateFacultyStudentAssignmentRequest request)
    {
        try
        {
            // Validate faculty exists
            var faculty = await _facultyRepository.GetByIdAsync(request.FacultyId);
            if (faculty == null)
            {
                return ServiceResult<FacultyStudentAssignmentDto>.CreateFailure("Faculty not found");
            }

            // Validate student exists
            var student = await _studentRepository.GetByIdAsync(request.StudentId);
            if (student == null)
            {
                return ServiceResult<FacultyStudentAssignmentDto>.CreateFailure("Student not found");
            }

            // Check if assignment already exists
            var existingAssignment = await _assignmentRepository.GetByFacultyAndStudentAsync(request.FacultyId, request.StudentId);
            if (existingAssignment != null)
            {
                return ServiceResult<FacultyStudentAssignmentDto>.CreateFailure("Assignment already exists");
            }

            // Create new assignment
            var assignment = new StudentFaculty
            {
                FacultyId = request.FacultyId,
                StudentId = request.StudentId,
                AssignedDate = DateTime.UtcNow,
                IsActive = true,
                Notes = request.Notes
            };

            // Create the assignment in the repository
            var createdAssignment = await _assignmentRepository.CreateAsync(assignment);

            // Map to DTO and return
            var dto = new FacultyStudentAssignmentDto
            {
                FacultyId = createdAssignment.FacultyId,
                StudentId = createdAssignment.StudentId,
                AssignedDate = createdAssignment.AssignedDate,
                IsActive = createdAssignment.IsActive,
                Notes = createdAssignment.Notes
            };

            return ServiceResult<FacultyStudentAssignmentDto>.CreateSuccess(dto);
        }
        catch (Exception ex)
        {
            return ServiceResult<FacultyStudentAssignmentDto>.CreateFailure($"Error creating assignment: {ex.Message}");
        }
    }

    public async Task<ServiceResult<List<FacultyStudentAssignmentDto>>> BulkAssignStudentsToFacultyAsync(BulkFacultyStudentAssignmentRequest request)
    {
        try
        {
            // Validate faculty exists
            var faculty = await _facultyRepository.GetByIdAsync(request.FacultyId);
            if (faculty == null)
            {
                return ServiceResult<List<FacultyStudentAssignmentDto>>.CreateFailure("Faculty not found");
            }

            // Validate all students exist
            var students = new List<Student>();
            foreach (var studentId in request.StudentIds)
            {
                var student = await _studentRepository.GetByIdAsync(studentId);
                if (student == null)
                {
                    return ServiceResult<List<FacultyStudentAssignmentDto>>.CreateFailure($"Student with ID {studentId} not found");
                }
                students.Add(student);
            }

            var results = new List<FacultyStudentAssignmentDto>();
            var errors = new List<string>();

            // Ensure collections are initialized
            if (faculty.StudentFaculties == null)
                faculty.StudentFaculties = new List<StudentFaculty>();

            foreach (var student in students)
            {
                // Check if assignment already exists
                if (await _assignmentRepository.ExistsAsync(request.FacultyId, student.Id))
                {
                    errors.Add($"Assignment already exists for student {student.FullName}");
                    continue;
                }

                // Initialize student collection if needed
                if (student.StudentFaculties == null)
                    student.StudentFaculties = new List<StudentFaculty>();

                // Use helper methods to maintain bidirectional relationship
                student.AssignFaculty(request.FacultyId, request.Notes);
                faculty.AssignStudent(student.Id, request.Notes);

                // Create new assignment
                var assignment = new StudentFaculty
                {
                    FacultyId = request.FacultyId,
                    StudentId = student.Id,
                    AssignedDate = DateTime.UtcNow,
                    IsActive = true,
                    Notes = request.Notes
                };

                // Create the assignment
                var createdAssignment = await _assignmentRepository.CreateAsync(assignment);
                var dto = await MapToAssignmentDtoAsync(createdAssignment);
                results.Add(dto);
            }

            if (results.Any())
            {
                var message = errors.Any() 
                    ? $"Successfully assigned {results.Count} students. {errors.Count} assignments failed."
                    : $"Successfully assigned {results.Count} students to faculty.";
                
                return ServiceResult<List<FacultyStudentAssignmentDto>>.CreateSuccess(results, message);
            }
            else
            {
                return ServiceResult<List<FacultyStudentAssignmentDto>>.CreateFailure("No students were assigned successfully", errors);
            }
        }
        catch (Exception ex)
        {
            return ServiceResult<List<FacultyStudentAssignmentDto>>.CreateFailure($"Failed to perform bulk assignment: {ex.Message}");
        }
    }

    public async Task<ServiceResult<FacultyStudentAssignmentDto>> UpdateAssignmentAsync(UpdateFacultyStudentAssignmentRequest request)
    {
        try
        {
            var assignment = await _assignmentRepository.GetByFacultyAndStudentAsync(request.FacultyId, request.StudentId);
            if (assignment == null)
            {
                return ServiceResult<FacultyStudentAssignmentDto>.CreateFailure("Assignment not found");
            }

            // Update the assignment properties
            assignment.IsActive = request.IsActive;
            assignment.Notes = request.Notes;

            // Use helper methods to maintain bidirectional relationship if status changed
            if (assignment.Student != null)
            {
                if (request.IsActive)
                {
                    assignment.Student.ActivateFacultyAssignment(request.FacultyId);
                }
                else
                {
                    assignment.Student.DeactivateFacultyAssignment(request.FacultyId);
                }
            }
            
            if (assignment.Faculty != null)
            {
                if (request.IsActive)
                {
                    assignment.Faculty.ActivateStudentAssignment(request.StudentId);
                }
                else
                {
                    assignment.Faculty.DeactivateStudentAssignment(request.StudentId);
                }
            }

            var updatedAssignment = await _assignmentRepository.UpdateAsync(assignment);

            var dto = await MapToAssignmentDtoAsync(updatedAssignment);
            return ServiceResult<FacultyStudentAssignmentDto>.CreateSuccess(dto, "Assignment updated successfully");
        }
        catch (Exception ex)
        {
            return ServiceResult<FacultyStudentAssignmentDto>.CreateFailure($"Failed to update assignment: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> DeactivateAssignmentAsync(Guid facultyId, Guid studentId)
    {
        try
        {
            // Get the assignment with navigation properties
            var assignment = await _assignmentRepository.GetByFacultyAndStudentAsync(facultyId, studentId);
            if (assignment == null)
            {
                return ServiceResult<bool>.CreateFailure("Assignment not found");
            }

            // Use helper methods to maintain bidirectional relationship
            if (assignment.Student != null)
            {
                assignment.Student.DeactivateFacultyAssignment(facultyId);
            }
            
            if (assignment.Faculty != null)
            {
                assignment.Faculty.DeactivateStudentAssignment(studentId);
            }

            var success = await _assignmentRepository.DeactivateAsync(facultyId, studentId);
            if (success)
            {
                return ServiceResult<bool>.CreateSuccess(true, "Assignment deactivated successfully");
            }
            else
            {
                return ServiceResult<bool>.CreateFailure("Assignment not found or already deactivated");
            }
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.CreateFailure($"Failed to deactivate assignment: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> ActivateAssignmentAsync(Guid facultyId, Guid studentId)
    {
        try
        {
            // Get the assignment with navigation properties
            var assignment = await _assignmentRepository.GetByFacultyAndStudentAsync(facultyId, studentId);
            if (assignment == null)
            {
                return ServiceResult<bool>.CreateFailure("Assignment not found");
            }

            // Use helper methods to maintain bidirectional relationship
            if (assignment.Student != null)
            {
                assignment.Student.ActivateFacultyAssignment(facultyId);
            }
            
            if (assignment.Faculty != null)
            {
                assignment.Faculty.ActivateStudentAssignment(studentId);
            }

            var success = await _assignmentRepository.ActivateAsync(facultyId, studentId);
            if (success)
            {
                return ServiceResult<bool>.CreateSuccess(true, "Assignment activated successfully");
            }
            else
            {
                return ServiceResult<bool>.CreateFailure("Assignment not found or already active");
            }
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.CreateFailure($"Failed to activate assignment: {ex.Message}");
        }
    }

    public async Task<ServiceResult<FacultyStudentAssignmentDto?>> GetAssignmentAsync(Guid facultyId, Guid studentId)
    {
        try
        {
            var assignment = await _assignmentRepository.GetByFacultyAndStudentAsync(facultyId, studentId);
            if (assignment == null)
            {
                return ServiceResult<FacultyStudentAssignmentDto?>.CreateSuccess(null, "Assignment not found");
            }

            var dto = await MapToAssignmentDtoAsync(assignment);
            return ServiceResult<FacultyStudentAssignmentDto?>.CreateSuccess(dto);
        }
        catch (Exception ex)
        {
            return ServiceResult<FacultyStudentAssignmentDto?>.CreateFailure($"Failed to get assignment: {ex.Message}");
        }
    }

    public async Task<ServiceResult<List<FacultyStudentAssignmentDto>>> GetFacultyAssignmentsAsync(Guid facultyId)
    {
        try
        {
            var assignments = await _assignmentRepository.GetByFacultyIdAsync(facultyId);
            var dtos = new List<FacultyStudentAssignmentDto>();

            foreach (var assignment in assignments)
            {
                var dto = await MapToAssignmentDtoAsync(assignment);
                dtos.Add(dto);
            }

            return ServiceResult<List<FacultyStudentAssignmentDto>>.CreateSuccess(dtos);
        }
        catch (Exception ex)
        {
            return ServiceResult<List<FacultyStudentAssignmentDto>>.CreateFailure($"Failed to get faculty assignments: {ex.Message}");
        }
    }

    public async Task<ServiceResult<List<FacultyStudentAssignmentDto>>> GetStudentAssignmentsAsync(Guid studentId)
    {
        try
        {
            var assignments = await _assignmentRepository.GetByStudentIdAsync(studentId);
            var dtos = new List<FacultyStudentAssignmentDto>();

            foreach (var assignment in assignments)
            {
                var dto = await MapToAssignmentDtoAsync(assignment);
                dtos.Add(dto);
            }

            return ServiceResult<List<FacultyStudentAssignmentDto>>.CreateSuccess(dtos);
        }
        catch (Exception ex)
        {
            return ServiceResult<List<FacultyStudentAssignmentDto>>.CreateFailure($"Failed to get student assignments: {ex.Message}");
        }
    }

    public async Task<ServiceResult<(List<FacultyStudentAssignmentDto> Assignments, int TotalCount)>> GetAssignmentsAsync(FacultyStudentAssignmentFilterDto filter)
    {
        try
        {
            var (assignments, totalCount) = await _assignmentRepository.GetAssignmentsAsync(filter);
            var dtos = new List<FacultyStudentAssignmentDto>();

            foreach (var assignment in assignments)
            {
                var dto = await MapToAssignmentDtoAsync(assignment);
                dtos.Add(dto);
            }

            return ServiceResult<(List<FacultyStudentAssignmentDto> Assignments, int TotalCount)>.CreateSuccess((dtos, totalCount));
        }
        catch (Exception ex)
        {
            return ServiceResult<(List<FacultyStudentAssignmentDto> Assignments, int TotalCount)>.CreateFailure($"Failed to get assignments: {ex.Message}");
        }
    }

    public async Task<ServiceResult<FacultyDashboardDto>> GetFacultyDashboardAsync(Guid facultyId)
    {
        try
        {
            // Get faculty information
            var faculty = await _facultyRepository.GetByIdAsync(facultyId);
            if (faculty == null)
            {
                return ServiceResult<FacultyDashboardDto>.CreateFailure("Faculty not found");
            }

            // Get assignments
            var assignments = await _assignmentRepository.GetByFacultyIdAsync(facultyId);
            var activeAssignments = assignments.Where(a => a.IsActive).ToList();

            // Map to DTOs
            var assignedStudents = new List<AssignedStudentDto>();
            foreach (var assignment in activeAssignments)
            {
                var studentDto = new AssignedStudentDto
                {
                    StudentId = assignment.StudentId,
                    StudentName = assignment.Student?.FullName ?? "Unknown",
                    StudentEmail = assignment.Student?.Email ?? "",
                    StudentRollNumber = assignment.Student?.RollNumber ?? "",
                    StudentGrade = assignment.Student?.Grade,
                    StudentSection = assignment.Student?.Section,
                    AssignedDate = assignment.AssignedDate,
                    IsActive = assignment.IsActive,
                    Notes = assignment.Notes
                };
                assignedStudents.Add(studentDto);
            }

            var dashboard = new FacultyDashboardDto
            {
                Faculty = new FacultyDto
                {
                    Id = faculty.Id,
                    FirstName = faculty.FirstName,
                    LastName = faculty.LastName,
                    Email = faculty.Email,
                    PhoneNumber = faculty.PhoneNumber,
                    DateOfBirth = faculty.DateOfBirth,
                    Address = faculty.Address,
                    Gender = faculty.Gender,
                    Department = faculty.Department,
                    Subject = faculty.Subject,
                    EmployeeId = faculty.EmployeeId,
                    HireDate = faculty.HireDate,
                    IsActive = faculty.IsActive
                },
                TotalAssignedStudents = assignments.Count,
                ActiveAssignments = activeAssignments.Count,
                AssignedStudents = assignedStudents
            };

            return ServiceResult<FacultyDashboardDto>.CreateSuccess(dashboard);
        }
        catch (Exception ex)
        {
            return ServiceResult<FacultyDashboardDto>.CreateFailure($"Failed to get faculty dashboard: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> IsStudentAssignedToFacultyAsync(Guid facultyId, Guid studentId)
    {
        try
        {
            var exists = await _assignmentRepository.ExistsAsync(facultyId, studentId);
            return ServiceResult<bool>.CreateSuccess(exists);
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.CreateFailure($"Failed to check assignment: {ex.Message}");
        }
    }

    public async Task<ServiceResult<int>> GetFacultyActiveAssignmentCountAsync(Guid facultyId)
    {
        try
        {
            var count = await _assignmentRepository.GetActiveAssignmentCountAsync(facultyId);
            return ServiceResult<int>.CreateSuccess(count);
        }
        catch (Exception ex)
        {
            return ServiceResult<int>.CreateFailure($"Failed to get active assignment count: {ex.Message}");
        }
    }

    private async Task<FacultyStudentAssignmentDto> MapToAssignmentDtoAsync(StudentFaculty assignment)
    {
        // Get faculty and student details
        var faculty = await _facultyRepository.GetByIdAsync(assignment.FacultyId);
        var student = await _studentRepository.GetByIdAsync(assignment.StudentId);

        return new FacultyStudentAssignmentDto
        {
            FacultyId = assignment.FacultyId,
            FacultyName = faculty?.FullName ?? "Unknown",
            FacultyEmail = faculty?.Email ?? "",
            FacultyDepartment = faculty?.Department ?? "",
            FacultySubject = faculty?.Subject ?? "",
            StudentId = assignment.StudentId,
            StudentName = student?.FullName ?? "Unknown",
            StudentEmail = student?.Email ?? "",
            StudentRollNumber = student?.RollNumber ?? "",
            StudentGrade = student?.Grade,
            StudentSection = student?.Section,
            AssignedDate = assignment.AssignedDate,
            IsActive = assignment.IsActive,
            Notes = assignment.Notes
        };
    }
}
