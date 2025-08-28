using NUnit.Framework;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduShield.Core.Dtos;
using EduShield.Core.Entities;
using EduShield.Core.Enums;
using FluentAssertions;
using System.Net;
using EduShield.Api.Tests.Fixtures;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;

namespace EduShield.Api.Tests.Integration;

[TestFixture]
public class FacultyStudentAssignmentIntegrationTests : IntegrationTestFixture
{
    private Guid _facultyId;
    private Guid _studentId;
    private string _adminToken;

    [SetUp]
    public override void Setup()
    {
        base.Setup();
        
        // Create test data synchronously
        CreateTestDataAsync().GetAwaiter().GetResult();
        
        // Get admin token for testing
        _adminToken = GetAdminTokenAsync().GetAwaiter().GetResult();
    }

    private async Task CreateTestDataAsync()
    {
        // Create faculty
        var faculty = new Faculty
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "Faculty",
            Email = "test.faculty@example.com",
            PhoneNumber = "+1234567890",
            DateOfBirth = new DateTime(1980, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Address = "123 Faculty St",
            Gender = Gender.Male,
            Department = "Computer Science",
            Subject = "Programming",
            EmployeeId = "FAC001",
            HireDate = DateTime.UtcNow.AddYears(-2),
            IsActive = true
        };

        // Create student
        var student = new Student
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "Student",
            Email = "test.student@example.com",
            PhoneNumber = "+1234567891",
            DateOfBirth = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Address = "123 Student St",
            Gender = Gender.Female,
            RollNumber = "STU001",
            EnrollmentDate = DateTime.UtcNow.AddYears(-1),
            Status = StudentStatus.Active,
            Grade = "12",
            Section = "A"
        };

        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EduShield.Core.Data.EduShieldDbContext>();
        
        context.Faculty.Add(faculty);
        context.Students.Add(student);
        await context.SaveChangesAsync();

        _facultyId = faculty.Id;
        _studentId = student.Id;
    }

    [Test]
    public async Task AssignStudentToFaculty_WithValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        var request = new CreateFacultyStudentAssignmentRequest
        {
            FacultyId = _facultyId,
            StudentId = _studentId,
            Notes = "Test assignment"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/faculty-student-assignments", request);
        var result = await response.Content.ReadFromJsonAsync<FacultyStudentAssignmentDto>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.FacultyId.Should().Be(_facultyId);
        result.StudentId.Should().Be(_studentId);
        result.Notes.Should().Be("Test assignment");
        result.IsActive.Should().BeTrue();
    }

    [Test]
    public async Task AssignStudentToFaculty_WithNonExistentFaculty_ShouldReturnNotFound()
    {
        // Arrange
        var request = new CreateFacultyStudentAssignmentRequest
        {
            FacultyId = Guid.NewGuid(),
            StudentId = _studentId
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/faculty-student-assignments", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task AssignStudentToFaculty_WithNonExistentStudent_ShouldReturnNotFound()
    {
        // Arrange
        var request = new CreateFacultyStudentAssignmentRequest
        {
            FacultyId = _facultyId,
            StudentId = Guid.NewGuid()
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/faculty-student-assignments", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task AssignStudentToFaculty_WithDuplicateAssignment_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CreateFacultyStudentAssignmentRequest
        {
            FacultyId = _facultyId,
            StudentId = _studentId
        };

        // First assignment
        await Client.PostAsJsonAsync("/api/v1/faculty-student-assignments", request);

        // Second assignment (duplicate)
        var response = await Client.PostAsJsonAsync("/api/v1/faculty-student-assignments", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task BulkAssignStudentsToFaculty_WithValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        var student2Id = Guid.NewGuid();
        
        // Create second student
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EduShield.Core.Data.EduShieldDbContext>();
        var student2 = new Student
        {
            Id = student2Id,
            FirstName = "Test2",
            LastName = "Student2",
            Email = "test2.student@example.com",
            PhoneNumber = "+1234567892",
            DateOfBirth = new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Address = "124 Student St",
            Gender = Gender.Male,
            RollNumber = "STU002",
            EnrollmentDate = DateTime.UtcNow.AddYears(-1),
            Status = StudentStatus.Active,
            Grade = "12",
            Section = "A"
        };
        context.Students.Add(student2);
        await context.SaveChangesAsync();

        var request = new BulkFacultyStudentAssignmentRequest
        {
            FacultyId = _facultyId,
            StudentIds = new List<Guid> { _studentId, student2Id },
            Notes = "Bulk assignment"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/faculty-student-assignments/bulk", request);
        var result = await response.Content.ReadFromJsonAsync<List<FacultyStudentAssignmentDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Should().HaveCount(2);
        result.Should().OnlyContain(a => a.FacultyId == _facultyId);
    }

    [Test]
    public async Task UpdateAssignment_WithValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        // First create an assignment
        var createRequest = new CreateFacultyStudentAssignmentRequest
        {
            FacultyId = _facultyId,
            StudentId = _studentId,
            Notes = "Initial notes"
        };
        await Client.PostAsJsonAsync("/api/v1/faculty-student-assignments", createRequest);

        // Update the assignment
        var updateRequest = new UpdateFacultyStudentAssignmentRequest
        {
            FacultyId = _facultyId,
            StudentId = _studentId,
            IsActive = false,
            Notes = "Updated notes"
        };

        // Act
        var response = await Client.PutAsJsonAsync("/api/v1/faculty-student-assignments", updateRequest);
        var result = await response.Content.ReadFromJsonAsync<FacultyStudentAssignmentDto>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.IsActive.Should().BeFalse();
        result.Notes.Should().Be("Updated notes");
    }

    [Test]
    public async Task UpdateAssignment_WithNonExistentAssignment_ShouldReturnNotFound()
    {
        // Arrange
        var request = new UpdateFacultyStudentAssignmentRequest
        {
            FacultyId = Guid.NewGuid(),
            StudentId = Guid.NewGuid(),
            IsActive = false
        };

        // Act
        var response = await Client.PutAsJsonAsync("/api/v1/faculty-student-assignments", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task DeactivateAssignment_WithValidIds_ShouldReturnSuccess()
    {
        // Arrange
        // First create an assignment
        var createRequest = new CreateFacultyStudentAssignmentRequest
        {
            FacultyId = _facultyId,
            StudentId = _studentId
        };
        await Client.PostAsJsonAsync("/api/v1/faculty-student-assignments", createRequest);

        // Act
        var response = await Client.DeleteAsync($"/api/v1/faculty-student-assignments/{_facultyId}/{_studentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task DeactivateAssignment_WithNonExistentAssignment_ShouldReturnNotFound()
    {
        // Act
        var response = await Client.DeleteAsync($"/api/v1/faculty-student-assignments/{Guid.NewGuid()}/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task ActivateAssignment_WithValidIds_ShouldReturnSuccess()
    {
        // Arrange
        // First create an assignment
        var createRequest = new CreateFacultyStudentAssignmentRequest
        {
            FacultyId = _facultyId,
            StudentId = _studentId
        };
        await Client.PostAsJsonAsync("/api/v1/faculty-student-assignments", createRequest);

        // Deactivate it first
        await Client.DeleteAsync($"/api/v1/faculty-student-assignments/{_facultyId}/{_studentId}");

        // Act
        var response = await Client.PatchAsync($"/api/v1/faculty-student-assignments/{_facultyId}/{_studentId}/activate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task GetAssignment_WithValidIds_ShouldReturnAssignment()
    {
        // Arrange
        // First create an assignment
        var createRequest = new CreateFacultyStudentAssignmentRequest
        {
            FacultyId = _facultyId,
            StudentId = _studentId,
            Notes = "Test assignment"
        };
        await Client.PostAsJsonAsync("/api/v1/faculty-student-assignments", createRequest);

        // Act
        var response = await Client.GetAsync($"/api/v1/faculty-student-assignments/{_facultyId}/{_studentId}");
        var result = await response.Content.ReadFromJsonAsync<FacultyStudentAssignmentDto>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.FacultyId.Should().Be(_facultyId);
        result.StudentId.Should().Be(_studentId);
    }

    [Test]
    public async Task GetAssignment_WithNonExistentIds_ShouldReturnNotFound()
    {
        // Act
        var response = await Client.GetAsync($"/api/v1/faculty-student-assignments/{Guid.NewGuid()}/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task GetFacultyAssignments_WithValidFacultyId_ShouldReturnAssignments()
    {
        // Arrange
        // First create an assignment
        var createRequest = new CreateFacultyStudentAssignmentRequest
        {
            FacultyId = _facultyId,
            StudentId = _studentId
        };
        await Client.PostAsJsonAsync("/api/v1/faculty-student-assignments", createRequest);

        // Act
        var response = await Client.GetAsync($"/api/v1/faculty-student-assignments/faculty/{_facultyId}");
        var result = await response.Content.ReadFromJsonAsync<List<FacultyStudentAssignmentDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Should().HaveCount(1);
        result[0].FacultyId.Should().Be(_facultyId);
    }

    [Test]
    public async Task GetStudentAssignments_WithValidStudentId_ShouldReturnAssignments()
    {
        // Arrange
        // First create an assignment
        var createRequest = new CreateFacultyStudentAssignmentRequest
        {
            FacultyId = _facultyId,
            StudentId = _studentId
        };
        await Client.PostAsJsonAsync("/api/v1/faculty-student-assignments", createRequest);

        // Act
        var response = await Client.GetAsync($"/api/v1/faculty-student-assignments/student/{_studentId}");
        var result = await response.Content.ReadFromJsonAsync<List<FacultyStudentAssignmentDto>>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Should().HaveCount(1);
        result[0].StudentId.Should().Be(_studentId);
    }

    [Test]
    public async Task GetAssignments_WithFilter_ShouldReturnPaginatedResults()
    {
        // Arrange
        // First create an assignment
        var createRequest = new CreateFacultyStudentAssignmentRequest
        {
            FacultyId = _facultyId,
            StudentId = _studentId
        };
        await Client.PostAsJsonAsync("/api/v1/faculty-student-assignments", createRequest);

        // Act
        var response = await Client.GetAsync("/api/v1/faculty-student-assignments?page=1&pageSize=10");
        var result = await response.Content.ReadFromJsonAsync<object>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
    }

    [Test]
    public async Task GetFacultyDashboard_WithValidFacultyId_ShouldReturnDashboard()
    {
        // Arrange
        // First create an assignment
        var createRequest = new CreateFacultyStudentAssignmentRequest
        {
            FacultyId = _facultyId,
            StudentId = _studentId
        };
        await Client.PostAsJsonAsync("/api/v1/faculty-student-assignments", createRequest);

        // Act
        var response = await Client.GetAsync($"/api/v1/faculty-student-assignments/faculty/{_facultyId}/dashboard");
        var result = await response.Content.ReadFromJsonAsync<FacultyDashboardDto>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.Faculty.Id.Should().Be(_facultyId);
        result.TotalAssignedStudents.Should().Be(1);
        result.ActiveAssignments.Should().Be(1);
        result.AssignedStudents.Should().HaveCount(1);
    }

    [Test]
    public async Task GetFacultyDashboard_WithNonExistentFacultyId_ShouldReturnNotFound()
    {
        // Act
        var response = await Client.GetAsync($"/api/v1/faculty-student-assignments/faculty/{Guid.NewGuid()}/dashboard");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task IsStudentAssignedToFaculty_WithExistingAssignment_ShouldReturnTrue()
    {
        // Arrange
        // First create an assignment
        var createRequest = new CreateFacultyStudentAssignmentRequest
        {
            FacultyId = _facultyId,
            StudentId = _studentId
        };
        await Client.PostAsJsonAsync("/api/v1/faculty-student-assignments", createRequest);

        // Act
        var response = await Client.GetAsync($"/api/v1/faculty-student-assignments/{_facultyId}/{_studentId}/exists");
        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeTrue();
    }

    [Test]
    public async Task IsStudentAssignedToFaculty_WithNonExistentAssignment_ShouldReturnFalse()
    {
        // Act
        var response = await Client.GetAsync($"/api/v1/faculty-student-assignments/{Guid.NewGuid()}/{Guid.NewGuid()}/exists");
        var result = await response.Content.ReadFromJsonAsync<bool>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().BeFalse();
    }

    [Test]
    public async Task GetFacultyActiveAssignmentCount_WithValidFacultyId_ShouldReturnCount()
    {
        // Arrange
        // First create an assignment
        var createRequest = new CreateFacultyStudentAssignmentRequest
        {
            FacultyId = _facultyId,
            StudentId = _studentId
        };
        await Client.PostAsJsonAsync("/api/v1/faculty-student-assignments", createRequest);

        // Act
        var response = await Client.GetAsync($"/api/v1/faculty-student-assignments/faculty/{_facultyId}/active-count");
        var result = await response.Content.ReadFromJsonAsync<int>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().Be(1);
    }

    [Test]
    public async Task AssignStudentToFaculty_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new CreateFacultyStudentAssignmentRequest
        {
            FacultyId = _facultyId,
            StudentId = _studentId
        };

        // Create a completely new client without any authentication context
        var unauthorizedClient = Factory.CreateClient();
        
        // Ensure no authentication headers
        unauthorizedClient.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await unauthorizedClient.PostAsJsonAsync("/api/v1/faculty-student-assignments", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        
        // Restore authentication for other tests
        SetupAuthenticatedClient(UserRole.Admin);
    }

    private async Task<string> GetAdminTokenAsync()
    {
        var devAuthRequest = new { email = "iamsaquibanwar@gmail.com" };
        var response = await Client.PostAsJsonAsync("/api/v1/auth/dev", devAuthRequest);
        var authResult = await response.Content.ReadFromJsonAsync<AuthResult>();
        return authResult!.Token;
    }
}
