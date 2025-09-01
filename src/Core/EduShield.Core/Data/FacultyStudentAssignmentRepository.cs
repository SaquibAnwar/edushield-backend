using Microsoft.EntityFrameworkCore;
using EduShield.Core.Dtos;
using EduShield.Core.Entities;
using EduShield.Core.Interfaces;

namespace EduShield.Core.Data;

/// <summary>
/// Repository implementation for managing faculty-student assignments
/// </summary>
public class FacultyStudentAssignmentRepository : IFacultyStudentAssignmentRepository
{
    private readonly EduShieldDbContext _context;

    public FacultyStudentAssignmentRepository(EduShieldDbContext context)
    {
        _context = context;
    }

    public async Task<StudentFaculty> CreateAsync(StudentFaculty assignment)
    {
        // Simply add the assignment - Entity Framework will handle the relationships
        _context.StudentFaculties.Add(assignment);
        await _context.SaveChangesAsync();
        return assignment;
    }

    public async Task<StudentFaculty?> GetByFacultyAndStudentAsync(Guid facultyId, Guid studentId)
    {
        return await _context.StudentFaculties
            .Include(sf => sf.Faculty)
            .Include(sf => sf.Student)
            .FirstOrDefaultAsync(sf => sf.FacultyId == facultyId && sf.StudentId == studentId);
    }

    public async Task<List<StudentFaculty>> GetByFacultyIdAsync(Guid facultyId)
    {
        return await _context.StudentFaculties
            .Include(sf => sf.Student)
            .Where(sf => sf.FacultyId == facultyId)
            .OrderBy(sf => sf.Student!.FirstName)
            .ThenBy(sf => sf.Student!.LastName)
            .ToListAsync();
    }

    public async Task<List<StudentFaculty>> GetByStudentIdAsync(Guid studentId)
    {
        return await _context.StudentFaculties
            .Include(sf => sf.Faculty)
            .Where(sf => sf.StudentId == studentId)
            .OrderBy(sf => sf.Faculty!.FirstName)
            .ThenBy(sf => sf.Faculty!.LastName)
            .ToListAsync();
    }

    public async Task<(List<StudentFaculty> Assignments, int TotalCount)> GetAssignmentsAsync(FacultyStudentAssignmentFilterDto filter)
    {
        var query = _context.StudentFaculties
            .Include(sf => sf.Faculty)
            .Include(sf => sf.Student)
            .AsQueryable();

        // Apply filters
        if (filter.FacultyId.HasValue)
        {
            query = query.Where(sf => sf.FacultyId == filter.FacultyId.Value);
        }

        if (filter.StudentId.HasValue)
        {
            query = query.Where(sf => sf.StudentId == filter.StudentId.Value);
        }

        if (filter.IsActive.HasValue)
        {
            query = query.Where(sf => sf.IsActive == filter.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Department))
        {
            query = query.Where(sf => sf.Faculty!.Department.Contains(filter.Department));
        }

        if (!string.IsNullOrWhiteSpace(filter.Grade))
        {
            query = query.Where(sf => sf.Student!.Grade == filter.Grade);
        }

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var searchTerm = filter.SearchTerm.ToLower();
            query = query.Where(sf => 
                sf.Faculty!.FirstName.ToLower().Contains(searchTerm) ||
                sf.Faculty!.LastName.ToLower().Contains(searchTerm) ||
                sf.Faculty!.Email.ToLower().Contains(searchTerm) ||
                sf.Student!.FirstName.ToLower().Contains(searchTerm) ||
                sf.Student!.LastName.ToLower().Contains(searchTerm) ||
                sf.Student!.Email.ToLower().Contains(searchTerm) ||
                sf.Student!.RollNumber.ToLower().Contains(searchTerm)
            );
        }

        // Get total count
        var totalCount = await query.CountAsync();

        // Apply pagination and ordering
        var assignments = await query
            .OrderBy(sf => sf.Faculty!.FirstName)
            .ThenBy(sf => sf.Faculty!.LastName)
            .ThenBy(sf => sf.Student!.FirstName)
            .ThenBy(sf => sf.Student!.LastName)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return (assignments, totalCount);
    }

    public async Task<StudentFaculty> UpdateAsync(StudentFaculty assignment)
    {
        // Simply update the assignment - Entity Framework will handle the relationships
        _context.StudentFaculties.Update(assignment);
        await _context.SaveChangesAsync();
        return assignment;
    }

    public async Task<bool> DeactivateAsync(Guid facultyId, Guid studentId)
    {
        var assignment = await _context.StudentFaculties
            .FirstOrDefaultAsync(sf => sf.FacultyId == facultyId && sf.StudentId == studentId);

        if (assignment == null)
            return false;

        assignment.IsActive = false;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ActivateAsync(Guid facultyId, Guid studentId)
    {
        var assignment = await _context.StudentFaculties
            .FirstOrDefaultAsync(sf => sf.FacultyId == facultyId && sf.StudentId == studentId);

        if (assignment == null)
            return false;

        assignment.IsActive = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(Guid facultyId, Guid studentId)
    {
        return await _context.StudentFaculties
            .AnyAsync(sf => sf.FacultyId == facultyId && sf.StudentId == studentId);
    }

    public async Task<int> GetActiveAssignmentCountAsync(Guid facultyId)
    {
        return await _context.StudentFaculties
            .CountAsync(sf => sf.FacultyId == facultyId && sf.IsActive);
    }

    public async Task<int> GetStudentActiveAssignmentCountAsync(Guid studentId)
    {
        return await _context.StudentFaculties
            .CountAsync(sf => sf.StudentId == studentId && sf.IsActive);
    }

    public async Task<List<StudentFaculty>> GetAllAsync()
    {
        return await _context.StudentFaculties
            .Include(sf => sf.Faculty)
            .Include(sf => sf.Student)
            .OrderBy(sf => sf.Faculty!.FirstName)
            .ThenBy(sf => sf.Faculty!.LastName)
            .ThenBy(sf => sf.Student!.FirstName)
            .ThenBy(sf => sf.Student!.LastName)
            .ToListAsync();
    }
}
