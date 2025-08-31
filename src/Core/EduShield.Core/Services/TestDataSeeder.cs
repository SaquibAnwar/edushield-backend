using EduShield.Core.Entities;
using EduShield.Core.Enums;
using EduShield.Core.Interfaces;
using EduShield.Core.Data;
using EduShield.Core.Security;
using System.Linq;

namespace EduShield.Core.Services;

public interface ITestDataSeeder
{
    Task SeedUsersAsync();
}

public class TestDataSeeder : ITestDataSeeder
{
    private readonly IUserRepository _userRepository;
    private readonly IStudentFeeRepository _feeRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IFacultyRepository _facultyRepository;
    private readonly IParentRepository _parentRepository; // Added _parentRepository
    private readonly IEncryptionService _encryptionService;

    public TestDataSeeder(
        IUserRepository userRepository,
        IStudentFeeRepository feeRepository,
        IStudentRepository studentRepository,
        IFacultyRepository facultyRepository,
        IParentRepository parentRepository, // Added _parentRepository
        IEncryptionService encryptionService)
    {
        _userRepository = userRepository;
        _feeRepository = feeRepository;
        _studentRepository = studentRepository;
        _facultyRepository = facultyRepository;
        _parentRepository = parentRepository; // Initialize _parentRepository
        _encryptionService = encryptionService;
    }

    public async Task SeedUsersAsync()
    {
        var users = new List<Entities.User>
        {
            new()
            {
                Email = "iamsaquibanwar@gmail.com",
                Name = "Saquib Admin",
                Role = UserRole.Admin,
                IsActive = true
            },
            new()
            {
                Email = "saquibanwar01@gmail.com",
                Name = "Saquib Student",
                Role = UserRole.Student,
                IsActive = true
            },
            new()
            {
                Email = "saquibedu@gmail.com",
                Name = "Saquib Faculty",
                Role = UserRole.Faculty,
                IsActive = true
            },
            new()
            {
                Email = "kirakrypto9ite@gmail.com",
                Name = "Saquib Parent",
                Role = UserRole.Parent,
                IsActive = true
            },
            new()
            {
                Email = "techtonicwave.business@gmail.com",
                Name = "Saquib Dev Admin",
                Role = UserRole.DevAuth,
                IsActive = true
            }
        };

        foreach (var user in users)
        {
            if (!await _userRepository.ExistsAsync(user.Email))
            {
                await _userRepository.CreateAsync(user);
            }
        }

        // Seed students for users with Student role
        await SeedStudentsAsync();
        
        // Seed faculty for users with Faculty role
        await SeedFacultyAsync();

        // Seed parents for users with Parent role
        await SeedParentsAsync();

        // Seed sample fee data
        await SeedSampleFeesAsync();
    }

    private async Task SeedStudentsAsync()
    {
        // Get all users and filter by role since GetByRoleAsync doesn't exist
        var allUsers = await _userRepository.GetAllAsync();
        var studentUsers = allUsers.Where(u => u.Role == UserRole.Student);
        
        foreach (var user in studentUsers)
        {
            var existingStudent = await _studentRepository.GetByEmailAsync(user.Email);
            if (existingStudent == null)
            {
                var student = new Student
                {
                    Id = Guid.NewGuid(),
                    FirstName = user.Name.Split(' ')[0],
                    LastName = user.Name.Split(' ').Length > 1 ? string.Join(" ", user.Name.Split(' ').Skip(1)) : "",
                    Email = user.Email,
                    PhoneNumber = "+1234567890",
                    DateOfBirth = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    Address = "123 Student Street, City, State",
                    Gender = Gender.Male,
                    RollNumber = $"student_{DateTime.UtcNow.Ticks % 10000}",
                    EnrollmentDate = DateTime.UtcNow.AddYears(-1),
                    Status = StudentStatus.Active,
                    Grade = "12",
                    Section = "A",
                    UserId = user.Id
                };
                
                await _studentRepository.CreateAsync(student);
            }
        }
    }

    private async Task SeedFacultyAsync()
    {
        // Get all users and filter by role since GetByRoleAsync doesn't exist
        var allUsers = await _userRepository.GetAllAsync();
        var facultyUsers = allUsers.Where(u => u.Role == UserRole.Faculty);
        
        foreach (var user in facultyUsers)
        {
            var existingFaculty = await _facultyRepository.GetByEmailAsync(user.Email);
            if (existingFaculty == null)
            {
                var faculty = new Faculty
                {
                    Id = Guid.NewGuid(),
                    FirstName = user.Name.Split(' ')[0],
                    LastName = user.Name.Split(' ').Length > 1 ? string.Join(" ", user.Name.Split(' ').Skip(1)) : "",
                    Email = user.Email,
                    PhoneNumber = "+1234567890",
                    DateOfBirth = new DateTime(1980, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    Address = "456 Faculty Avenue, City, State",
                    Gender = Gender.Male,
                    EmployeeId = $"faculty_{DateTime.UtcNow.Ticks % 10000}",
                    HireDate = DateTime.UtcNow.AddYears(-2),
                    Department = "Computer Science",
                    Subject = "Programming",
                    IsActive = true,
                    UserId = user.Id
                };
                
                await _facultyRepository.CreateAsync(faculty);
            }
        }
    }

    private async Task SeedParentsAsync()
    {
        // Get all users and filter by role since GetByRoleAsync doesn't exist
        var allUsers = await _userRepository.GetAllAsync();
        var parentUsers = allUsers.Where(u => u.Role == UserRole.Parent);
        
        foreach (var user in parentUsers)
        {
            try
            {
                var existingParent = await _parentRepository.GetByEmailAsync(user.Email);
                if (existingParent == null)
                {
                    // Double-check using EmailExistsAsync for additional safety
                    var emailExists = await _parentRepository.EmailExistsAsync(user.Email);
                    if (!emailExists)
                    {
                        var parent = new Parent
                        {
                            Id = Guid.NewGuid(),
                            FirstName = user.Name.Split(' ')[0],
                            LastName = user.Name.Split(' ').Length > 1 ? string.Join(" ", user.Name.Split(' ').Skip(1)) : "",
                            Email = user.Email,
                            PhoneNumber = "+1234567890",
                            DateOfBirth = new DateTime(1975, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                            Address = "789 Parent Lane, City, State",
                            City = "City",
                            State = "State",
                            PostalCode = "12345",
                            Country = "USA",
                            Gender = Gender.Male,
                            Occupation = "Software Engineer",
                            Employer = "Tech Company",
                            WorkPhone = "+1234567891",
                            EmergencyContactName = "Emergency Contact",
                            EmergencyContactPhone = "+1234567892",
                            EmergencyContactRelationship = "Spouse",
                            ParentType = ParentType.Primary,
                            IsEmergencyContact = true,
                            IsAuthorizedToPickup = true,
                            IsActive = true,
                            UserId = user.Id
                        };
                        
                        await _parentRepository.AddAsync(parent);
                    }
                }
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException pgEx && pgEx.SqlState == "23505")
            {
                // Ignore duplicate key violations - parent already exists
                Console.WriteLine($"Parent with email {user.Email} already exists, skipping...");
            }
        }
    }

    private async Task SeedSampleFeesAsync()
    {
        // Get existing students to create fees for
        var students = await _studentRepository.GetAllAsync();
        if (!students.Any())
        {
            return; // No students to create fees for
        }

        var sampleFees = new List<StudentFee>();
        var terms = new[] { "2024-Q1", "2024-Q2", "2024-Q3", "2024-Q4" };
        var feeTypes = Enum.GetValues<FeeType>();

        foreach (var student in students.Take(3)) // Create fees for first 3 students
        {
            foreach (var term in terms)
            {
                foreach (var feeType in feeTypes)
                {
                    var dueDate = term switch
                    {
                        "2024-Q1" => new DateTime(2024, 3, 31, 0, 0, 0, DateTimeKind.Utc),
                        "2024-Q2" => new DateTime(2024, 6, 30, 0, 0, 0, DateTimeKind.Utc),
                        "2024-Q3" => new DateTime(2024, 9, 30, 0, 0, 0, DateTimeKind.Utc),
                        "2024-Q4" => new DateTime(2024, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                        _ => DateTime.UtcNow.AddMonths(3)
                    };

                    var totalAmount = feeType switch
                    {
                        FeeType.Tuition => 5000m,
                        FeeType.Exam => 1000m,
                        FeeType.Transport => 2000m,
                        FeeType.Library => 500m,
                        FeeType.Misc => 300m,
                        _ => 1000m
                    };

                    var fee = new StudentFee
                    {
                        StudentId = student.Id,
                        FeeType = feeType,
                        Term = term,
                        PaymentStatus = PaymentStatus.Pending,
                        DueDate = dueDate,
                        Notes = $"Sample {feeType} fee for {term}"
                    };

                    // Set encrypted fields using the encryption service
                    fee.EncryptedTotalAmount = _encryptionService.EncryptDecimal(totalAmount);
                    fee.EncryptedAmountPaid = _encryptionService.EncryptDecimal(0m);
                    fee.EncryptedAmountDue = _encryptionService.EncryptDecimal(totalAmount);
                    fee.EncryptedFineAmount = _encryptionService.EncryptDecimal(0m);

                    sampleFees.Add(fee);
                }
            }
        }

        // Create fees in batches, checking for existing fees by StudentId, FeeType, and Term
        foreach (var fee in sampleFees)
        {
            // Check if a fee with the same StudentId, FeeType, and Term already exists
            var existingFees = await _feeRepository.GetByStudentIdAsync(fee.StudentId);
            var feeExists = existingFees.Any(f => f.FeeType == fee.FeeType && f.Term == f.Term);
            
            if (!feeExists)
            {
                await _feeRepository.CreateAsync(fee);
            }
        }
    }
}
