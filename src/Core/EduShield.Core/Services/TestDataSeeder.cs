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
    private readonly IParentRepository _parentRepository;
    private readonly IFacultyStudentAssignmentRepository _assignmentRepository;
    private readonly IStudentPerformanceRepository _performanceRepository;
    private readonly IEncryptionService _encryptionService;

    public TestDataSeeder(
        IUserRepository userRepository,
        IStudentFeeRepository feeRepository,
        IStudentRepository studentRepository,
        IFacultyRepository facultyRepository,
        IParentRepository parentRepository,
        IFacultyStudentAssignmentRepository assignmentRepository,
        IStudentPerformanceRepository performanceRepository,
        IEncryptionService encryptionService)
    {
        _userRepository = userRepository;
        _feeRepository = feeRepository;
        _studentRepository = studentRepository;
        _facultyRepository = facultyRepository;
        _parentRepository = parentRepository;
        _assignmentRepository = assignmentRepository;
        _performanceRepository = performanceRepository;
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
        try
        {
            await SeedStudentsAsync();
            Console.WriteLine("Students seeded successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error seeding students: {ex.Message}");
        }
        
        // Seed faculty for users with Faculty role
        try
        {
            await SeedFacultyAsync();
            Console.WriteLine("Faculty seeded successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error seeding faculty: {ex.Message}");
        }

        // Seed parents for users with Parent role
        try
        {
            await SeedParentsAsync();
            Console.WriteLine("Parents seeded successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error seeding parents: {ex.Message}");
        }

        // Seed sample fee data
        try
        {
            await SeedSampleFeesAsync();
            Console.WriteLine("Sample fees seeded successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error seeding sample fees: {ex.Message}");
        }

        // Seed faculty-student assignments
        try
        {
            await SeedFacultyStudentAssignmentsAsync();
            Console.WriteLine("Faculty-student assignments seeded successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error seeding faculty-student assignments: {ex.Message}");
        }

        // Seed sample performance data
        try
        {
            await SeedSamplePerformanceDataAsync();
            Console.WriteLine("Sample performance data seeded successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error seeding sample performance data: {ex.Message}");
        }
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
                        Console.WriteLine($"Created parent: {user.Email}");
                    }
                    else
                    {
                        Console.WriteLine($"Parent with email {user.Email} already exists (EmailExistsAsync), skipping...");
                    }
                }
                else
                {
                    Console.WriteLine($"Parent with email {user.Email} already exists (GetByEmailAsync), skipping...");
                }
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException pgEx && pgEx.SqlState == "23505")
            {
                // Ignore duplicate key violations - parent already exists
                Console.WriteLine($"Parent with email {user.Email} already exists (caught duplicate key), skipping...");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating parent {user.Email}: {ex.Message}");
                // Continue with next parent instead of failing completely
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
            try
            {
                // Check if a fee with the same StudentId, FeeType, and Term already exists
                var existingFees = await _feeRepository.GetByStudentIdAsync(fee.StudentId);
                var feeExists = existingFees.Any(f => f.FeeType == fee.FeeType && f.Term == fee.Term);
                
                if (!feeExists)
                {
                    await _feeRepository.CreateAsync(fee);
                    Console.WriteLine($"Created fee: {fee.FeeType} for student {fee.StudentId} in term {fee.Term}");
                }
                else
                {
                    Console.WriteLine($"Fee {fee.FeeType} for student {fee.StudentId} in term {fee.Term} already exists, skipping...");
                }
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException pgEx && pgEx.SqlState == "23505")
            {
                // Ignore duplicate key violations
                Console.WriteLine($"Fee {fee.FeeType} for student {fee.StudentId} in term {fee.Term} already exists (caught duplicate), skipping...");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating fee {fee.FeeType} for student {fee.StudentId}: {ex.Message}");
                // Continue with next fee instead of failing completely
            }
        }
    }

    private async Task SeedFacultyStudentAssignmentsAsync()
    {
        // Get faculty and students
        var faculty = await _facultyRepository.GetAllAsync();
        var students = await _studentRepository.GetAllAsync();

        if (!faculty.Any() || !students.Any())
        {
            Console.WriteLine("No faculty or students found for assignments");
            return;
        }

        // Assign first 3 students to the first faculty member (saquibedu@gmail.com)
        var firstFaculty = faculty.FirstOrDefault(f => f.Email == "saquibedu@gmail.com");
        if (firstFaculty == null)
        {
            Console.WriteLine("Faculty with email saquibedu@gmail.com not found");
            return;
        }

        var studentsToAssign = students.Take(3).ToList();
        
        foreach (var student in studentsToAssign)
        {
            try
            {
                // Check if assignment already exists
                var existingAssignment = await _assignmentRepository.GetByFacultyAndStudentAsync(firstFaculty.Id, student.Id);
                if (existingAssignment == null)
                {
                    var assignment = new StudentFaculty
                    {
                        FacultyId = firstFaculty.Id,
                        StudentId = student.Id,
                        AssignedDate = DateTime.UtcNow,
                        IsActive = true,
                        Subject = firstFaculty.Subject ?? "Programming",
                        AcademicYear = "2024-2025",
                        Semester = "Fall"
                    };

                    await _assignmentRepository.CreateAsync(assignment);
                    Console.WriteLine($"Assigned student {student.FirstName} {student.LastName} to faculty {firstFaculty.FirstName} {firstFaculty.LastName}");
                }
                else
                {
                    Console.WriteLine($"Assignment already exists for student {student.FirstName} {student.LastName} and faculty {firstFaculty.FirstName} {firstFaculty.LastName}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating assignment for student {student.Id}: {ex.Message}");
            }
        }
    }

    private async Task SeedSamplePerformanceDataAsync()
    {
        // Get faculty-student assignments
        var assignments = await _assignmentRepository.GetAllAsync();
        if (!assignments.Any())
        {
            Console.WriteLine("No faculty-student assignments found for performance data");
            return;
        }

        var examTypes = Enum.GetValues<ExamType>();
        var subjects = new[] { "Programming", "Mathematics", "Computer Science", "Database Systems", "Web Development" };
        var random = new Random();

        foreach (var assignment in assignments.Take(10)) // Limit to first 10 assignments
        {
            try
            {
                // Create 3-5 performance records per assignment
                var recordCount = random.Next(3, 6);
                
                for (int i = 0; i < recordCount; i++)
                {
                    var examType = examTypes[random.Next(examTypes.Length)];
                    var subject = subjects[random.Next(subjects.Length)];
                    var score = random.Next(60, 100); // Random score between 60-100
                    var maxScore = 100;
                    var percentage = (decimal)score / maxScore * 100;
                    
                    var grade = percentage switch
                    {
                        >= 90 => "A+",
                        >= 85 => "A",
                        >= 80 => "B+",
                        >= 75 => "B",
                        >= 70 => "C+",
                        >= 65 => "C",
                        >= 60 => "D",
                        _ => "F"
                    };

                    var performance = new StudentPerformance
                    {
                        Id = Guid.NewGuid(),
                        StudentId = assignment.StudentId,
                        Subject = subject,
                        ExamType = examType,
                        ExamTitle = $"{examType} - {subject} Assessment",
                        ExamDate = DateTime.UtcNow.AddDays(-random.Next(1, 90)), // Random date in last 90 days
                        Score = score,
                        MaxScore = maxScore,
                        Comments = $"Good performance in {subject}. Keep up the good work!",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _performanceRepository.CreateAsync(performance);
                    Console.WriteLine($"Created performance record: {performance.Subject} - {performance.Grade} for student {assignment.StudentId}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating performance data for assignment {assignment.FacultyId}-{assignment.StudentId}: {ex.Message}");
            }
        }
    }
}
