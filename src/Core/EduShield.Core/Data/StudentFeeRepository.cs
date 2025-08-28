using EduShield.Core.Entities;
using EduShield.Core.Enums;
using EduShield.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EduShield.Core.Data;

/// <summary>
/// Repository implementation for student fee data access
/// </summary>
public class StudentFeeRepository : IStudentFeeRepository
{
    private readonly EduShieldDbContext _context;

    public StudentFeeRepository(EduShieldDbContext context)
    {
        _context = context;
    }

    public async Task<StudentFee?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.StudentFees
            .Include(sf => sf.Student)
            .FirstOrDefaultAsync(sf => sf.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<StudentFee>> GetByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        return await _context.StudentFees
            .Include(sf => sf.Student)
            .Where(sf => sf.StudentId == studentId)
            .OrderByDescending(sf => sf.DueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<StudentFee>> GetByFeeTypeAsync(FeeType feeType, CancellationToken cancellationToken = default)
    {
        return await _context.StudentFees
            .Include(sf => sf.Student)
            .Where(sf => sf.FeeType == feeType)
            .OrderByDescending(sf => sf.DueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<StudentFee>> GetByTermAsync(string term, CancellationToken cancellationToken = default)
    {
        return await _context.StudentFees
            .Include(sf => sf.Student)
            .Where(sf => sf.Term == term)
            .OrderByDescending(sf => sf.DueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<StudentFee>> GetByPaymentStatusAsync(PaymentStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.StudentFees
            .Include(sf => sf.Student)
            .Where(sf => sf.PaymentStatus == status)
            .OrderByDescending(sf => sf.DueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<StudentFee>> GetOverdueAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.Today;
        return await _context.StudentFees
            .Include(sf => sf.Student)
            .Where(sf => sf.DueDate < today && sf.PaymentStatus != PaymentStatus.Paid)
            .OrderByDescending(sf => sf.DueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<StudentFee>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _context.StudentFees
            .Include(sf => sf.Student)
            .Where(sf => sf.DueDate >= startDate && sf.DueDate <= endDate)
            .OrderByDescending(sf => sf.DueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<StudentFee>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.StudentFees
            .Include(sf => sf.Student)
            .OrderByDescending(sf => sf.DueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<StudentFee>> GetByFacultyIdAsync(Guid facultyId, CancellationToken cancellationToken = default)
    {
        return await _context.StudentFees
            .Include(sf => sf.Student)
                .ThenInclude(s => s.StudentFaculties)
                    .ThenInclude(sf => sf.Faculty)
            .Where(sf => sf.Student.StudentFaculties.Any(sfa => sfa.FacultyId == facultyId && sfa.IsActive))
            .OrderByDescending(sf => sf.DueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<StudentFee>> GetByParentIdAsync(Guid parentId, CancellationToken cancellationToken = default)
    {
        return await _context.StudentFees
            .Include(sf => sf.Student)
            .Where(sf => sf.Student.ParentId == parentId)
            .OrderByDescending(sf => sf.DueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<StudentFee> CreateAsync(StudentFee fee, CancellationToken cancellationToken = default)
    {
        _context.StudentFees.Add(fee);
        await _context.SaveChangesAsync(cancellationToken);
        return fee;
    }

    public async Task<StudentFee> UpdateAsync(StudentFee fee, CancellationToken cancellationToken = default)
    {
        _context.StudentFees.Update(fee);
        await _context.SaveChangesAsync(cancellationToken);
        return fee;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var fee = await _context.StudentFees.FindAsync(new object[] { id }, cancellationToken);
        if (fee != null)
        {
            _context.StudentFees.Remove(fee);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.StudentFees.AnyAsync(sf => sf.Id == id, cancellationToken);
    }

    public async Task<object> GetStudentFeeStatisticsAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        var fees = await _context.StudentFees
            .Where(sf => sf.StudentId == studentId)
            .ToListAsync(cancellationToken);

        if (!fees.Any())
        {
            return new
            {
                TotalFees = 0,
                TotalAmount = 0m,
                TotalPaid = 0m,
                TotalDue = 0m,
                OverdueFees = 0,
                PaidFees = 0,
                PendingFees = 0
            };
        }

        return new
        {
            TotalFees = fees.Count,
            TotalAmount = fees.Sum(f => f.TotalAmount),
            TotalPaid = fees.Sum(f => f.AmountPaid),
            TotalDue = fees.Sum(f => f.AmountDue),
            OverdueFees = fees.Count(f => f.IsOverdue),
            PaidFees = fees.Count(f => f.PaymentStatus == PaymentStatus.Paid),
            PendingFees = fees.Count(f => f.PaymentStatus == PaymentStatus.Pending)
        };
    }
}