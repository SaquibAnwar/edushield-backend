using EduShield.Core.Dtos;
using EduShield.Core.Entities;
using EduShield.Core.Interfaces;

namespace EduShield.Core.Services;

/// <summary>
/// Service implementation for faculty business operations
/// </summary>
public class FacultyService : IFacultyService
{
    private readonly IFacultyRepository _facultyRepository;
    private readonly IUserRepository _userRepository;

    public FacultyService(IFacultyRepository facultyRepository, IUserRepository userRepository)
    {
        _facultyRepository = facultyRepository;
        _userRepository = userRepository;
    }

    public async Task<FacultyDto> CreateAsync(CreateFacultyRequest request, CancellationToken cancellationToken = default)
    {
        // Validate email uniqueness
        if (await _facultyRepository.EmailExistsAsync(request.Email, cancellationToken))
        {
            throw new InvalidOperationException($"Faculty with email '{request.Email}' already exists.");
        }

        // Validate hire date
        if (request.HireDate > DateTime.Today)
        {
            throw new InvalidOperationException("Hire date cannot be in the future.");
        }

        // Validate date of birth
        if (request.DateOfBirth > DateTime.Today)
        {
            throw new InvalidOperationException("Date of birth cannot be in the future.");
        }

        // Validate minimum age (18 years old)
        var minimumAge = DateTime.Today.AddYears(-18);
        if (request.DateOfBirth > minimumAge)
        {
            throw new InvalidOperationException("Faculty member must be at least 18 years old.");
        }

        // Validate user ID if provided
        if (request.UserId.HasValue)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId.Value);
            if (user == null)
            {
                throw new InvalidOperationException($"User with ID '{request.UserId.Value}' not found.");
            }
        }

        var faculty = new Faculty
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            DateOfBirth = request.DateOfBirth,
            Address = request.Address,
            Gender = request.Gender,
            Department = request.Department,
            Subject = request.Subject,
            HireDate = request.HireDate,
            UserId = request.UserId,
            IsActive = true
        };

        var createdFaculty = await _facultyRepository.CreateAsync(faculty, cancellationToken);
        return MapToDto(createdFaculty);
    }

    public async Task<FacultyDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var faculty = await _facultyRepository.GetByIdAsync(id, cancellationToken);
        return faculty != null ? MapToDto(faculty) : null;
    }

    public async Task<FacultyDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var faculty = await _facultyRepository.GetByEmailAsync(email, cancellationToken);
        return faculty != null ? MapToDto(faculty) : null;
    }

    public async Task<FacultyDto?> GetByEmployeeIdAsync(string employeeId, CancellationToken cancellationToken = default)
    {
        var faculty = await _facultyRepository.GetByEmployeeIdAsync(employeeId, cancellationToken);
        return faculty != null ? MapToDto(faculty) : null;
    }

    public async Task<IEnumerable<FacultyDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var faculty = await _facultyRepository.GetAllAsync(cancellationToken);
        return faculty.Select(MapToDto);
    }

    public async Task<IEnumerable<FacultyDto>> GetByDepartmentAsync(string department, CancellationToken cancellationToken = default)
    {
        var faculty = await _facultyRepository.GetByDepartmentAsync(department, cancellationToken);
        return faculty.Select(MapToDto);
    }

    public async Task<IEnumerable<FacultyDto>> GetBySubjectAsync(string subject, CancellationToken cancellationToken = default)
    {
        var faculty = await _facultyRepository.GetBySubjectAsync(subject, cancellationToken);
        return faculty.Select(MapToDto);
    }

    public async Task<IEnumerable<FacultyDto>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        var faculty = await _facultyRepository.GetActiveAsync(cancellationToken);
        return faculty.Select(MapToDto);
    }

    public async Task<FacultyDto> UpdateAsync(Guid id, UpdateFacultyRequest request, CancellationToken cancellationToken = default)
    {
        var existingFaculty = await _facultyRepository.GetByIdAsync(id, cancellationToken);
        if (existingFaculty == null)
        {
            throw new InvalidOperationException($"Faculty with ID '{id}' not found.");
        }

        // Validate email uniqueness if changing
        if (!string.IsNullOrEmpty(request.Email) && request.Email != existingFaculty.Email)
        {
            if (await _facultyRepository.EmailExistsAsync(request.Email, cancellationToken))
            {
                throw new InvalidOperationException($"Faculty with email '{request.Email}' already exists.");
            }
        }

        // Validate hire date if changing
        if (request.HireDate.HasValue && request.HireDate.Value > DateTime.Today)
        {
            throw new InvalidOperationException("Hire date cannot be in the future.");
        }

        // Validate date of birth if changing
        if (request.DateOfBirth.HasValue)
        {
            if (request.DateOfBirth.Value > DateTime.Today)
            {
                throw new InvalidOperationException("Date of birth cannot be in the future.");
            }

            // Validate minimum age (18 years old)
            var minimumAge = DateTime.Today.AddYears(-18);
            if (request.DateOfBirth.Value > minimumAge)
            {
                throw new InvalidOperationException("Faculty member must be at least 18 years old.");
            }
        }

        // Validate user ID if changing
        if (request.UserId.HasValue && request.UserId.Value != existingFaculty.UserId)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId.Value);
            if (user == null)
            {
                throw new InvalidOperationException($"User with ID '{request.UserId.Value}' not found.");
            }
        }

        // Update properties if provided
        if (request.FirstName != null) existingFaculty.FirstName = request.FirstName;
        if (request.LastName != null) existingFaculty.LastName = request.LastName;
        if (request.Email != null) existingFaculty.Email = request.Email;
        if (request.PhoneNumber != null) existingFaculty.PhoneNumber = request.PhoneNumber;
        if (request.DateOfBirth.HasValue) existingFaculty.DateOfBirth = request.DateOfBirth.Value;
        if (request.Address != null) existingFaculty.Address = request.Address;
        if (request.Gender.HasValue) existingFaculty.Gender = request.Gender.Value;
        if (request.Department != null) existingFaculty.Department = request.Department;
        if (request.Subject != null) existingFaculty.Subject = request.Subject;
        if (request.HireDate.HasValue) existingFaculty.HireDate = request.HireDate.Value;
        if (request.IsActive.HasValue) existingFaculty.IsActive = request.IsActive.Value;
        if (request.UserId.HasValue) existingFaculty.UserId = request.UserId;

        var updatedFaculty = await _facultyRepository.UpdateAsync(existingFaculty, cancellationToken);
        return MapToDto(updatedFaculty);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!await _facultyRepository.ExistsAsync(id, cancellationToken))
        {
            throw new InvalidOperationException($"Faculty with ID '{id}' not found.");
        }

        await _facultyRepository.DeleteAsync(id, cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _facultyRepository.ExistsAsync(id, cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _facultyRepository.EmailExistsAsync(email, cancellationToken);
    }

    public async Task<bool> EmployeeIdExistsAsync(string employeeId, CancellationToken cancellationToken = default)
    {
        return await _facultyRepository.EmployeeIdExistsAsync(employeeId, cancellationToken);
    }

    private static FacultyDto MapToDto(Faculty faculty)
    {
        // Debug: Log the StudentFaculties collection
        var assignedStudents = new List<AssignedStudentDto>();
        
        if (faculty.StudentFaculties != null && faculty.StudentFaculties.Any())
        {
            assignedStudents = faculty.StudentFaculties
                .Where(sf => sf.Student != null)
                .Select(sf => new AssignedStudentDto
                {
                    StudentId = sf.StudentId,
                    StudentName = sf.Student!.FullName,
                    StudentEmail = sf.Student.Email,
                    StudentRollNumber = sf.Student.RollNumber ?? string.Empty,
                    StudentGrade = sf.Student.Grade,
                    StudentSection = sf.Student.Section,
                    AssignedDate = sf.AssignedDate,
                    IsActive = sf.IsActive,
                    Notes = sf.Notes
                })
                .ToList();
        }

        return new FacultyDto
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
            IsActive = faculty.IsActive,
            UserId = faculty.UserId,
            CreatedAt = faculty.CreatedAt,
            UpdatedAt = faculty.UpdatedAt,
            AssignedStudents = assignedStudents
        };
    }
}
