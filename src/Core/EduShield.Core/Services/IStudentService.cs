using EduShield.Core.Dtos;
using EduShield.Core.Enums;

namespace EduShield.Core.Services;

public interface IStudentService
{
    Task<StudentDto> CreateAsync(CreateStudentRequest request, CancellationToken cancellationToken = default);
    Task<StudentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<StudentDto?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<StudentDto?> GetByRollNumberAsync(string rollNumber, CancellationToken cancellationToken = default);
    Task<IEnumerable<StudentDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<StudentDto>> GetAllAsync(StudentFilters filters, CancellationToken cancellationToken = default);
    Task<PaginatedResponse<StudentDto>> GetPaginatedAsync(StudentFilters filters, CancellationToken cancellationToken = default);
    Task<IEnumerable<StudentDto>> GetByFacultyIdAsync(Guid facultyId, CancellationToken cancellationToken = default);
    Task<IEnumerable<StudentDto>> GetByParentIdAsync(Guid parentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<StudentDto>> GetByStatusAsync(StudentStatus status, CancellationToken cancellationToken = default);
    Task<StudentDto> UpdateAsync(Guid id, UpdateStudentRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> RollNumberExistsAsync(string rollNumber, CancellationToken cancellationToken = default);
}
