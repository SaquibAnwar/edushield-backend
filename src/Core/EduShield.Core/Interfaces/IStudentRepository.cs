using EduShield.Core.Entities;
using EduShield.Core.Enums;

namespace EduShield.Core.Interfaces;

public interface IStudentRepository
{
    Task<Student?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Student?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Student?> GetByRollNumberAsync(string rollNumber, CancellationToken cancellationToken = default);
    Task<Student?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Student>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Student>> GetByFacultyIdAsync(Guid facultyId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Student>> GetByParentIdAsync(Guid parentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Student>> GetByStatusAsync(StudentStatus status, CancellationToken cancellationToken = default);
    Task<Student> CreateAsync(Student student, CancellationToken cancellationToken = default);
    Task<Student> UpdateAsync(Student student, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> RollNumberExistsAsync(string rollNumber, CancellationToken cancellationToken = default);
    Task<string> GenerateNextRollNumberAsync(CancellationToken cancellationToken = default);
}
