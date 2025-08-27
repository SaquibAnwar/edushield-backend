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
            .Include(f => f.Student)
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }
    
    public async Task<IEnumerable<StudentFee>> GetByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        return await _context.StudentFees
            .Include(f => f.Student)
            .Where(f => f.StudentId == studentId)
            .OrderBy(f => f.DueDate)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<IEnumerable<StudentFee>> GetByFeeTypeAsync(FeeType feeType, CancellationToken cancellationToken = default)
    {
        return await _context.StudentFees
            .Include(f => f.Student)
            .Where(f => f.FeeType == feeType)
            .OrderBy(f => f.DueDate)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<IEnumerable<StudentFee>> GetByTermAsync(string term, CancellationToken cancellationToken = default)
    {
        return await _context.StudentFees
            .Include(f => f.Student)
            .Where(f => f.Term == term)
            .OrderBy(f => f.DueDate)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<IEnumerable<StudentFee>> GetByPaymentStatusAsync(PaymentStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.StudentFees
            .Include(f => f.Student)
            .Where(f => f.PaymentStatus == status)
            .OrderBy(f => f.DueDate)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<IEnumerable<StudentFee>> GetOverdueAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.Today;
        
        return await _context.StudentFees
            .Include(f => f.Student)
            .Where(f => f.DueDate < today && f.PaymentStatus != PaymentStatus.Paid)
            .OrderBy(f => f.DueDate)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<IEnumerable<StudentFee>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _context.StudentFees
            .Include(f => f.Student)
            .Where(f => f.DueDate >= startDate && f.DueDate <= endDate)
            .OrderBy(f => f.DueDate)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<IEnumerable<StudentFee>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.StudentFees
            .Include(f => f.Student)
            .OrderBy(f => f.DueDate)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<IEnumerable<StudentFee>> GetByFacultyIdAsync(Guid facultyId, CancellationToken cancellationToken = default)
    {
        return await _context.StudentFees
            .Include(f => f.Student)
            .ThenInclude(s => s.StudentFaculties)
            .Where(f => f.Student.StudentFaculties.Any(sf => sf.FacultyId == facultyId && sf.IsActive))
            .OrderBy(f => f.DueDate)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<IEnumerable<StudentFee>> GetByParentIdAsync(Guid parentId, CancellationToken cancellationToken = default)
    {
        return await _context.StudentFees
            .Include(f => f.Student)
            .Where(f => f.Student.ParentId == parentId)
            .OrderBy(f => f.DueDate)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<StudentFee> CreateAsync(StudentFee fee, CancellationToken cancellationToken = default)
    {
        _context.StudentFees.Add(fee);
        await _context.SaveChangesAsync(cancellationToken);
        
        // Reload the fee with Student navigation property included
        return await _context.StudentFees
            .Include(f => f.Student)
            .FirstAsync(f => f.Id == fee.Id, cancellationToken);
    }
    
    public async Task<StudentFee> UpdateAsync(StudentFee fee, CancellationToken cancellationToken = default)
    {
        _context.StudentFees.Update(fee);
        await _context.SaveChangesAsync(cancellationToken);
        
        // Reload the fee with Student navigation property included
        return await _context.StudentFees
            .Include(f => f.Student)
            .FirstAsync(f => f.Id == fee.Id, cancellationToken);
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
        return await _context.StudentFees.AnyAsync(f => f.Id == id, cancellationToken);
    }
    
    public async Task<object> GetStudentFeeStatisticsAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        var fees = await _context.StudentFees
            .Where(f => f.StudentId == studentId)
            .ToListAsync(cancellationToken);
        
        var totalFees = fees.Count;
        var paidFees = fees.Count(f => f.PaymentStatus == PaymentStatus.Paid);
        var pendingFees = fees.Count(f => f.PaymentStatus == PaymentStatus.Pending);
        var partialFees = fees.Count(f => f.PaymentStatus == PaymentStatus.Partial);
        var overdueFees = fees.Count(f => f.PaymentStatus == PaymentStatus.Overdue);
        
        // Note: These calculations would need to be done in the service layer after decryption
        // For now, we'll return 0 for these values
        var totalAmount = 0m;
        var totalPaid = 0m;
        var totalDue = 0m;
        var totalFines = 0m;
        
        return new
        {
            TotalFees = totalFees,
            PaidFees = paidFees,
            PendingFees = pendingFees,
            PartialFees = partialFees,
            OverdueFees = overdueFees,
            TotalAmount = totalAmount,
            TotalPaid = totalPaid,
            TotalDue = totalDue,
            TotalFines = totalFines,
            PaymentRate = totalFees > 0 ? (decimal)paidFees / totalFees * 100 : 0
        };
    }
}
