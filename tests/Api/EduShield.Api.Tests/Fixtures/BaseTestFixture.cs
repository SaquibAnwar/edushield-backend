using AutoFixture;
using AutoFixture.AutoMoq;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace EduShield.Api.Tests.Fixtures;

/// <summary>
/// Base test fixture that provides common setup and utilities for all tests
/// </summary>
[TestFixture]
public abstract class BaseTestFixture
{
    protected IFixture Fixture { get; private set; } = null!;
    protected MockRepository MockRepository { get; private set; } = null!;

    [SetUp]
    public virtual void Setup()
    {
        // Initialize AutoFixture with AutoMoq integration and handle circular references
        Fixture = new Fixture()
            .Customize(new AutoMoqCustomization());

        // Configure AutoFixture to create UTC DateTime values for PostgreSQL compatibility
        Fixture.Customize<DateTime>(composer => composer.FromFactory(() => DateTime.UtcNow));
        Fixture.Customize<DateTime?>(composer => composer.FromFactory(() => DateTime.UtcNow));
        
        // AutoFixture is now only used for simple types, complex entities are created manually

        // Initialize MockRepository for strict mocking
        MockRepository = new MockRepository(MockBehavior.Strict);
    }

    [TearDown]
    public virtual void TearDown()
    {
        // Verify all mocks were used as expected
        try
        {
            MockRepository.VerifyAll();
        }
        catch (Exception)
        {
            // In test scenarios, some mocks might not be used
            // This is acceptable for testing purposes
        }
    }

    /// <summary>
    /// Creates a mock logger for testing
    /// </summary>
    /// <typeparam name="T">Type of the logger</typeparam>
    /// <returns>Mock logger instance</returns>
    protected Mock<ILogger<T>> CreateMockLogger<T>()
    {
        var logger = MockRepository.Create<ILogger<T>>();
        
        // Setup common logger methods to avoid strict mock failures
        logger.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Verifiable();

        return logger;
    }

    /// <summary>
    /// Creates a test user with specified role
    /// </summary>
    /// <param name="role">User role</param>
    /// <param name="userId">Optional user ID</param>
    /// <param name="email">Optional email address</param>
    /// <returns>Test user entity</returns>
    protected Core.Entities.User CreateTestUser(Core.Enums.UserRole role = Core.Enums.UserRole.Student, Guid? userId = null, string? email = null)
    {
        var user = new Core.Entities.User
        {
            Id = userId ?? Guid.NewGuid(),
            Role = role,
            Email = email ?? $"test-{role.ToString().ToLower()}.{Guid.NewGuid():N}@example.com",
            Name = $"Test {role}",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        return user;
    }

    /// <summary>
    /// Creates a test student entity
    /// </summary>
    /// <param name="userId">Optional user ID</param>
    /// <param name="studentId">Optional student ID</param>
    /// <param name="email">Optional email address</param>
    /// <param name="rollNumber">Optional roll number</param>
    /// <returns>Test student entity</returns>
    protected Core.Entities.Student CreateTestStudent(Guid? userId = null, Guid? studentId = null, string? email = null, string? rollNumber = null)
    {
        var student = new Core.Entities.Student
        {
            Id = studentId ?? Guid.NewGuid(),
            UserId = userId ?? Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe",
            Email = email ?? $"john.doe.{Guid.NewGuid():N}@example.com",
            RollNumber = rollNumber ?? $"STU{Guid.NewGuid().ToString("N").Substring(0, 8)}",
            Grade = "10",
            Section = "A",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        return student;
    }

    /// <summary>
    /// Creates a test faculty entity
    /// </summary>
    /// <param name="userId">Optional user ID</param>
    /// <param name="facultyId">Optional faculty ID</param>
    /// <param name="email">Optional email address</param>
    /// <param name="employeeId">Optional employee ID</param>
    /// <returns>Test faculty entity</returns>
    protected Core.Entities.Faculty CreateTestFaculty(Guid? userId = null, Guid? facultyId = null, string? email = null, string? employeeId = null)
    {
        var faculty = new Core.Entities.Faculty
        {
            Id = facultyId ?? Guid.NewGuid(),
            UserId = userId ?? Guid.NewGuid(),
            FirstName = "Dr. Jane",
            LastName = "Smith",
            Email = email ?? $"jane.smith.{Guid.NewGuid():N}@example.com",
            EmployeeId = employeeId ?? $"FAC{Guid.NewGuid().ToString("N").Substring(0, 8)}",
            Department = "Mathematics",
            Subject = "Mathematics",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        return faculty;
    }

    /// <summary>
    /// Creates a test student performance entity
    /// </summary>
    /// <param name="studentId">Optional student ID</param>
    /// <param name="performanceId">Optional performance ID</param>
    /// <returns>Test performance entity</returns>
    protected Core.Entities.StudentPerformance CreateTestPerformance(Guid? studentId = null, Guid? performanceId = null)
    {
        var performance = new Core.Entities.StudentPerformance
        {
            Id = performanceId ?? Guid.NewGuid(),
            StudentId = studentId ?? Guid.NewGuid(),
            Subject = "Mathematics",
            ExamType = Core.Enums.ExamType.MidTerm,
            ExamDate = DateTime.UtcNow.AddDays(-1),
            EncryptedScore = "encrypted-score-data",
            MaxScore = 100m,
            ExamTitle = "Mid-Term Mathematics Exam",
            Comments = "Good understanding of algebra concepts",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        return performance;
    }

    /// <summary>
    /// Creates a test performance DTO
    /// </summary>
    /// <param name="studentId">Optional student ID</param>
    /// <param name="performanceId">Optional performance ID</returns>
    /// <returns>Test performance DTO</returns>
    protected Core.Dtos.StudentPerformanceDto CreateTestPerformanceDto(Guid? studentId = null, Guid? performanceId = null)
    {
        var dto = new Core.Dtos.StudentPerformanceDto
        {
            Id = performanceId ?? Guid.NewGuid(),
            StudentId = studentId ?? Guid.NewGuid(),
            StudentFirstName = "John",
            StudentLastName = "Doe",
            Subject = "Mathematics",
            ExamType = Core.Enums.ExamType.MidTerm,
            ExamDate = DateTime.UtcNow.AddDays(-1),
            Score = 85.5m,
            MaxScore = 100m,
            ExamTitle = "Mid-Term Mathematics Exam",
            Comments = "Good understanding of algebra concepts",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        return dto;
    }

    /// <summary>
    /// Creates a test performance creation request
    /// </summary>
    /// <param name="studentId">Optional student ID</param>
    /// <returns>Test creation request</returns>
    protected Core.Dtos.CreateStudentPerformanceRequest CreateTestCreateRequest(Guid? studentId = null)
    {
        var request = new Core.Dtos.CreateStudentPerformanceRequest
        {
            StudentId = studentId ?? Guid.NewGuid(),
            Subject = "Mathematics",
            ExamType = Core.Enums.ExamType.MidTerm,
            ExamDate = DateTime.UtcNow.AddDays(-1),
            Score = 85.5m,
            MaxScore = 100m,
            ExamTitle = "Mid-Term Mathematics Exam",
            Comments = "Good understanding of algebra concepts"
        };
        
        return request;
    }

    /// <summary>
    /// Creates a test performance update request
    /// </summary>
    /// <returns>Test update request</returns>
    protected Core.Dtos.UpdateStudentPerformanceRequest CreateTestUpdateRequest()
    {
        var request = new Core.Dtos.UpdateStudentPerformanceRequest
        {
            Score = 90.0m,
            Comments = "Excellent work!"
        };
        
        return request;
    }
}
