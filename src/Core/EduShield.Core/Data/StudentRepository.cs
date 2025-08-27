using EduShield.Core.Entities;
using EduShield.Core.Enums;
using EduShield.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EduShield.Core.Data;

public class StudentRepository : IStudentRepository
{
    private readonly EduShieldDbContext _context;

    public StudentRepository(EduShieldDbContext context)
    {
        _context = context;
    }

    public async Task<Student?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Students
            .Include(s => s.User)
            .Include(s => s.Parent)
            .Include(s => s.StudentFaculties)
                .ThenInclude(sf => sf.Faculty)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<Student?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Students
            .Include(s => s.User)
            .Include(s => s.Parent)
            .Include(s => s.StudentFaculties)
                .ThenInclude(sf => sf.Faculty)
            .FirstOrDefaultAsync(s => s.Email == email, cancellationToken);
    }

    public async Task<Student?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Students
            .Include(s => s.User)
            .Include(s => s.Parent)
            .Include(s => s.StudentFaculties)
                .ThenInclude(sf => sf.Faculty)
            .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);
    }

    public async Task<Student?> GetByRollNumberAsync(string rollNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Students
            .Include(s => s.User)
            .Include(s => s.Parent)
            .Include(s => s.StudentFaculties)
                .ThenInclude(sf => sf.Faculty)
            .FirstOrDefaultAsync(s => s.RollNumber == rollNumber, cancellationToken);
    }

    public async Task<IEnumerable<Student>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Students
            .Include(s => s.User)
            .Include(s => s.Parent)
            .Include(s => s.StudentFaculties)
                .ThenInclude(sf => sf.Faculty)
            .OrderBy(s => s.RollNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Student>> GetByFacultyIdAsync(Guid facultyId, CancellationToken cancellationToken = default)
    {
        return await _context.Students
            .Include(s => s.User)
            .Include(s => s.Parent)
            .Include(s => s.StudentFaculties)
                .ThenInclude(sf => sf.Faculty)
            .Where(s => s.StudentFaculties.Any(sf => sf.FacultyId == facultyId && sf.IsActive))
            .OrderBy(s => s.RollNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Student>> GetByParentIdAsync(Guid parentId, CancellationToken cancellationToken = default)
    {
        return await _context.Students
            .Include(s => s.User)
            .Include(s => s.Parent)
            .Include(s => s.StudentFaculties)
                .ThenInclude(sf => sf.Faculty)
            .Where(s => s.ParentId == parentId)
            .OrderBy(s => s.RollNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Student>> GetByStatusAsync(StudentStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Students
            .Include(s => s.User)
            .Include(s => s.Parent)
            .Include(s => s.StudentFaculties)
                .ThenInclude(sf => sf.Faculty)
            .Where(s => s.Status == status)
            .OrderBy(s => s.RollNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task<Student> CreateAsync(Student student, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(student.RollNumber))
        {
            student.RollNumber = await GenerateNextRollNumberAsync(cancellationToken);
        }

        _context.Students.Add(student);
        await _context.SaveChangesAsync(cancellationToken);
        return student;
    }

    public async Task<Student> UpdateAsync(Student student, CancellationToken cancellationToken = default)
    {
        _context.Students.Update(student);
        await _context.SaveChangesAsync(cancellationToken);
        return student;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var student = await _context.Students.FindAsync(new object[] { id }, cancellationToken);
        if (student != null)
        {
            _context.Students.Remove(student);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Students.AnyAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Students.AnyAsync(s => s.Email == email, cancellationToken);
    }

    public async Task<bool> RollNumberExistsAsync(string rollNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Students.AnyAsync(s => s.RollNumber == rollNumber, cancellationToken);
    }

    public async Task<string> GenerateNextRollNumberAsync(CancellationToken cancellationToken = default)
    {
        var lastStudent = await _context.Students
            .Where(s => s.RollNumber.StartsWith("student_"))
            .OrderByDescending(s => s.RollNumber)
            .FirstOrDefaultAsync(cancellationToken);

        if (lastStudent == null)
        {
            return "student_1";
        }

        if (int.TryParse(lastStudent.RollNumber.Replace("student_", ""), out int lastNumber))
        {
            return $"student_{lastNumber + 1}";
        }

        return "student_1";
    }
}
