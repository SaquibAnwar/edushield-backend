using NUnit.Framework;
using EduShield.Api.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text;
using System.Text.Json;
using EduShield.Core.Dtos;
using EduShield.Core.Enums;
using EduShield.Core.Interfaces;
using EduShield.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace EduShield.Api.Tests.Integration;

[TestFixture]
public class StudentFeeIntegrationTests : IntegrationTestFixture
{
    private IStudentFeeService _feeService;
    private IStudentService _studentService;

    [SetUp]
    public override void Setup()
    {
        base.Setup();
        
        // Get required services for test setup
        var scope = Factory.Services.CreateScope();
        _feeService = scope.ServiceProvider.GetRequiredService<IStudentFeeService>();
        _studentService = scope.ServiceProvider.GetRequiredService<IStudentService>();
        
        // Set up authentication as admin for all tests
        SetupAuthenticatedClient(UserRole.Admin);
    }

    [Test]
    public async Task CreateAndRetrieveFee_ShouldWorkEndToEnd()
    {
        // Arrange - Get an existing student from test data
        var existingStudent = DbContext.Students.First();
        var request = new CreateStudentFeeRequest
        {
            StudentId = existingStudent.Id,
            FeeType = FeeType.Tuition,
            Term = "2024-Q1",
            TotalAmount = 5000m,
            DueDate = DateTime.UtcNow.AddMonths(1),
            Notes = "Test fee creation"
        };

        // Act - Create fee via HTTP
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var createResponse = await Client.PostAsync("/api/v1/student-fees", content);

        // Assert - Creation
        Assert.That(createResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        var createResult = await createResponse.Content.ReadAsStringAsync();
        var createdFee = JsonSerializer.Deserialize<StudentFeeDto>(createResult);
        Assert.That(createdFee, Is.Not.Null);
        Assert.That(createdFee!.StudentId, Is.EqualTo(request.StudentId));

        // Act - Retrieve fee via HTTP
        var retrieveResponse = await Client.GetAsync($"/api/v1/student-fees/{createdFee.Id}");

        // Assert - Retrieval
        Assert.That(retrieveResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var retrieveResult = await retrieveResponse.Content.ReadAsStringAsync();
        var retrievedFee = JsonSerializer.Deserialize<StudentFeeDto>(retrieveResult);
        Assert.That(retrievedFee, Is.Not.Null);
        Assert.That(retrievedFee!.Id, Is.EqualTo(createdFee.Id));
    }

    [Test]
    public async Task UpdateFee_ShouldWorkEndToEnd()
    {
        // Arrange - Get an existing student from test data
        var existingStudent = DbContext.Students.First();
        var request = new CreateStudentFeeRequest
        {
            StudentId = existingStudent.Id,
            FeeType = FeeType.Exam,
            Term = "2024-Q1",
            TotalAmount = 1000m,
            DueDate = DateTime.UtcNow.AddMonths(1)
        };

        var createResult = await _feeService.CreateAsync(request, CancellationToken.None);
        var createdFee = createResult;

        var updateRequest = new UpdateStudentFeeRequest
        {
            TotalAmount = 1200m,
            Notes = "Updated fee amount"
        };

        // Act - Update fee via HTTP
        var json = JsonSerializer.Serialize(updateRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var updateResponse = await Client.PutAsync($"/api/v1/student-fees/{createdFee.Id}", content);

        // Assert
        Assert.That(updateResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var updateResult = await updateResponse.Content.ReadAsStringAsync();
        var updatedFee = JsonSerializer.Deserialize<StudentFeeDto>(updateResult);
        Assert.That(updatedFee, Is.Not.Null);
        Assert.That(updatedFee!.TotalAmount, Is.EqualTo(1200m));
    }

    [Test]
    public async Task DeleteFee_ShouldWorkEndToEnd()
    {
        // Arrange - Get an existing student from test data
        var existingStudent = DbContext.Students.First();
        var request = new CreateStudentFeeRequest
        {
            StudentId = existingStudent.Id,
            FeeType = FeeType.Transport,
            Term = "2024-Q1",
            TotalAmount = 2000m,
            DueDate = DateTime.UtcNow.AddMonths(1)
        };

        var createResult = await _feeService.CreateAsync(request, CancellationToken.None);
        var createdFee = createResult;

        // Act - Delete fee via HTTP
        var deleteResponse = await Client.DeleteAsync($"/api/v1/student-fees/{createdFee.Id}");

        // Assert
        Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        // Verify fee is deleted via HTTP
        var retrieveResponse = await Client.GetAsync($"/api/v1/student-fees/{createdFee.Id}");
        Assert.That(retrieveResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetFeesByType_ShouldReturnFilteredResults()
    {
        // Arrange - Get an existing student from test data
        var existingStudent = DbContext.Students.First();
        var requests = new[]
        {
            new CreateStudentFeeRequest
            {
                StudentId = existingStudent.Id,
                FeeType = FeeType.Tuition,
                Term = "2024-Q1",
                TotalAmount = 5000m,
                DueDate = DateTime.UtcNow.AddMonths(1)
            },
            new CreateStudentFeeRequest
            {
                StudentId = existingStudent.Id,
                FeeType = FeeType.Exam,
                Term = "2024-Q1",
                TotalAmount = 1000m,
                DueDate = DateTime.UtcNow.AddMonths(1)
            }
        };

        // Create fees using the service directly for test setup
        foreach (var req in requests)
        {
            await _feeService.CreateAsync(req, CancellationToken.None);
        }

        // Act - Test the HTTP endpoint
        var response = await Client.GetAsync("/api/v1/student-fees?feeType=Tuition");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var content = await response.Content.ReadAsStringAsync();
        var fees = JsonSerializer.Deserialize<IEnumerable<StudentFeeDto>>(content);
        Assert.That(fees, Is.Not.Null);
        Assert.That(fees!.All(f => f.FeeType == FeeType.Tuition), Is.True);
    }

    [Test]
    public async Task GetFeesByTerm_ShouldReturnFilteredResults()
    {
        // Arrange - Get an existing student from test data
        var existingStudent = DbContext.Students.First();
        var requests = new[]
        {
            new CreateStudentFeeRequest
            {
                StudentId = existingStudent.Id,
                FeeType = FeeType.Library,
                Term = "2024-Q1",
                TotalAmount = 500m,
                DueDate = DateTime.UtcNow.AddMonths(1)
            },
            new CreateStudentFeeRequest
            {
                StudentId = existingStudent.Id,
                FeeType = FeeType.Misc,
                Term = "2024-Q2",
                TotalAmount = 300m,
                DueDate = DateTime.UtcNow.AddMonths(2)
            }
        };

        // Create fees using the service directly for test setup
        foreach (var req in requests)
        {
            await _feeService.CreateAsync(req, CancellationToken.None);
        }

        // Act - Test the HTTP endpoint
        var response = await Client.GetAsync("/api/v1/student-fees?term=2024-Q1");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var content = await response.Content.ReadAsStringAsync();
        var fees = JsonSerializer.Deserialize<IEnumerable<StudentFeeDto>>(content);
        Assert.That(fees, Is.Not.Null);
        Assert.That(fees!.All(f => f.Term == "2024-Q1"), Is.True);
    }

    [Test]
    public async Task GetOverdueFees_ShouldReturnOverdueFees()
    {
        // Arrange - Get an existing student from test data
        var existingStudent = DbContext.Students.First();
        var request = new CreateStudentFeeRequest
        {
            StudentId = existingStudent.Id,
            FeeType = FeeType.Tuition,
            Term = "2024-Q1",
            TotalAmount = 5000m,
            DueDate = DateTime.UtcNow.AddMonths(1) // Start with future due date
        };

        var createResult = await _feeService.CreateAsync(request, CancellationToken.None);
        var createdFee = createResult;

        // Update the fee to be overdue by setting a past due date
        var updateRequest = new UpdateStudentFeeRequest
        {
            DueDate = DateTime.UtcNow.AddDays(-10) // Make it overdue
        };

        await _feeService.UpdateAsync(createdFee.Id, updateRequest, CancellationToken.None);

        // Act - Test the HTTP endpoint
        var response = await Client.GetAsync("/api/v1/student-fees/overdue");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var content = await response.Content.ReadAsStringAsync();
        var fees = JsonSerializer.Deserialize<IEnumerable<StudentFeeDto>>(content);
        Assert.That(fees, Is.Not.Null);
        Assert.That(fees!.Any(f => f.Id == createdFee.Id), Is.True);
    }

    [Test]
    public async Task CalculateLateFees_ShouldCalculateAndUpdateFees()
    {
        // Arrange - Get an existing student from test data
        var existingStudent = DbContext.Students.First();
        var request = new CreateStudentFeeRequest
        {
            StudentId = existingStudent.Id,
            FeeType = FeeType.Transport,
            Term = "2024-Q1",
            TotalAmount = 2000m,
            DueDate = DateTime.UtcNow.AddMonths(1) // Start with future due date
        };

        var createResult = await _feeService.CreateAsync(request, CancellationToken.None);
        var createdFee = createResult;

        // Update the fee to be overdue by setting a past due date
        var updateRequest = new UpdateStudentFeeRequest
        {
            DueDate = DateTime.UtcNow.AddDays(-15) // Make it overdue
        };

        await _feeService.UpdateAsync(createdFee.Id, updateRequest, CancellationToken.None);

        // Act - Test the HTTP endpoint
        var response = await Client.PostAsync("/api/v1/student-fees/calculate-late-fees", null);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<object>(content);
        Assert.That(result, Is.Not.Null);

        // Verify late fees were calculated
        var retrieveResponse = await Client.GetAsync($"/api/v1/student-fees/{createdFee.Id}");
        var updatedFee = JsonSerializer.Deserialize<StudentFeeDto>(
            await retrieveResponse.Content.ReadAsStringAsync());
        Assert.That(updatedFee!.FineAmount, Is.GreaterThan(0));
    }

    [Test]
    public async Task GetStudentFeeStatistics_ShouldReturnStatistics()
    {
        // Arrange - Get an existing student from test data
        var existingStudent = DbContext.Students.First();
        var requests = new[]
        {
            new CreateStudentFeeRequest
            {
                StudentId = existingStudent.Id,
                FeeType = FeeType.Tuition,
                Term = "2024-Q1",
                TotalAmount = 5000m,
                DueDate = DateTime.UtcNow.AddMonths(1)
            },
            new CreateStudentFeeRequest
            {
                StudentId = existingStudent.Id,
                FeeType = FeeType.Exam,
                Term = "2024-Q1",
                TotalAmount = 1000m,
                DueDate = DateTime.UtcNow.AddMonths(1)
            }
        };

        // Create fees using the service directly for test setup
        foreach (var req in requests)
        {
            await _feeService.CreateAsync(req, CancellationToken.None);
        }

        // Act - Test the HTTP endpoint
        var response = await Client.GetAsync($"/api/v1/student-fees/statistics/{existingStudent.Id}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var content = await response.Content.ReadAsStringAsync();
        var statistics = JsonSerializer.Deserialize<object>(content);
        Assert.That(statistics, Is.Not.Null);
    }

    [Test]
    public async Task GetAllFees_ShouldReturnFeesWithFilters()
    {
        // Arrange - Get an existing student from test data
        var existingStudent = DbContext.Students.First();
        var requests = new[]
        {
            new CreateStudentFeeRequest
            {
                StudentId = existingStudent.Id,
                FeeType = FeeType.Tuition,
                Term = "2024-Q1",
                TotalAmount = 5000m,
                DueDate = DateTime.UtcNow.AddMonths(1)
            },
            new CreateStudentFeeRequest
            {
                StudentId = existingStudent.Id,
                FeeType = FeeType.Transport,
                Term = "2024-Q1",
                TotalAmount = 2000m,
                DueDate = DateTime.UtcNow.AddMonths(1)
            }
        };

        // Create fees using the service directly for test setup
        foreach (var req in requests)
        {
            await _feeService.CreateAsync(req, CancellationToken.None);
        }

        // Act - Get all fees via HTTP
        var response = await Client.GetAsync("/api/v1/student-fees");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var content = await response.Content.ReadAsStringAsync();
        var fees = JsonSerializer.Deserialize<IEnumerable<StudentFeeDto>>(content);
        Assert.That(fees, Is.Not.Null);
        Assert.That(fees!.Count(), Is.GreaterThanOrEqualTo(2));

        // Act - Get fees with type filter via HTTP
        var filteredResponse = await Client.GetAsync("/api/v1/student-fees?feeType=Tuition");

        // Assert
        Assert.That(filteredResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var filteredContent = await filteredResponse.Content.ReadAsStringAsync();
        var filteredFees = JsonSerializer.Deserialize<IEnumerable<StudentFeeDto>>(filteredContent);
        Assert.That(filteredFees, Is.Not.Null);
        Assert.That(filteredFees!.All(f => f.FeeType == FeeType.Tuition), Is.True);
    }
}
