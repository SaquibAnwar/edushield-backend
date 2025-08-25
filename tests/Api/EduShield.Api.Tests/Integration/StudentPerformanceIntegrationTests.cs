using System.Net;
using System.Text;
using System.Text.Json;
using EduShield.Api.Tests.Fixtures;
using EduShield.Core.Dtos;
using EduShield.Core.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;

namespace EduShield.Api.Tests.Integration;

/// <summary>
/// Integration tests for StudentPerformance API endpoints
/// </summary>
[TestFixture]
[Category("Integration")]
public class StudentPerformanceIntegrationTests : IntegrationTestFixture
{
    [Test]
    public async Task GetAllPerformance_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        ClearAuthentication();

        // Act
        var response = await Client.GetAsync("/api/v1/student-performance");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task GetAllPerformance_WithAdminToken_ShouldReturnAllRecords()
    {
        // Arrange
        SetupAuthenticatedClient(UserRole.Admin);

        // Act
        var response = await Client.GetAsync("/api/v1/student-performance");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var performances = JsonSerializer.Deserialize<List<StudentPerformanceDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        performances.Should().NotBeNull();
        performances!.Count.Should().BeGreaterThan(0);
    }

    [Test]
    public async Task CreatePerformance_WithAdminToken_ShouldSucceed()
    {
        // Arrange
        SetupAuthenticatedClient(UserRole.Admin);

        // Get an existing student ID from the seeded data
        var existingStudent = DbContext.Students.First();
        var request = CreateTestCreateRequest(existingStudent.Id);
        
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PostAsync("/api/v1/student-performance", content);

        // Assert
        if (response.StatusCode != HttpStatusCode.Created)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Error response: {response.StatusCode} - {errorContent}");
        }
        
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var createdPerformance = JsonSerializer.Deserialize<StudentPerformanceDto>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        createdPerformance.Should().NotBeNull();
        createdPerformance!.Subject.Should().Be(request.Subject);
        createdPerformance.ExamType.Should().Be(request.ExamType);
    }

    [Test]
    public async Task CreatePerformance_WithStudentToken_ShouldReturnForbidden()
    {
        // Arrange
        SetupAuthenticatedClient(UserRole.Student);

        var request = CreateTestCreateRequest();
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PostAsync("/api/v1/student-performance", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task GetPerformanceById_WithValidId_ShouldReturnPerformance()
    {
        // Arrange
        SetupAuthenticatedClient(UserRole.Admin);
        
        // Get an existing performance ID from the seeded data
        var existingPerformance = DbContext.StudentPerformances.First();
        var performanceId = existingPerformance.Id;

        // Act
        var response = await Client.GetAsync($"/api/v1/student-performance/{performanceId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var performance = JsonSerializer.Deserialize<StudentPerformanceDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        performance.Should().NotBeNull();
        performance!.Id.Should().Be(performanceId);
    }

    [Test]
    public async Task GetPerformanceById_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        SetupAuthenticatedClient(UserRole.Admin);
        var invalidId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"/api/v1/student-performance/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task UpdatePerformance_WithAdminToken_ShouldSucceed()
    {
        // Arrange
        SetupAuthenticatedClient(UserRole.Admin);
        
        // Get an existing performance ID from the seeded data
        var existingPerformance = DbContext.StudentPerformances.First();
        var performanceId = existingPerformance.Id;

        var request = CreateTestUpdateRequest();
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PutAsync($"/api/v1/student-performance/{performanceId}", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var updatedPerformance = JsonSerializer.Deserialize<StudentPerformanceDto>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        updatedPerformance.Should().NotBeNull();
        updatedPerformance!.Id.Should().Be(performanceId);
    }

    [Test]
    public async Task DeletePerformance_WithAdminToken_ShouldSucceed()
    {
        // Arrange
        SetupAuthenticatedClient(UserRole.Admin);
        
        // Get an existing performance ID from the seeded data
        var existingPerformance = DbContext.StudentPerformances.First();
        var performanceId = existingPerformance.Id;

        // Act
        var response = await Client.DeleteAsync($"/api/v1/student-performance/{performanceId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        // Refresh the database context to see the changes
        DbContext.ChangeTracker.Clear();
        
        // Verify the performance was actually deleted
        var deletedPerformance = DbContext.StudentPerformances.Find(performanceId);
        deletedPerformance.Should().BeNull();
    }

    [Test]
    public async Task DeletePerformance_WithFacultyToken_ShouldReturnForbidden()
    {
        // Arrange
        SetupAuthenticatedClient(UserRole.Faculty);
        var performanceId = Guid.NewGuid();

        // Act
        var response = await Client.DeleteAsync($"/api/v1/student-performance/{performanceId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task GetPerformanceBySubject_WithValidSubject_ShouldReturnResults()
    {
        // Arrange
        SetupAuthenticatedClient(UserRole.Admin);
        var subject = "Mathematics";

        // Act
        var response = await Client.GetAsync($"/api/v1/student-performance/subject/{subject}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var performances = JsonSerializer.Deserialize<List<StudentPerformanceDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        performances.Should().NotBeNull();
        performances!.Count.Should().BeGreaterThan(0);
        performances.Should().AllSatisfy(p => p.Subject.Should().Be(subject));
    }

    [Test]
    public async Task GetPerformanceByExamType_WithValidExamType_ShouldReturnResults()
    {
        // Arrange
        SetupAuthenticatedClient(UserRole.Admin);
        var examType = ExamType.MidTerm;

        // Act
        var response = await Client.GetAsync($"/api/v1/student-performance/exam-type/{examType}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var performances = JsonSerializer.Deserialize<List<StudentPerformanceDto>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        performances.Should().NotBeNull();
        performances!.Count.Should().BeGreaterThan(0);
        performances.Should().AllSatisfy(p => p.ExamType.Should().Be(examType));
    }

    [Test]
    public async Task GetStudentStatistics_WithValidStudentId_ShouldReturnStatistics()
    {
        // Arrange
        SetupAuthenticatedClient(UserRole.Admin);
        
        // Get an existing student ID from the seeded data
        var existingStudent = DbContext.Students.First();
        var studentId = existingStudent.Id;

        // Act
        var response = await Client.GetAsync($"/api/v1/student-performance/statistics/{studentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        
        // The response should contain statistics data - check for the actual format returned
        Console.WriteLine($"Statistics response: {content}");
        content.Should().Contain("totalExams");
        content.Should().Contain("averageScore");
    }
}
