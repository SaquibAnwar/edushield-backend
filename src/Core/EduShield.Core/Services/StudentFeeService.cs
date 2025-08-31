using EduShield.Core.Dtos;
using EduShield.Core.Entities;
using EduShield.Core.Enums;
using EduShield.Core.Interfaces;
using EduShield.Core.Security;
using Microsoft.Extensions.Logging;

namespace EduShield.Core.Services;

/// <summary>
/// Service implementation for student fee business operations
/// </summary>
public class StudentFeeService : IStudentFeeService
{
    private readonly IStudentFeeRepository _feeRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEncryptionService _encryptionService;
    private readonly IFeeCalculatorService _feeCalculator;
    private readonly IPaymentService _paymentService;
    private readonly ILogger<StudentFeeService> _logger;
    
    public StudentFeeService(
        IStudentFeeRepository feeRepository,
        IStudentRepository studentRepository,
        IUserRepository userRepository,
        IEncryptionService encryptionService,
        IFeeCalculatorService feeCalculator,
        IPaymentService paymentService,
        ILogger<StudentFeeService> logger)
    {
        _feeRepository = feeRepository;
        _studentRepository = studentRepository;
        _userRepository = userRepository;
        _encryptionService = encryptionService;
        _feeCalculator = feeCalculator;
        _paymentService = paymentService;
        _logger = logger;
    }
    
    public async Task<StudentFeeDto> CreateAsync(CreateStudentFeeRequest request, CancellationToken cancellationToken = default)
    {
        // Validate student exists
        var student = await _studentRepository.GetByIdAsync(request.StudentId, cancellationToken);
        if (student == null)
        {
            throw new InvalidOperationException($"Student with ID '{request.StudentId}' not found.");
        }
        
        // Validate due date
        if (request.DueDate <= DateTime.Today)
        {
            throw new InvalidOperationException("Due date must be in the future.");
        }
        
        // Calculate initial fine amount (should be 0 for new fees)
        var fineAmount = _feeCalculator.CalculateLateFee(request.DueDate);
        
        var fee = new StudentFee
        {
            StudentId = request.StudentId,
            FeeType = request.FeeType,
            Term = request.Term,
            DueDate = request.DueDate,
            Notes = request.Notes,
            // Encrypt amounts before storing
            EncryptedTotalAmount = _encryptionService.EncryptDecimal(request.TotalAmount),
            EncryptedAmountPaid = _encryptionService.EncryptDecimal(0m),
            EncryptedAmountDue = _encryptionService.EncryptDecimal(request.TotalAmount),
            EncryptedFineAmount = _encryptionService.EncryptDecimal(fineAmount),
            PaymentStatus = PaymentStatus.Pending
        };
        
        var createdFee = await _feeRepository.CreateAsync(fee, cancellationToken);
        _logger.LogInformation("Fee record created successfully with ID: {FeeId}", createdFee.Id);
        
        return MapToDto(createdFee);
    }
    
    public async Task<StudentFeeDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var fee = await _feeRepository.GetByIdAsync(id, cancellationToken);
        return fee != null ? MapToDto(fee) : null;
    }
    
    public async Task<IEnumerable<StudentFeeDto>> GetByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        var fees = await _feeRepository.GetByStudentIdAsync(studentId, cancellationToken);
        return fees.Select(MapToDto);
    }

    public async Task<IEnumerable<StudentFeeDto>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // First get the student by user ID
        var student = await _studentRepository.GetByUserIdAsync(userId, cancellationToken);
        if (student == null)
        {
            return Enumerable.Empty<StudentFeeDto>();
        }
        
        // Then get the fees for that student
        var fees = await _feeRepository.GetByStudentIdAsync(student.Id, cancellationToken);
        return fees.Select(MapToDto);
    }
    
    public async Task<IEnumerable<StudentFeeDto>> GetByFeeTypeAsync(FeeType feeType, CancellationToken cancellationToken = default)
    {
        var fees = await _feeRepository.GetByFeeTypeAsync(feeType, cancellationToken);
        return fees.Select(MapToDto);
    }
    
    public async Task<IEnumerable<StudentFeeDto>> GetByTermAsync(string term, CancellationToken cancellationToken = default)
    {
        var fees = await _feeRepository.GetByTermAsync(term, cancellationToken);
        return fees.Select(MapToDto);
    }
    
    public async Task<IEnumerable<StudentFeeDto>> GetByPaymentStatusAsync(PaymentStatus status, CancellationToken cancellationToken = default)
    {
        var fees = await _feeRepository.GetByPaymentStatusAsync(status, cancellationToken);
        return fees.Select(MapToDto);
    }
    
    public async Task<IEnumerable<StudentFeeDto>> GetOverdueAsync(CancellationToken cancellationToken = default)
    {
        var fees = await _feeRepository.GetOverdueAsync(cancellationToken);
        return fees.Select(MapToDto);
    }
    
    public async Task<IEnumerable<StudentFeeDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var fees = await _feeRepository.GetByDateRangeAsync(startDate, endDate, cancellationToken);
        return fees.Select(MapToDto);
    }
    
    public async Task<IEnumerable<StudentFeeDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var fees = await _feeRepository.GetAllAsync(cancellationToken);
        return fees.Select(MapToDto);
    }
    
    public async Task<IEnumerable<StudentFeeDto>> GetByFacultyIdAsync(Guid facultyId, CancellationToken cancellationToken = default)
    {
        var fees = await _feeRepository.GetByFacultyIdAsync(facultyId, cancellationToken);
        return fees.Select(MapToDto);
    }
    
    public async Task<IEnumerable<StudentFeeDto>> GetByParentIdAsync(Guid parentId, CancellationToken cancellationToken = default)
    {
        var fees = await _feeRepository.GetByParentIdAsync(parentId, cancellationToken);
        return fees.Select(MapToDto);
    }
    
    public async Task<StudentFeeDto> UpdateAsync(Guid id, UpdateStudentFeeRequest request, CancellationToken cancellationToken = default)
    {
        var existingFee = await _feeRepository.GetByIdAsync(id, cancellationToken);
        if (existingFee == null)
        {
            throw new InvalidOperationException($"Fee record with ID '{id}' not found.");
        }
        
        // Update fields if provided
        if (request.FeeType.HasValue)
            existingFee.FeeType = request.FeeType.Value;
        
        if (!string.IsNullOrEmpty(request.Term))
            existingFee.Term = request.Term;
        
        if (request.TotalAmount.HasValue)
        {
            existingFee.EncryptedTotalAmount = _encryptionService.EncryptDecimal(request.TotalAmount.Value);
        }
        
        if (request.AmountPaid.HasValue)
        {
            existingFee.EncryptedAmountPaid = _encryptionService.EncryptDecimal(request.AmountPaid.Value);
        }
        
        if (request.DueDate.HasValue)
            existingFee.DueDate = request.DueDate.Value;
        
        if (request.Notes != null)
            existingFee.Notes = request.Notes;
        
        // Recalculate amounts and status
        await RecalculateFeeAmountsAsync(existingFee);
        
        var updatedFee = await _feeRepository.UpdateAsync(existingFee, cancellationToken);
        _logger.LogInformation("Fee record updated successfully with ID: {FeeId}", id);
        
        return MapToDto(updatedFee);
    }
    
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!await _feeRepository.ExistsAsync(id, cancellationToken))
        {
            throw new InvalidOperationException($"Fee record with ID '{id}' not found.");
        }
        
        await _feeRepository.DeleteAsync(id, cancellationToken);
        _logger.LogInformation("Fee record deleted successfully with ID: {FeeId}", id);
    }
    
    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _feeRepository.ExistsAsync(id, cancellationToken);
    }
    
    public async Task<PaymentResult> MakePaymentAsync(Guid id, PaymentRequest request, CancellationToken cancellationToken = default)
    {
        var fee = await _feeRepository.GetByIdAsync(id, cancellationToken);
        if (fee == null)
        {
            throw new InvalidOperationException($"Fee record with ID '{id}' not found.");
        }
        
        // Validate payment amount
        if (request.Amount <= 0)
        {
            throw new InvalidOperationException("Payment amount must be greater than zero.");
        }
        
        var currentAmountDue = _encryptionService.DecryptDecimal(fee.EncryptedAmountDue);
        if (request.Amount > currentAmountDue)
        {
            throw new InvalidOperationException("Payment amount cannot exceed the amount due.");
        }
        
        // Process payment through payment service
        var paymentResult = await _paymentService.ProcessPaymentAsync(
            request.Amount, 
            "INR", 
            $"Payment for {fee.FeeType} fee - {fee.Term}",
            new Dictionary<string, string>
            {
                { "feeId", fee.Id.ToString() },
                { "studentId", fee.StudentId.ToString() },
                { "feeType", fee.FeeType.ToString() },
                { "term", fee.Term }
            });
        
        if (!paymentResult.Success)
        {
            return new PaymentResult
            {
                Success = false,
                ErrorMessage = paymentResult.ErrorMessage ?? "Payment processing failed",
                PaymentDate = DateTime.UtcNow
            };
        }
        
        // Update fee record with payment
        var currentAmountPaid = _encryptionService.DecryptDecimal(fee.EncryptedAmountPaid);
        fee.EncryptedAmountPaid = _encryptionService.EncryptDecimal(currentAmountPaid + request.Amount);
        fee.LastPaymentDate = DateTime.UtcNow;
        
        // Recalculate amounts and status
        await RecalculateFeeAmountsAsync(fee);
        
        // Update the fee record
        var updatedFee = await _feeRepository.UpdateAsync(fee, cancellationToken);
        
        _logger.LogInformation("Payment processed successfully for fee {FeeId}: {Amount}", id, request.Amount);
        
        var newAmountDue = _encryptionService.DecryptDecimal(updatedFee.EncryptedAmountDue);
        return new PaymentResult
        {
            Success = true,
            TransactionId = paymentResult.TransactionId,
            AmountPaid = request.Amount,
            NewAmountDue = newAmountDue,
            NewPaymentStatus = updatedFee.PaymentStatus.ToString(),
            PaymentDate = DateTime.UtcNow,
            UpdatedFee = MapToDto(updatedFee)
        };
    }
    
    public async Task<object> GetStudentFeeStatisticsAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        return await _feeRepository.GetStudentFeeStatisticsAsync(studentId, cancellationToken);
    }
    
    public async Task<int> CalculateLateFeesAsync(CancellationToken cancellationToken = default)
    {
        var overdueFees = await _feeRepository.GetOverdueAsync(cancellationToken);
        var updatedCount = 0;
        
        foreach (var fee in overdueFees)
        {
            var newFineAmount = _feeCalculator.CalculateLateFee(fee.DueDate);
            
            var currentFineAmount = _encryptionService.DecryptDecimal(fee.EncryptedFineAmount);
            if (newFineAmount != currentFineAmount)
            {
                fee.EncryptedFineAmount = _encryptionService.EncryptDecimal(newFineAmount);
                
                await RecalculateFeeAmountsAsync(fee);
                await _feeRepository.UpdateAsync(fee, cancellationToken);
                updatedCount++;
            }
        }
        
        _logger.LogInformation("Updated late fees for {Count} overdue fee records", updatedCount);
        return updatedCount;
    }

    public async Task<PaginatedResponse<StudentFeeDto>> GetPaginatedAsync(StudentFeeFilterRequest filter, CancellationToken cancellationToken = default)
    {
        // Validate and sanitize pagination parameters
        filter.Validate();

        // Get all fees first (we'll implement repository-level pagination later)
        var allFees = await _feeRepository.GetAllAsync(cancellationToken);
        
        // Apply filters
        var filteredFees = allFees.AsQueryable();

        if (filter.StudentId.HasValue)
        {
            filteredFees = filteredFees.Where(f => f.StudentId == filter.StudentId.Value);
        }

        if (filter.FeeType.HasValue)
        {
            filteredFees = filteredFees.Where(f => f.FeeType == filter.FeeType.Value);
        }

        if (filter.PaymentStatus.HasValue)
        {
            filteredFees = filteredFees.Where(f => f.PaymentStatus == filter.PaymentStatus.Value);
        }

        if (!string.IsNullOrEmpty(filter.Term))
        {
            filteredFees = filteredFees.Where(f => f.Term.Contains(filter.Term, StringComparison.OrdinalIgnoreCase));
        }

        if (filter.IsOverdue.HasValue)
        {
            if (filter.IsOverdue.Value)
            {
                filteredFees = filteredFees.Where(f => f.DueDate < DateTime.Today && f.PaymentStatus != PaymentStatus.Paid);
            }
            else
            {
                filteredFees = filteredFees.Where(f => f.DueDate >= DateTime.Today || f.PaymentStatus == PaymentStatus.Paid);
            }
        }

        if (filter.FromDate.HasValue)
        {
            filteredFees = filteredFees.Where(f => f.DueDate >= filter.FromDate.Value);
        }

        if (filter.ToDate.HasValue)
        {
            filteredFees = filteredFees.Where(f => f.DueDate <= filter.ToDate.Value);
        }

        if (!string.IsNullOrEmpty(filter.Search))
        {
            filteredFees = filteredFees.Where(f => 
                f.Term.Contains(filter.Search, StringComparison.OrdinalIgnoreCase) ||
                f.FeeType.ToString().Contains(filter.Search, StringComparison.OrdinalIgnoreCase) ||
                (f.Student != null && (f.Student.FirstName.Contains(filter.Search, StringComparison.OrdinalIgnoreCase) ||
                                     f.Student.LastName.Contains(filter.Search, StringComparison.OrdinalIgnoreCase) ||
                                     f.Student.RollNumber.Contains(filter.Search, StringComparison.OrdinalIgnoreCase))));
        }

        // Apply sorting
        if (!string.IsNullOrEmpty(filter.SortBy))
        {
            var isDescending = filter.SortOrder?.ToLower() == "desc";
            
            filteredFees = filter.SortBy.ToLower() switch
            {
                "feetype" => isDescending ? filteredFees.OrderByDescending(f => f.FeeType) : filteredFees.OrderBy(f => f.FeeType),
                "term" => isDescending ? filteredFees.OrderByDescending(f => f.Term) : filteredFees.OrderBy(f => f.Term),
                "duedate" => isDescending ? filteredFees.OrderByDescending(f => f.DueDate) : filteredFees.OrderBy(f => f.DueDate),
                "paymentstatus" => isDescending ? filteredFees.OrderByDescending(f => f.PaymentStatus) : filteredFees.OrderBy(f => f.PaymentStatus),
                "student" => isDescending ? filteredFees.OrderByDescending(f => f.Student != null ? f.Student.FirstName : "") : filteredFees.OrderBy(f => f.Student != null ? f.Student.FirstName : ""),
                _ => isDescending ? filteredFees.OrderByDescending(f => f.CreatedAt) : filteredFees.OrderBy(f => f.CreatedAt)
            };
        }
        else
        {
            // Default sorting by due date descending
            filteredFees = filteredFees.OrderByDescending(f => f.DueDate);
        }

        // Get total count
        var totalCount = filteredFees.Count();

        // Apply pagination
        var pagedFees = filteredFees
            .Skip(filter.Skip)
            .Take(filter.Limit)
            .ToList();

        // Map to DTOs
        var feeDtos = pagedFees.Select(MapToDto).ToList();

        return PaginatedResponse<StudentFeeDto>.Create(
            feeDtos,
            totalCount,
            filter.Page,
            filter.Limit
        );
    }
    
    private Task RecalculateFeeAmountsAsync(StudentFee fee)
    {
        // Calculate fine amount based on current due date
        var fineAmount = _feeCalculator.CalculateLateFee(fee.DueDate);
        fee.EncryptedFineAmount = _encryptionService.EncryptDecimal(fineAmount);
        
        // Calculate amount due
        var totalAmount = _encryptionService.DecryptDecimal(fee.EncryptedTotalAmount);
        var amountPaid = _encryptionService.DecryptDecimal(fee.EncryptedAmountPaid);
        var amountDue = _feeCalculator.CalculateAmountDue(totalAmount, amountPaid, fineAmount);
        fee.EncryptedAmountDue = _encryptionService.EncryptDecimal(amountDue);
        
        // Determine payment status
        fee.PaymentStatus = _feeCalculator.DeterminePaymentStatus(totalAmount, amountPaid, fineAmount);
        
        return Task.CompletedTask;
    }
    
    private StudentFeeDto MapToDto(StudentFee fee)
    {
        // Debug: Log the actual values being loaded
        _logger.LogDebug("Mapping StudentFee to DTO - Id: {Id}, StudentId: {StudentId}", 
            fee.Id, fee.StudentId);
        
        // Decrypt encrypted fields and populate computed properties
        var totalAmount = _encryptionService.DecryptDecimal(fee.EncryptedTotalAmount);
        var amountPaid = _encryptionService.DecryptDecimal(fee.EncryptedAmountPaid);
        var amountDue = _encryptionService.DecryptDecimal(fee.EncryptedAmountDue);
        var fineAmount = _encryptionService.DecryptDecimal(fee.EncryptedFineAmount);
        
        // Debug: Log the decrypted values
        _logger.LogDebug("Decrypted values - TotalAmount: {TotalAmount}, AmountPaid: {AmountPaid}, AmountDue: {AmountDue}, FineAmount: {FineAmount}", 
            totalAmount, amountPaid, amountDue, fineAmount);
        
        return new StudentFeeDto
        {
            Id = fee.Id,
            StudentId = fee.StudentId, // This should be the actual StudentId from the database
            StudentFirstName = fee.Student?.FirstName ?? "Unknown",
            StudentLastName = fee.Student?.LastName ?? "Unknown",
            StudentRollNumber = fee.Student?.RollNumber ?? "Unknown",
            FeeType = fee.FeeType,
            Term = fee.Term,
            TotalAmount = totalAmount,
            AmountPaid = amountPaid,
            AmountDue = amountDue,
            PaymentStatus = fee.PaymentStatus,
            DueDate = fee.DueDate,
            LastPaymentDate = fee.LastPaymentDate,
            FineAmount = fineAmount,
            Notes = fee.Notes,
            CreatedAt = fee.CreatedAt,
            UpdatedAt = fee.UpdatedAt
        };
    }
}
