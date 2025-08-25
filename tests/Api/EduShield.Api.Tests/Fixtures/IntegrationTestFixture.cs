using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using EduShield.Api;
using EduShield.Core.Data;
using EduShield.Core.Entities;
using EduShield.Core.Enums;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.TestHost;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Http;

namespace EduShield.Api.Tests.Fixtures;

/// <summary>
/// Test fixture for integration testing with the full application stack
/// </summary>
[TestFixture]
public abstract class IntegrationTestFixture : BaseTestFixture
{
    protected WebApplicationFactory<Program> Factory { get; private set; } = null!;
    protected EduShieldDbContext DbContext { get; private set; } = null!;
    protected HttpClient Client { get; private set; } = null!;

    [SetUp]
    public override void Setup()
    {
        base.Setup();

        // Create test application factory
        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                // Set environment to "Test" to use in-memory database and skip data seeding
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    context.HostingEnvironment.EnvironmentName = "Test";
                });
                
                builder.ConfigureServices(services =>
                {
                    // Configure test authentication
                    ConfigureTestAuthentication(services);
                });
            });

        // Create HTTP client
        Client = Factory.CreateClient();

        // Get database context
        var scope = Factory.Services.CreateScope();
        DbContext = scope.ServiceProvider.GetRequiredService<EduShieldDbContext>();
        
        // Ensure database is created
        DbContext.Database.EnsureCreated();
        
        // Seed test data
        SeedTestData();
    }

    [TearDown]
    public override void TearDown()
    {
        // Clean up
        Client?.Dispose();
        Factory?.Dispose();
        
        // Clean up database
        if (DbContext != null)
        {
            DbContext.Database.EnsureDeleted();
            DbContext.Dispose();
        }
        
        base.TearDown();
    }

    /// <summary>
    /// Configures test authentication for integration tests
    /// </summary>
    /// <param name="services">Service collection</param>
    protected virtual void ConfigureTestAuthentication(IServiceCollection services)
    {
        // Remove the default JWT authentication
        var authHandlers = services.Where(s => s.ServiceType == typeof(IAuthenticationHandler)).ToList();
        foreach (var handler in authHandlers)
        {
            services.Remove(handler);
        }
        
        var authSchemeProviders = services.Where(s => s.ServiceType == typeof(IAuthenticationSchemeProvider)).ToList();
        foreach (var provider in authSchemeProviders)
        {
            services.Remove(provider);
        }
        
        // Add test authentication scheme
        services.AddAuthentication("Test")
            .AddScheme<TestAuthenticationSchemeOptions, TestAuthenticationHandler>("Test", options => { });
        
        // Override the authorization policy evaluator to always succeed
        services.AddSingleton<IPolicyEvaluator, TestPolicyEvaluator>();
    }

    /// <summary>
    /// Seeds the test database with initial data
    /// </summary>
    protected virtual void SeedTestData()
    {
        try
        {
            // Create test users first
            var adminUser = CreateTestUser(UserRole.Admin);
            var facultyUser = CreateTestUser(UserRole.Faculty);
            var studentUser = CreateTestUser(UserRole.Student);
            var parentUser = CreateTestUser(UserRole.Parent);

            DbContext.Users.AddRange(adminUser, facultyUser, studentUser, parentUser);
            DbContext.SaveChanges();

            // Create test students
            var student = CreateTestStudent(studentUser.Id);
            var parentStudent = CreateTestStudent(parentUser.Id);
            
            DbContext.Students.AddRange(student, parentStudent);
            DbContext.SaveChanges();

            // Create test faculty
            var faculty = CreateTestFaculty(facultyUser.Id);
            DbContext.Faculty.Add(faculty);
            DbContext.SaveChanges();

            // Create test performance records
            var performance1 = CreateTestPerformance(student.Id);
            var performance2 = CreateTestPerformance(parentStudent.Id);
            
            DbContext.StudentPerformances.AddRange(performance1, performance2);
            DbContext.SaveChanges();
        }
        catch (Exception ex)
        {
            // Log the error for debugging
            Console.WriteLine($"Error seeding test data: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Creates a test JWT token for authentication
    /// </summary>
    /// <param name="role">User role</param>
    /// <param name="userId">Optional user ID</param>
    /// <returns>JWT token string</returns>
    protected string CreateTestJwtToken(UserRole role, Guid? userId = null)
    {
        // This would create a real JWT token for testing
        // In a real implementation, you might use the actual JWT service
        var testUserId = userId ?? Guid.NewGuid();
        
        // For now, return a placeholder - this should be implemented with actual JWT generation
        return $"test-jwt-token-{role}-{testUserId}";
    }

    /// <summary>
    /// Sets up the HTTP client with authentication
    /// </summary>
    /// <param name="role">User role</param>
    /// <param name="userId">Optional user ID</param>
    protected void SetupAuthenticatedClient(UserRole role, Guid? userId = null)
    {
        var token = CreateTestJwtToken(role, userId);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Test", token);
        
        // Store the role and userId in the test authentication context
        TestAuthenticationHandler.CurrentRole = role;
        TestAuthenticationHandler.CurrentUserId = userId ?? Guid.NewGuid();
    }

    /// <summary>
    /// Clears authentication from the HTTP client
    /// </summary>
    protected void ClearAuthentication()
    {
        Client.DefaultRequestHeaders.Authorization = null;
        TestAuthenticationHandler.CurrentRole = null;
        TestAuthenticationHandler.CurrentUserId = null;
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
}

/// <summary>
/// Test authentication scheme options
/// </summary>
public class TestAuthenticationSchemeOptions : AuthenticationSchemeOptions
{
}

/// <summary>
/// Test authentication handler that creates authenticated users for testing
/// </summary>
public class TestAuthenticationHandler : AuthenticationHandler<TestAuthenticationSchemeOptions>
{
    public static UserRole? CurrentRole { get; set; }
    public static Guid? CurrentUserId { get; set; }

    public TestAuthenticationHandler(IOptionsMonitor<TestAuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (CurrentRole == null || CurrentUserId == null)
        {
            return Task.FromResult(AuthenticateResult.Fail("No test authentication context"));
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, CurrentUserId.Value.ToString()),
            new Claim(ClaimTypes.Role, CurrentRole.ToString()),
            new Claim("role", CurrentRole.ToString()),
            new Claim("userId", CurrentUserId.Value.ToString())
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = 401;
        return Task.CompletedTask;
    }

    protected Task HandleForbidAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = 403;
        return Task.CompletedTask;
    }
}

/// <summary>
/// Test policy evaluator that always succeeds for testing
/// </summary>
public class TestPolicyEvaluator : IPolicyEvaluator
{
    public Task<PolicyAuthorizationResult> AuthorizeAsync(AuthorizationPolicy policy, AuthenticateResult authenticationResult, HttpContext context, object? resource)
    {
        // For testing, we'll let the actual authorization policies run
        // The TestAuthenticationHandler will provide the necessary claims
        return Task.FromResult(PolicyAuthorizationResult.Success());
    }

    public Task<AuthenticateResult> AuthenticateAsync(AuthorizationPolicy policy, HttpContext context)
    {
        // Use the default authentication result
        return Task.FromResult(context.AuthenticateAsync("Test").Result);
    }
}
