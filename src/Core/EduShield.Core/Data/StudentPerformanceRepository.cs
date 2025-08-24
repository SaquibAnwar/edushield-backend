using EduShield.Core.Entities;
using EduShield.Core.Enums;
using EduShield.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EduShield.Core.Data;

/// <summary>
/// Repository implementation for student performance data operations
/// </summary>
public class StudentPerformanceRepository : IStudentPerformanceRepository
{
    private readonly EduShieldDbContext _context;

    public StudentPerformanceRepository(EduShieldDbContext context)
    {
        _context = context;
    }

    public async Task<StudentPerformance?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.StudentPerformances
            .Include(sp => sp.Student)
            .FirstOrDefaultAsync(sp => sp.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<StudentPerformance>> GetByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        return await _context.StudentPerformances
            .Include(sp => sp.Student)
            .Where(sp => sp.StudentId == studentId)
            .OrderByDescending(sp => sp.ExamDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<StudentPerformance>> GetBySubjectAsync(string subject, CancellationToken cancellationToken = default)
    {
        return await _context.StudentPerformances
            .Include(sp => sp.Student)
            .Where(sp => sp.Subject.ToLower() == subject.ToLower())
            .OrderByDescending(sp => sp.ExamDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<StudentPerformance>> GetByExamTypeAsync(ExamType examType, CancellationToken cancellationToken = default)
    {
        return await _context.StudentPerformances
            .Include(sp => sp.Student)
            .Where(sp => sp.ExamType == examType)
            .OrderByDescending(sp => sp.ExamDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<StudentPerformance>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _context.StudentPerformances
            .Include(sp => sp.Student)
            .Where(sp => sp.ExamDate >= startDate && sp.ExamDate <= endDate)
            .OrderByDescending(sp => sp.ExamDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<StudentPerformance>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.StudentPerformances
            .Include(sp => sp.Student)
            .OrderByDescending(sp => sp.ExamDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<StudentPerformance>> GetByFacultyIdAsync(Guid facultyId, CancellationToken cancellationToken = default)
    {
        // Get performance records for students assigned to the faculty
        return await _context.StudentPerformances
            .Include(sp => sp.Student)
                .ThenInclude(s => s.StudentFaculties)
                    .ThenInclude(sf => sf.Faculty)
            .Where(sp => sp.Student.StudentFaculties.Any(sf => sf.FacultyId == facultyId && sf.IsActive))
            .OrderByDescending(sp => sp.ExamDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<StudentPerformance> CreateAsync(StudentPerformance performance, CancellationToken cancellationToken = default)
    {
        _context.StudentPerformances.Add(performance);
        await _context.SaveChangesAsync(cancellationToken);
        return performance;
    }

    public async Task<StudentPerformance> UpdateAsync(StudentPerformance performance, CancellationToken cancellationToken = default)
    {
        _context.StudentPerformances.Update(performance);
        await _context.SaveChangesAsync(cancellationToken);
        return performance;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var performance = await _context.StudentPerformances.FindAsync(new object[] { id }, cancellationToken);
        if (performance != null)
        {
            _context.StudentPerformances.Remove(performance);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.StudentPerformances.AnyAsync(sp => sp.Id == id, cancellationToken);
    }

    public async Task<object> GetStudentStatisticsAsync(Guid studentId, string? subject = null, CancellationToken cancellationToken = default)
    {
        var query = _context.StudentPerformances.Where(sp => sp.StudentId == studentId);
        
        if (!string.IsNullOrEmpty(subject))
        {
            query = query.Where(sp => sp.Subject.ToLower() == subject.ToLower());
        }

        var performances = await query.ToListAsync(cancellationToken);
        
        if (!performances.Any())
        {
            return new
            {
                TotalExams = 0,
                AverageScore = 0m,
                HighestScore = 0m,
                LowestScore = 0m,
                SubjectBreakdown = new object[] { }
            };
        }

        var totalExams = performances.Count;
        var averageScore = performances.Average(sp => sp.Score);
        var highestScore = performances.Max(sp => sp.Score);
        var lowestScore = performances.Min(sp => sp.Score);

        var subjectBreakdown = performances
            .GroupBy(sp => sp.Subject)
            .Select(g => new
            {
                Subject = g.Key,
                Count = g.Count(),
                AverageScore = g.Average(sp => sp.Score),
                HighestScore = g.Max(sp => sp.Score),
                LowestScore = g.Min(sp => sp.Score)
            })
            .OrderBy(x => x.Subject)
            .ToArray();

        return new
        {
            TotalExams = totalExams,
            AverageScore = Math.Round(averageScore, 2),
            HighestScore = highestScore,
            LowestScore = lowestScore,
            SubjectBreakdown = subjectBreakdown
        };
    }
}
