using EduShield.Core.Dtos;
using EduShield.Core.Entities;
using EduShield.Core.Enums;
using EduShield.Core.Interfaces;
using EduShield.Core.Services;

namespace EduShield.Core.Services;

public class StudentService : IStudentService
{
    private readonly IStudentRepository _studentRepository;
    private readonly IUserRepository _userRepository;

    public StudentService(IStudentRepository studentRepository, IUserRepository userRepository)
    {
        _studentRepository = studentRepository;
        _userRepository = userRepository;
    }

    public async Task<StudentDto> CreateAsync(CreateStudentRequest request, CancellationToken cancellationToken = default)
    {
        // Validate email uniqueness
        if (await _studentRepository.EmailExistsAsync(request.Email, cancellationToken))
        {
            throw new InvalidOperationException($"Student with email '{request.Email}' already exists.");
        }

        // Validate enrollment date
        if (request.EnrollmentDate > DateTime.Today)
        {
            throw new InvalidOperationException("Enrollment date cannot be in the future.");
        }

        // Validate date of birth
        if (request.DateOfBirth > DateTime.Today)
        {
            throw new InvalidOperationException("Date of birth cannot be in the future.");
        }

        var student = new Student
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            DateOfBirth = request.DateOfBirth,
            Address = request.Address,
            Gender = request.Gender,
            EnrollmentDate = request.EnrollmentDate,
            Grade = request.Grade,
            Section = request.Section,
            ParentId = request.ParentId,
            Status = StudentStatus.Active
        };

        // Assign faculties if provided
        if (request.FacultyIds.Any())
        {
            foreach (var facultyId in request.FacultyIds)
            {
                student.AssignFaculty(facultyId);
            }
        }

        var createdStudent = await _studentRepository.CreateAsync(student, cancellationToken);
        return MapToDto(createdStudent);
    }

    public async Task<StudentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var student = await _studentRepository.GetByIdAsync(id, cancellationToken);
        return student != null ? MapToDto(student) : null;
    }

    public async Task<StudentDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var student = await _studentRepository.GetByEmailAsync(email, cancellationToken);
        return student != null ? MapToDto(student) : null;
    }

    public async Task<StudentDto?> GetByRollNumberAsync(string rollNumber, CancellationToken cancellationToken = default)
    {
        var student = await _studentRepository.GetByRollNumberAsync(rollNumber, cancellationToken);
        return student != null ? MapToDto(student) : null;
    }

    public async Task<IEnumerable<StudentDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var students = await _studentRepository.GetAllAsync(cancellationToken);
        return students.Select(MapToDto);
    }

    public async Task<IEnumerable<StudentDto>> GetByFacultyIdAsync(Guid facultyId, CancellationToken cancellationToken = default)
    {
        var students = await _studentRepository.GetByFacultyIdAsync(facultyId, cancellationToken);
        return students.Select(MapToDto);
    }

    public async Task<IEnumerable<StudentDto>> GetByParentIdAsync(Guid parentId, CancellationToken cancellationToken = default)
    {
        var students = await _studentRepository.GetByParentIdAsync(parentId, cancellationToken);
        return students.Select(MapToDto);
    }

    public async Task<IEnumerable<StudentDto>> GetByStatusAsync(StudentStatus status, CancellationToken cancellationToken = default)
    {
        var students = await _studentRepository.GetByStatusAsync(status, cancellationToken);
        return students.Select(MapToDto);
    }

    public async Task<StudentDto> UpdateAsync(Guid id, UpdateStudentRequest request, CancellationToken cancellationToken = default)
    {
        var existingStudent = await _studentRepository.GetByIdAsync(id, cancellationToken);
        if (existingStudent == null)
        {
            throw new InvalidOperationException($"Student with ID '{id}' not found.");
        }

        // Validate email uniqueness if changing
        if (!string.IsNullOrEmpty(request.Email) && request.Email != existingStudent.Email)
        {
            if (await _studentRepository.EmailExistsAsync(request.Email, cancellationToken))
            {
                throw new InvalidOperationException($"Student with email '{request.Email}' already exists.");
            }
        }

        // Update properties if provided
        if (request.FirstName != null) existingStudent.FirstName = request.FirstName;
        if (request.LastName != null) existingStudent.LastName = request.LastName;
        if (request.Email != null) existingStudent.Email = request.Email;
        if (request.PhoneNumber != null) existingStudent.PhoneNumber = request.PhoneNumber;
        if (request.DateOfBirth.HasValue) existingStudent.DateOfBirth = request.DateOfBirth.Value;
        if (request.Address != null) existingStudent.Address = request.Address;
        if (request.Gender.HasValue) existingStudent.Gender = request.Gender.Value;
        if (request.EnrollmentDate.HasValue) existingStudent.EnrollmentDate = request.EnrollmentDate.Value;
        if (request.Grade != null) existingStudent.Grade = request.Grade;
        if (request.Section != null) existingStudent.Section = request.Section;
        if (request.Status.HasValue) existingStudent.Status = request.Status.Value;
        if (request.ParentId.HasValue) existingStudent.ParentId = request.ParentId;

        // Update faculty assignments if provided
        if (request.FacultyIds != null)
        {
            // Clear existing assignments
            existingStudent.StudentFaculties.Clear();
            
            // Add new assignments
            foreach (var facultyId in request.FacultyIds)
            {
                existingStudent.AssignFaculty(facultyId);
            }
        }

        var updatedStudent = await _studentRepository.UpdateAsync(existingStudent, cancellationToken);
        return MapToDto(updatedStudent);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _studentRepository.DeleteAsync(id, cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _studentRepository.ExistsAsync(id, cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _studentRepository.EmailExistsAsync(email, cancellationToken);
    }

    public async Task<bool> RollNumberExistsAsync(string rollNumber, CancellationToken cancellationToken = default)
    {
        return await _studentRepository.RollNumberExistsAsync(rollNumber, cancellationToken);
    }

    private static StudentDto MapToDto(Student student)
    {
        return new StudentDto
        {
            Id = student.Id,
            FirstName = student.FirstName,
            LastName = student.LastName,
            Email = student.Email,
            PhoneNumber = student.PhoneNumber,
            DateOfBirth = student.DateOfBirth,
            Address = student.Address,
            Gender = student.Gender,
            RollNumber = student.RollNumber,
            EnrollmentDate = student.EnrollmentDate,
            Status = student.Status,
            Grade = student.Grade,
            Section = student.Section,
            UserId = student.UserId,
            ParentId = student.ParentId,
            CreatedAt = student.CreatedAt,
            UpdatedAt = student.UpdatedAt,
            AssignedFaculties = student.StudentFaculties
                .Where(sf => sf.IsActive && sf.Faculty != null)
                .Select(sf => new FacultyAssignmentDto
                {
                    Id = sf.Faculty!.Id,
                    FirstName = sf.Faculty.FirstName,
                    LastName = sf.Faculty.LastName,
                    Department = sf.Faculty.Department,
                    Subject = sf.Faculty.Subject,
                    AssignedDate = sf.AssignedDate
                })
                .ToList(),
            Parent = student.Parent != null ? new UserDto
            {
                Id = student.Parent.Id,
                Email = student.Parent.Email,
                Name = student.Parent.FullName,
                Role = UserRole.Parent,
                IsActive = student.Parent.IsActive,
                CreatedAt = student.Parent.CreatedAt,
                UpdatedAt = student.Parent.UpdatedAt
            } : null
        };
    }
}
