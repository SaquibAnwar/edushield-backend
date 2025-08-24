using EduShield.Core.Entities;
using EduShield.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EduShield.Core.Data;

/// <summary>
/// Repository implementation for faculty data operations
/// </summary>
public class FacultyRepository : IFacultyRepository
{
    private readonly EduShieldDbContext _context;

    public FacultyRepository(EduShieldDbContext context)
    {
        _context = context;
    }

    public async Task<Faculty?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Faculty
            .Include(f => f.User)
            .Include(f => f.StudentFaculties)
                .ThenInclude(sf => sf.Student)
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public async Task<Faculty?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Faculty
            .Include(f => f.User)
            .Include(f => f.StudentFaculties)
                .ThenInclude(sf => sf.Student)
            .FirstOrDefaultAsync(f => f.Email.ToLower() == email.ToLower(), cancellationToken);
    }

    public async Task<Faculty?> GetByEmployeeIdAsync(string employeeId, CancellationToken cancellationToken = default)
    {
        return await _context.Faculty
            .Include(f => f.User)
            .Include(f => f.StudentFaculties)
                .ThenInclude(sf => sf.Student)
            .FirstOrDefaultAsync(f => f.EmployeeId == employeeId, cancellationToken);
    }

    public async Task<IEnumerable<Faculty>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Faculty
            .Include(f => f.User)
            .Include(f => f.StudentFaculties)
                .ThenInclude(sf => sf.Student)
            .OrderBy(f => f.EmployeeId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Faculty>> GetByDepartmentAsync(string department, CancellationToken cancellationToken = default)
    {
        return await _context.Faculty
            .Include(f => f.User)
            .Include(f => f.StudentFaculties)
                .ThenInclude(sf => sf.Student)
            .Where(f => f.Department.ToLower() == department.ToLower())
            .OrderBy(f => f.EmployeeId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Faculty>> GetBySubjectAsync(string subject, CancellationToken cancellationToken = default)
    {
        return await _context.Faculty
            .Include(f => f.User)
            .Include(f => f.StudentFaculties)
                .ThenInclude(sf => sf.Student)
            .Where(f => f.Subject.ToLower() == subject.ToLower())
            .OrderBy(f => f.EmployeeId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Faculty>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Faculty
            .Include(f => f.User)
            .Include(f => f.StudentFaculties)
                .ThenInclude(sf => sf.Student)
            .Where(f => f.IsActive)
            .OrderBy(f => f.EmployeeId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Faculty> CreateAsync(Faculty faculty, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(faculty.EmployeeId))
        {
            faculty.EmployeeId = await GenerateNextEmployeeIdAsync(cancellationToken);
        }

        _context.Faculty.Add(faculty);
        await _context.SaveChangesAsync(cancellationToken);
        return faculty;
    }

    public async Task<Faculty> UpdateAsync(Faculty faculty, CancellationToken cancellationToken = default)
    {
        _context.Faculty.Update(faculty);
        await _context.SaveChangesAsync(cancellationToken);
        return faculty;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var faculty = await _context.Faculty.FindAsync(new object[] { id }, cancellationToken);
        if (faculty != null)
        {
            _context.Faculty.Remove(faculty);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Faculty.AnyAsync(f => f.Id == id, cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Faculty.AnyAsync(f => f.Email.ToLower() == email.ToLower(), cancellationToken);
    }

    public async Task<bool> EmployeeIdExistsAsync(string employeeId, CancellationToken cancellationToken = default)
    {
        return await _context.Faculty.AnyAsync(f => f.EmployeeId == employeeId, cancellationToken);
    }

    public async Task<string> GenerateNextEmployeeIdAsync(CancellationToken cancellationToken = default)
    {
        var lastFaculty = await _context.Faculty
            .Where(f => f.EmployeeId != null && f.EmployeeId.StartsWith("faculty_"))
            .OrderByDescending(f => f.EmployeeId)
            .FirstOrDefaultAsync(cancellationToken);

        if (lastFaculty == null)
        {
            return "faculty_001";
        }

        if (lastFaculty.EmployeeId != null && int.TryParse(lastFaculty.EmployeeId.Replace("faculty_", ""), out int lastNumber))
        {
            return $"faculty_{(lastNumber + 1):D3}";
        }

        return "faculty_001";
    }
}
