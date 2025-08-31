using EduShield.Core.Dtos;
using EduShield.Core.Entities;
using EduShield.Core.Enums;
using EduShield.Core.Interfaces;
using EduShield.Core.Security;

namespace EduShield.Core.Services;

/// <summary>
/// Service implementation for student performance business operations
/// </summary>
public class StudentPerformanceService : IStudentPerformanceService
{
    private readonly IStudentPerformanceRepository _performanceRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEncryptionService _encryptionService;

    public StudentPerformanceService(
        IStudentPerformanceRepository performanceRepository,
        IStudentRepository studentRepository,
        IUserRepository userRepository,
        IEncryptionService encryptionService)
    {
        _performanceRepository = performanceRepository;
        _studentRepository = studentRepository;
        _userRepository = userRepository;
        _encryptionService = encryptionService;
    }

    public async Task<StudentPerformanceDto> CreateAsync(CreateStudentPerformanceRequest request, CancellationToken cancellationToken = default)
    {
        // Validate student exists
        var student = await _studentRepository.GetByIdAsync(request.StudentId, cancellationToken);
        if (student == null)
        {
            throw new InvalidOperationException($"Student with ID '{request.StudentId}' not found.");
        }

        // Validate exam date
        if (request.ExamDate > DateTime.Today)
        {
            throw new InvalidOperationException("Exam date cannot be in the future.");
        }

        // Validate score
        if (request.Score < 0)
        {
            throw new InvalidOperationException("Score cannot be negative.");
        }

        if (request.MaxScore.HasValue && request.Score > request.MaxScore.Value)
        {
            throw new InvalidOperationException("Score cannot exceed maximum score.");
        }

        var performance = new StudentPerformance
        {
            StudentId = request.StudentId,
            Subject = request.Subject,
            ExamType = request.ExamType,
            ExamDate = request.ExamDate,
            MaxScore = request.MaxScore,
            ExamTitle = request.ExamTitle,
            Comments = request.Comments,
            // Encrypt the score before storing
            EncryptedScore = _encryptionService.EncryptDecimal(request.Score)
        };

        var createdPerformance = await _performanceRepository.CreateAsync(performance, cancellationToken);
        return MapToDto(createdPerformance);
    }

    public async Task<StudentPerformanceDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var performance = await _performanceRepository.GetByIdAsync(id, cancellationToken);
        return performance != null ? MapToDto(performance) : null;
    }

    public async Task<IEnumerable<StudentPerformanceDto>> GetByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        var performances = await _performanceRepository.GetByStudentIdAsync(studentId, cancellationToken);
        return performances.Select(MapToDto);
    }

    public async Task<IEnumerable<StudentPerformanceDto>> GetBySubjectAsync(string subject, CancellationToken cancellationToken = default)
    {
        var performances = await _performanceRepository.GetBySubjectAsync(subject, cancellationToken);
        return performances.Select(MapToDto);
    }

    public async Task<IEnumerable<StudentPerformanceDto>> GetByExamTypeAsync(ExamType examType, CancellationToken cancellationToken = default)
    {
        var performances = await _performanceRepository.GetByExamTypeAsync(examType, cancellationToken);
        return performances.Select(MapToDto);
    }

    public async Task<IEnumerable<StudentPerformanceDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var performances = await _performanceRepository.GetByDateRangeAsync(startDate, endDate, cancellationToken);
        return performances.Select(MapToDto);
    }

    public async Task<IEnumerable<StudentPerformanceDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var performances = await _performanceRepository.GetAllAsync(cancellationToken);
        return performances.Select(MapToDto);
    }

    public async Task<IEnumerable<StudentPerformanceDto>> GetByFacultyIdAsync(Guid facultyId, CancellationToken cancellationToken = default)
    {
        var performances = await _performanceRepository.GetByFacultyIdAsync(facultyId, cancellationToken);
        return performances.Select(MapToDto);
    }

    public async Task<StudentPerformanceDto> UpdateAsync(Guid id, UpdateStudentPerformanceRequest request, CancellationToken cancellationToken = default)
    {
        var existingPerformance = await _performanceRepository.GetByIdAsync(id, cancellationToken);
        if (existingPerformance == null)
        {
            throw new InvalidOperationException($"Performance record with ID '{id}' not found.");
        }

        // Validate exam date if changing
        if (request.ExamDate.HasValue && request.ExamDate.Value > DateTime.Today)
        {
            throw new InvalidOperationException("Exam date cannot be in the future.");
        }

        // Validate score if changing
        if (request.Score.HasValue)
        {
            if (request.Score.Value < 0)
            {
                throw new InvalidOperationException("Score cannot be negative.");
            }

            var maxScore = request.MaxScore ?? existingPerformance.MaxScore;
            if (maxScore.HasValue && request.Score.Value > maxScore.Value)
            {
                throw new InvalidOperationException("Score cannot exceed maximum score.");
            }

            // Encrypt the new score
            existingPerformance.EncryptedScore = _encryptionService.EncryptDecimal(request.Score.Value);
        }

        // Update other properties if provided
        if (request.Subject != null) existingPerformance.Subject = request.Subject;
        if (request.ExamType.HasValue) existingPerformance.ExamType = request.ExamType.Value;
        if (request.ExamDate.HasValue) existingPerformance.ExamDate = request.ExamDate.Value;
        if (request.MaxScore.HasValue) existingPerformance.MaxScore = request.MaxScore.Value;
        if (request.ExamTitle != null) existingPerformance.ExamTitle = request.ExamTitle;
        if (request.Comments != null) existingPerformance.Comments = request.Comments;

        var updatedPerformance = await _performanceRepository.UpdateAsync(existingPerformance, cancellationToken);
        return MapToDto(updatedPerformance);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!await _performanceRepository.ExistsAsync(id, cancellationToken))
        {
            throw new InvalidOperationException($"Performance record with ID '{id}' not found.");
        }

        await _performanceRepository.DeleteAsync(id, cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _performanceRepository.ExistsAsync(id, cancellationToken);
    }

    public async Task<object> GetStudentStatisticsAsync(Guid studentId, string? subject = null, CancellationToken cancellationToken = default)
    {
        // Validate student exists
        if (!await _studentRepository.ExistsAsync(studentId, cancellationToken))
        {
            throw new InvalidOperationException($"Student with ID '{studentId}' not found.");
        }

        return await _performanceRepository.GetStudentStatisticsAsync(studentId, subject, cancellationToken);
    }

    public async Task<PaginatedResponse<StudentPerformanceDto>> GetPaginatedAsync(StudentPerformanceFilterRequest filter, CancellationToken cancellationToken = default)
    {
        // Validate and sanitize pagination parameters
        filter.Validate();

        // Get all performances first (we'll implement repository-level pagination later)
        var allPerformances = await _performanceRepository.GetAllAsync(cancellationToken);
        
        // Apply filters
        var filteredPerformances = allPerformances.AsQueryable();

        if (filter.StudentId.HasValue)
        {
            filteredPerformances = filteredPerformances.Where(p => p.StudentId == filter.StudentId.Value);
        }

        if (!string.IsNullOrEmpty(filter.Subject))
        {
            filteredPerformances = filteredPerformances.Where(p => p.Subject.Contains(filter.Subject, StringComparison.OrdinalIgnoreCase));
        }

        if (filter.ExamType.HasValue)
        {
            filteredPerformances = filteredPerformances.Where(p => p.ExamType == filter.ExamType.Value);
        }

        if (filter.FromDate.HasValue)
        {
            filteredPerformances = filteredPerformances.Where(p => p.ExamDate >= filter.FromDate.Value);
        }

        if (filter.ToDate.HasValue)
        {
            filteredPerformances = filteredPerformances.Where(p => p.ExamDate <= filter.ToDate.Value);
        }

        if (!string.IsNullOrEmpty(filter.Search))
        {
            filteredPerformances = filteredPerformances.Where(p => 
                p.Subject.Contains(filter.Search, StringComparison.OrdinalIgnoreCase) ||
                p.ExamTitle.Contains(filter.Search, StringComparison.OrdinalIgnoreCase) ||
                (p.Student != null && (p.Student.FirstName.Contains(filter.Search, StringComparison.OrdinalIgnoreCase) ||
                                     p.Student.LastName.Contains(filter.Search, StringComparison.OrdinalIgnoreCase))));
        }

        // Apply sorting
        if (!string.IsNullOrEmpty(filter.SortBy))
        {
            var isDescending = filter.SortOrder?.ToLower() == "desc";
            
            filteredPerformances = filter.SortBy.ToLower() switch
            {
                "subject" => isDescending ? filteredPerformances.OrderByDescending(p => p.Subject) : filteredPerformances.OrderBy(p => p.Subject),
                "examdate" => isDescending ? filteredPerformances.OrderByDescending(p => p.ExamDate) : filteredPerformances.OrderBy(p => p.ExamDate),
                "examtype" => isDescending ? filteredPerformances.OrderByDescending(p => p.ExamType) : filteredPerformances.OrderBy(p => p.ExamType),
                "student" => isDescending ? filteredPerformances.OrderByDescending(p => p.Student != null ? p.Student.FirstName : "") : filteredPerformances.OrderBy(p => p.Student != null ? p.Student.FirstName : ""),
                _ => isDescending ? filteredPerformances.OrderByDescending(p => p.CreatedAt) : filteredPerformances.OrderBy(p => p.CreatedAt)
            };
        }
        else
        {
            // Default sorting by exam date descending
            filteredPerformances = filteredPerformances.OrderByDescending(p => p.ExamDate);
        }

        // Get total count
        var totalCount = filteredPerformances.Count();

        // Apply pagination
        var pagedPerformances = filteredPerformances
            .Skip(filter.Skip)
            .Take(filter.Limit)
            .ToList();

        // Map to DTOs
        var performanceDtos = pagedPerformances.Select(MapToDto).ToList();

        return PaginatedResponse<StudentPerformanceDto>.Create(
            performanceDtos,
            totalCount,
            filter.Page,
            filter.Limit
        );
    }

    private StudentPerformanceDto MapToDto(StudentPerformance performance)
    {
        // Decrypt the score for the DTO
        var decryptedScore = _encryptionService.DecryptDecimal(performance.EncryptedScore);
        
        return new StudentPerformanceDto
        {
            Id = performance.Id,
            StudentId = performance.StudentId,
            StudentFirstName = performance.Student?.FirstName ?? string.Empty,
            StudentLastName = performance.Student?.LastName ?? string.Empty,
            Subject = performance.Subject,
            ExamType = performance.ExamType,
            ExamDate = performance.ExamDate,
            Score = decryptedScore,
            MaxScore = performance.MaxScore,
            ExamTitle = performance.ExamTitle,
            Comments = performance.Comments,
            CreatedAt = performance.CreatedAt,
            UpdatedAt = performance.UpdatedAt
        };
    }
}
