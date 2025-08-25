using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using EduShield.Core.Data;
using EduShield.Core.Entities;
using EduShield.Core.Enums;

namespace EduShield.Api.Tests.Fixtures;

/// <summary>
/// Test fixture for database testing with in-memory Entity Framework context
/// </summary>
[TestFixture]
public abstract class DatabaseTestFixture : BaseTestFixture
{
    protected EduShieldDbContext DbContext { get; private set; } = null!;
    protected IServiceProvider ServiceProvider { get; private set; } = null!;

    [SetUp]
    public override void Setup()
    {
        base.Setup();

        // Create in-memory database
        var services = new ServiceCollection();
        
        services.AddDbContext<EduShieldDbContext>(options =>
        {
            options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
                   .UseInternalServiceProvider(services.BuildServiceProvider());
        });

        services.AddLogging(builder => builder.AddConsole());

        ServiceProvider = services.BuildServiceProvider();
        DbContext = ServiceProvider.GetRequiredService<EduShieldDbContext>();
        
        // Ensure database is created
        DbContext.Database.EnsureCreated();
        
        // Seed test data
        SeedTestData();
    }

    [TearDown]
    public override void TearDown()
    {
        // Clean up database
        DbContext.Database.EnsureDeleted();
        DbContext.Dispose();
        
        // Dispose service provider
        if (ServiceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
        
        base.TearDown();
    }

    /// <summary>
    /// Seeds the test database with initial data
    /// </summary>
    protected virtual void SeedTestData()
    {
        // Create test users
        var adminUser = CreateTestUser(UserRole.Admin);
        var facultyUser = CreateTestUser(UserRole.Faculty);
        var studentUser = CreateTestUser(UserRole.Student);
        var parentUser = CreateTestUser(UserRole.Parent);

        DbContext.Users.AddRange(adminUser, facultyUser, studentUser, parentUser);

        // Create test students
        var student = CreateTestStudent(studentUser.Id);
        var parentStudent = CreateTestStudent(parentUser.Id);
        
        DbContext.Students.AddRange(student, parentStudent);

        // Create test faculty
        var faculty = CreateTestFaculty(facultyUser.Id);
        DbContext.Faculty.Add(faculty);

        // Create test performance records
        var performance1 = CreateTestPerformance(student.Id);
        var performance2 = CreateTestPerformance(parentStudent.Id);
        
        DbContext.StudentPerformances.AddRange(performance1, performance2);

        // Save changes
        DbContext.SaveChanges();
    }

    /// <summary>
    /// Creates a test user and adds it to the database
    /// </summary>
    /// <param name="role">User role</param>
    /// <param name="userId">Optional user ID</param>
    /// <returns>Created user entity</returns>
    protected User CreateAndAddUser(UserRole role = UserRole.Student, Guid? userId = null)
    {
        var user = CreateTestUser(role, userId);
        DbContext.Users.Add(user);
        DbContext.SaveChanges();
        return user;
    }

    /// <summary>
    /// Creates a test student and adds it to the database
    /// </summary>
    /// <param name="userId">Optional user ID</param>
    /// <param name="studentId">Optional student ID</param>
    /// <returns>Created student entity</returns>
    protected Student CreateAndAddStudent(Guid? userId = null, Guid? studentId = null)
    {
        var student = CreateTestStudent(userId, studentId);
        DbContext.Students.Add(student);
        DbContext.SaveChanges();
        return student;
    }

    /// <summary>
    /// Creates a test faculty and adds it to the database
    /// </summary>
    /// <param name="userId">Optional user ID</param>
    /// <param name="facultyId">Optional faculty ID</param>
    /// <returns>Created faculty entity</returns>
    protected Faculty CreateAndAddFaculty(Guid? userId = null, Guid? facultyId = null)
    {
        var faculty = CreateTestFaculty(userId, facultyId);
        DbContext.Faculty.Add(faculty);
        DbContext.SaveChanges();
        return faculty;
    }

    /// <summary>
    /// Creates a test performance record and adds it to the database
    /// </summary>
    /// <param name="studentId">Optional student ID</param>
    /// <param name="performanceId">Optional performance ID</param>
    /// <returns>Created performance entity</returns>
    protected StudentPerformance CreateAndAddPerformance(Guid? studentId = null, Guid? performanceId = null)
    {
        var performance = CreateTestPerformance(studentId, performanceId);
        DbContext.StudentPerformances.Add(performance);
        DbContext.SaveChanges();
        return performance;
    }

    /// <summary>
    /// Clears all data from the database
    /// </summary>
    protected void ClearDatabase()
    {
        DbContext.StudentPerformances.RemoveRange(DbContext.StudentPerformances);
        DbContext.Students.RemoveRange(DbContext.Students);
        DbContext.Faculty.RemoveRange(DbContext.Faculty);
        DbContext.Users.RemoveRange(DbContext.Users);
        DbContext.SaveChanges();
    }

    /// <summary>
    /// Gets a user by role from the database
    /// </summary>
    /// <param name="role">User role to find</param>
    /// <returns>User entity or null if not found</returns>
    protected User? GetUserByRole(UserRole role)
    {
        return DbContext.Users.FirstOrDefault(u => u.Role == role);
    }

    /// <summary>
    /// Gets a student by user ID from the database
    /// </summary>
    /// <param name="userId">User ID to find</param>
    /// <returns>Student entity or null if not found</returns>
    protected Student? GetStudentByUserId(Guid userId)
    {
        return DbContext.Students.FirstOrDefault(s => s.UserId == userId);
    }

    /// <summary>
    /// Gets a faculty by user ID from the database
    /// </summary>
    /// <param name="userId">User ID to find</param>
    /// <returns>Faculty entity or null if not found</returns>
    protected Faculty? GetFacultyByUserId(Guid userId)
    {
        return DbContext.Faculty.FirstOrDefault(f => f.UserId == userId);
    }
}
