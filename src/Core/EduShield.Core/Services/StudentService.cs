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
    private readonly ICacheService _cacheService;
    private const int CACHE_EXPIRATION_MINUTES = 15;

    public StudentService(IStudentRepository studentRepository, IUserRepository userRepository, ICacheService cacheService)
    {
        _studentRepository = studentRepository;
        _userRepository = userRepository;
        _cacheService = cacheService;
    }

    public async Task<StudentDto> CreateAsync(CreateStudentRequest request, CancellationToken cancellationToken = default)
    {
        // Validate email uniqueness in both User and Student tables
        if (await _userRepository.ExistsAsync(request.Email))
        {
            throw new InvalidOperationException($"User with email '{request.Email}' already exists.");
        }
        
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

        // Create User record for authentication
        var user = new Entities.User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            Name = $"{request.FirstName} {request.LastName}".Trim(),
            Role = UserRole.Student,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Create and save the user first
        var createdUser = await _userRepository.CreateAsync(user);

        // Generate roll number
        var rollNumber = await GenerateRollNumberAsync(cancellationToken);

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
            Status = StudentStatus.Active,
            RollNumber = rollNumber,
            UserId = createdUser.Id // Link to the created user
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
        var studentDto = MapToDto(createdStudent);
        
        // Cache the newly created student
        var cacheKey = $"student_{createdStudent.Id}";
        await _cacheService.SetAsync(cacheKey, studentDto, TimeSpan.FromMinutes(CACHE_EXPIRATION_MINUTES), cancellationToken);
        
        return studentDto;
    }

    private async Task<string> GenerateRollNumberAsync(CancellationToken cancellationToken)
    {
        var existingStudents = await _studentRepository.GetAllAsync(cancellationToken);
        var maxRollNumber = existingStudents
            .Where(s => s.RollNumber.StartsWith("student_"))
            .Select(s => 
            {
                if (int.TryParse(s.RollNumber.Replace("student_", ""), out var num))
                    return num;
                return 0;
            })
            .DefaultIfEmpty(0)
            .Max();
        
        return $"student_{maxRollNumber + 1:D4}";
    }

    public async Task<StudentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"student_{id}";
        
        // Try to get from cache first
        var cachedStudent = await _cacheService.GetAsync<StudentDto>(cacheKey, cancellationToken);
        if (cachedStudent != null)
        {
            return cachedStudent;
        }

        // If not in cache, get from database
        var student = await _studentRepository.GetByIdAsync(id, cancellationToken);
        if (student == null)
        {
            return null;
        }

        var studentDto = MapToDto(student);
        
        // Cache the result
        await _cacheService.SetAsync(cacheKey, studentDto, TimeSpan.FromMinutes(CACHE_EXPIRATION_MINUTES), cancellationToken);
        
        return studentDto;
    }

    public async Task<StudentDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"student_email_{email.ToLowerInvariant()}";
        
        // Try to get from cache first
        var cachedStudent = await _cacheService.GetAsync<StudentDto>(cacheKey, cancellationToken);
        if (cachedStudent != null)
        {
            return cachedStudent;
        }

        // If not in cache, get from database
        var student = await _studentRepository.GetByEmailAsync(email, cancellationToken);
        if (student == null)
        {
            return null;
        }

        var studentDto = MapToDto(student);
        
        // Cache the result
        await _cacheService.SetAsync(cacheKey, studentDto, TimeSpan.FromMinutes(CACHE_EXPIRATION_MINUTES), cancellationToken);
        
        return studentDto;
    }

    public async Task<StudentDto?> GetByRollNumberAsync(string rollNumber, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"student_roll_{rollNumber.ToLowerInvariant()}";
        
        // Try to get from cache first
        var cachedStudent = await _cacheService.GetAsync<StudentDto>(cacheKey, cancellationToken);
        if (cachedStudent != null)
        {
            return cachedStudent;
        }

        // If not in cache, get from database
        var student = await _studentRepository.GetByRollNumberAsync(rollNumber, cancellationToken);
        if (student == null)
        {
            return null;
        }

        var studentDto = MapToDto(student);
        
        // Cache the result
        await _cacheService.SetAsync(cacheKey, studentDto, TimeSpan.FromMinutes(CACHE_EXPIRATION_MINUTES), cancellationToken);
        
        return studentDto;
    }

    public async Task<IEnumerable<StudentDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var students = await _studentRepository.GetAllAsync(cancellationToken);
        return students.Select(MapToDto);
    }

    public async Task<IEnumerable<StudentDto>> GetAllAsync(StudentFilters filters, CancellationToken cancellationToken = default)
    {
        var students = await _studentRepository.GetAllAsync(cancellationToken);
        var studentDtos = students.Select(MapToDto);

        // Apply filters
        if (!string.IsNullOrWhiteSpace(filters.Search))
        {
            var searchTerm = filters.Search.ToLowerInvariant();
            studentDtos = studentDtos.Where(s => 
                s.FirstName.ToLowerInvariant().Contains(searchTerm) ||
                s.LastName.ToLowerInvariant().Contains(searchTerm) ||
                s.FullName.ToLowerInvariant().Contains(searchTerm) ||
                s.Email.ToLowerInvariant().Contains(searchTerm) ||
                (!string.IsNullOrEmpty(s.RollNumber) && s.RollNumber.ToLowerInvariant().Contains(searchTerm)) ||
                (!string.IsNullOrEmpty(s.Grade) && s.Grade.ToLowerInvariant().Contains(searchTerm)) ||
                (!string.IsNullOrEmpty(s.Section) && s.Section.ToLowerInvariant().Contains(searchTerm))
            );
        }

        if (filters.Status.HasValue)
        {
            studentDtos = studentDtos.Where(s => s.Status == filters.Status.Value);
        }

        if (filters.Gender.HasValue)
        {
            studentDtos = studentDtos.Where(s => s.Gender == filters.Gender.Value);
        }

        if (!string.IsNullOrWhiteSpace(filters.Grade))
        {
            studentDtos = studentDtos.Where(s => 
                !string.IsNullOrEmpty(s.Grade) && 
                s.Grade.Equals(filters.Grade, StringComparison.OrdinalIgnoreCase)
            );
        }

        if (!string.IsNullOrWhiteSpace(filters.Section))
        {
            studentDtos = studentDtos.Where(s => 
                !string.IsNullOrEmpty(s.Section) && 
                s.Section.Equals(filters.Section, StringComparison.OrdinalIgnoreCase)
            );
        }

        if (filters.ParentId.HasValue)
        {
            studentDtos = studentDtos.Where(s => s.ParentId == filters.ParentId.Value);
        }

        if (filters.FacultyId.HasValue)
        {
            studentDtos = studentDtos.Where(s => 
                s.AssignedFaculties.Any(f => f.Id == filters.FacultyId.Value)
            );
        }

        if (filters.EnrollmentDateFrom.HasValue)
        {
            studentDtos = studentDtos.Where(s => s.EnrollmentDate >= filters.EnrollmentDateFrom.Value);
        }

        if (filters.EnrollmentDateTo.HasValue)
        {
            studentDtos = studentDtos.Where(s => s.EnrollmentDate <= filters.EnrollmentDateTo.Value);
        }

        if (filters.DateOfBirthFrom.HasValue)
        {
            studentDtos = studentDtos.Where(s => s.DateOfBirth >= filters.DateOfBirthFrom.Value);
        }

        if (filters.DateOfBirthTo.HasValue)
        {
            studentDtos = studentDtos.Where(s => s.DateOfBirth <= filters.DateOfBirthTo.Value);
        }

        if (filters.IsEnrolled.HasValue)
        {
            studentDtos = studentDtos.Where(s => s.IsEnrolled == filters.IsEnrolled.Value);
        }

        // Apply sorting
        if (!string.IsNullOrWhiteSpace(filters.SortBy))
        {
            var isDescending = filters.SortOrder?.ToLowerInvariant() == "desc";
            
            studentDtos = filters.SortBy.ToLowerInvariant() switch
            {
                "firstname" => isDescending ? studentDtos.OrderByDescending(s => s.FirstName) : studentDtos.OrderBy(s => s.FirstName),
                "lastname" => isDescending ? studentDtos.OrderByDescending(s => s.LastName) : studentDtos.OrderBy(s => s.LastName),
                "fullname" => isDescending ? studentDtos.OrderByDescending(s => s.FullName) : studentDtos.OrderBy(s => s.FullName),
                "email" => isDescending ? studentDtos.OrderByDescending(s => s.Email) : studentDtos.OrderBy(s => s.Email),
                "rollnumber" => isDescending ? studentDtos.OrderByDescending(s => s.RollNumber) : studentDtos.OrderBy(s => s.RollNumber),
                "grade" => isDescending ? studentDtos.OrderByDescending(s => s.Grade) : studentDtos.OrderBy(s => s.Grade),
                "section" => isDescending ? studentDtos.OrderByDescending(s => s.Section) : studentDtos.OrderBy(s => s.Section),
                "enrollmentdate" => isDescending ? studentDtos.OrderByDescending(s => s.EnrollmentDate) : studentDtos.OrderBy(s => s.EnrollmentDate),
                "dateofbirth" => isDescending ? studentDtos.OrderByDescending(s => s.DateOfBirth) : studentDtos.OrderBy(s => s.DateOfBirth),
                "status" => isDescending ? studentDtos.OrderByDescending(s => s.Status) : studentDtos.OrderBy(s => s.Status),
                "createdat" => isDescending ? studentDtos.OrderByDescending(s => s.CreatedAt) : studentDtos.OrderBy(s => s.CreatedAt),
                _ => studentDtos.OrderBy(s => s.FullName) // Default sort by full name
            };
        }
        else
        {
            studentDtos = studentDtos.OrderBy(s => s.FullName);
        }

        // Apply pagination
        if (filters.Page.HasValue && filters.Limit.HasValue)
        {
            var skip = (filters.Page.Value - 1) * filters.Limit.Value;
            studentDtos = studentDtos.Skip(skip).Take(filters.Limit.Value);
        }

        return studentDtos;
    }

    public async Task<PaginatedResponse<StudentDto>> GetPaginatedAsync(StudentFilters filters, CancellationToken cancellationToken = default)
    {
        var students = await _studentRepository.GetAllAsync(cancellationToken);
        var studentDtos = students.Select(MapToDto);

        // Apply filters (same logic as GetAllAsync)
        if (!string.IsNullOrWhiteSpace(filters.Search))
        {
            var searchTerm = filters.Search.ToLowerInvariant();
            studentDtos = studentDtos.Where(s => 
                s.FirstName.ToLowerInvariant().Contains(searchTerm) ||
                s.LastName.ToLowerInvariant().Contains(searchTerm) ||
                s.FullName.ToLowerInvariant().Contains(searchTerm) ||
                s.Email.ToLowerInvariant().Contains(searchTerm) ||
                (!string.IsNullOrEmpty(s.RollNumber) && s.RollNumber.ToLowerInvariant().Contains(searchTerm)) ||
                (!string.IsNullOrEmpty(s.Grade) && s.Grade.ToLowerInvariant().Contains(searchTerm)) ||
                (!string.IsNullOrEmpty(s.Section) && s.Section.ToLowerInvariant().Contains(searchTerm))
            );
        }

        if (filters.Status.HasValue)
        {
            studentDtos = studentDtos.Where(s => s.Status == filters.Status.Value);
        }

        if (filters.Gender.HasValue)
        {
            studentDtos = studentDtos.Where(s => s.Gender == filters.Gender.Value);
        }

        if (!string.IsNullOrWhiteSpace(filters.Grade))
        {
            studentDtos = studentDtos.Where(s => 
                !string.IsNullOrEmpty(s.Grade) && 
                s.Grade.Equals(filters.Grade, StringComparison.OrdinalIgnoreCase)
            );
        }

        if (!string.IsNullOrWhiteSpace(filters.Section))
        {
            studentDtos = studentDtos.Where(s => 
                !string.IsNullOrEmpty(s.Section) && 
                s.Section.Equals(filters.Section, StringComparison.OrdinalIgnoreCase)
            );
        }

        if (filters.ParentId.HasValue)
        {
            studentDtos = studentDtos.Where(s => s.ParentId == filters.ParentId.Value);
        }

        if (filters.FacultyId.HasValue)
        {
            studentDtos = studentDtos.Where(s => 
                s.AssignedFaculties.Any(f => f.Id == filters.FacultyId.Value)
            );
        }

        if (filters.EnrollmentDateFrom.HasValue)
        {
            studentDtos = studentDtos.Where(s => s.EnrollmentDate >= filters.EnrollmentDateFrom.Value);
        }

        if (filters.EnrollmentDateTo.HasValue)
        {
            studentDtos = studentDtos.Where(s => s.EnrollmentDate <= filters.EnrollmentDateTo.Value);
        }

        if (filters.DateOfBirthFrom.HasValue)
        {
            studentDtos = studentDtos.Where(s => s.DateOfBirth >= filters.DateOfBirthFrom.Value);
        }

        if (filters.DateOfBirthTo.HasValue)
        {
            studentDtos = studentDtos.Where(s => s.DateOfBirth <= filters.DateOfBirthTo.Value);
        }

        if (filters.IsEnrolled.HasValue)
        {
            studentDtos = studentDtos.Where(s => s.IsEnrolled == filters.IsEnrolled.Value);
        }

        // Apply sorting
        if (!string.IsNullOrWhiteSpace(filters.SortBy))
        {
            var isDescending = filters.SortOrder?.ToLowerInvariant() == "desc";
            
            studentDtos = filters.SortBy.ToLowerInvariant() switch
            {
                "firstname" => isDescending ? studentDtos.OrderByDescending(s => s.FirstName) : studentDtos.OrderBy(s => s.FirstName),
                "lastname" => isDescending ? studentDtos.OrderByDescending(s => s.LastName) : studentDtos.OrderBy(s => s.LastName),
                "fullname" => isDescending ? studentDtos.OrderByDescending(s => s.FullName) : studentDtos.OrderBy(s => s.FullName),
                "email" => isDescending ? studentDtos.OrderByDescending(s => s.Email) : studentDtos.OrderBy(s => s.Email),
                "rollnumber" => isDescending ? studentDtos.OrderByDescending(s => s.RollNumber) : studentDtos.OrderBy(s => s.RollNumber),
                "grade" => isDescending ? studentDtos.OrderByDescending(s => s.Grade) : studentDtos.OrderBy(s => s.Grade),
                "section" => isDescending ? studentDtos.OrderByDescending(s => s.Section) : studentDtos.OrderBy(s => s.Section),
                "enrollmentdate" => isDescending ? studentDtos.OrderByDescending(s => s.EnrollmentDate) : studentDtos.OrderBy(s => s.EnrollmentDate),
                "dateofbirth" => isDescending ? studentDtos.OrderByDescending(s => s.DateOfBirth) : studentDtos.OrderBy(s => s.DateOfBirth),
                "status" => isDescending ? studentDtos.OrderByDescending(s => s.Status) : studentDtos.OrderBy(s => s.Status),
                "createdat" => isDescending ? studentDtos.OrderByDescending(s => s.CreatedAt) : studentDtos.OrderBy(s => s.CreatedAt),
                _ => studentDtos.OrderBy(s => s.FullName) // Default sort by full name
            };
        }
        else
        {
            studentDtos = studentDtos.OrderBy(s => s.FullName);
        }

        // Get total count before pagination
        var totalCount = studentDtos.Count();

        // Apply pagination
        var page = filters.Page ?? 1;
        var pageSize = filters.Limit ?? 10;
        
        var skip = (page - 1) * pageSize;
        var paginatedData = studentDtos.Skip(skip).Take(pageSize);

        return new PaginatedResponse<StudentDto>(paginatedData, totalCount, page, pageSize);
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
        var studentDto = MapToDto(updatedStudent);
        
        // Invalidate and update cache
        await InvalidateStudentCacheAsync(updatedStudent, cancellationToken);
        
        // Cache the updated student
        var cacheKey = $"student_{updatedStudent.Id}";
        await _cacheService.SetAsync(cacheKey, studentDto, TimeSpan.FromMinutes(CACHE_EXPIRATION_MINUTES), cancellationToken);
        
        return studentDto;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Get student info before deletion for cache invalidation
        var student = await _studentRepository.GetByIdAsync(id, cancellationToken);
        
        await _studentRepository.DeleteAsync(id, cancellationToken);
        
        // Invalidate cache after deletion
        if (student != null)
        {
            await InvalidateStudentCacheAsync(student, cancellationToken);
        }
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

    private async Task InvalidateStudentCacheAsync(Student student, CancellationToken cancellationToken = default)
    {
        // Invalidate all possible cache keys for this student
        var cacheKeys = new[]
        {
            $"student_{student.Id}",
            $"student_email_{student.Email.ToLowerInvariant()}",
            $"student_roll_{student.RollNumber.ToLowerInvariant()}"
        };

        foreach (var key in cacheKeys)
        {
            await _cacheService.RemoveAsync(key, cancellationToken);
        }
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
