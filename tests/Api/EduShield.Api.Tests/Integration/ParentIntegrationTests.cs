using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using EduShield.Api.Controllers;
using EduShield.Core.Dtos;
using EduShield.Core.Entities;
using EduShield.Core.Enums;
using EduShield.Core.Data;
using EduShield.Core.Interfaces;
using EduShield.Core.Services;
using EduShield.Core.Security;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using EduShield.Api.Tests.Fixtures;
using FluentAssertions;

namespace EduShield.Api.Tests.Integration;

[TestFixture]
public class ParentIntegrationTests : IntegrationTestFixture
{
    private IParentService _parentService;
    private IParentRepository _parentRepository;
    private IStudentRepository _studentRepository;
    private IEncryptionService _encryptionService;

    [SetUp]
    public override void Setup()
    {
        base.Setup();
        
        // Create a scope to resolve scoped services
        var scope = Factory.Services.CreateScope();
        
        // Get services from the test fixture scope
        _parentService = scope.ServiceProvider.GetRequiredService<IParentService>();
        _parentRepository = scope.ServiceProvider.GetRequiredService<IParentRepository>();
        _studentRepository = scope.ServiceProvider.GetRequiredService<IStudentRepository>();
        _encryptionService = scope.ServiceProvider.GetRequiredService<IEncryptionService>();
        
        // Set up authentication for admin user
        SetupAuthenticatedClient(UserRole.Admin);
    }

    // Setup and TearDown methods removed for now

    #region Parent CRUD Tests

    [Test]
    public async Task CreateParent_ValidRequest_CreatesParentSuccessfully()
    {
        // Arrange
        var request = CreateValidCreateParentRequest();

        // Act
        var result = await _parentService.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBe(Guid.Empty);
        result.FirstName.Should().Be(request.FirstName);
        result.LastName.Should().Be(request.LastName);
        result.Email.Should().Be(request.Email);
        result.ParentType.Should().Be(request.ParentType);
        result.IsActive.Should().BeTrue();

        // Verify in database
        var savedParent = await DbContext.Parents.FindAsync(result.Id);
        savedParent.Should().NotBeNull();
        savedParent!.FirstName.Should().Be(request.FirstName);
    }

    [Test]
    public async Task CreateParent_DuplicateEmail_ThrowsException()
    {
        // Arrange
        var request = CreateValidCreateParentRequest();
        await _parentService.CreateAsync(request); // Create first parent

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _parentService.CreateAsync(request));
        Assert.That(exception?.Message, Does.Contain("already exists"));
    }

    [Test]
    public async Task GetParentById_ValidId_ReturnsParent()
    {
        // Arrange
        var request = CreateValidCreateParentRequest();
        var createdParent = await _parentService.CreateAsync(request);

        // Act
        var retrievedParent = await _parentService.GetByIdAsync(createdParent.Id);

        // Assert
        Assert.That(retrievedParent, Is.Not.Null);
        Assert.That(retrievedParent?.Id, Is.EqualTo(createdParent.Id));
        Assert.That(retrievedParent?.FirstName, Is.EqualTo(createdParent.FirstName));
    }

    [Test]
    public async Task GetParentById_InvalidId_ReturnsNull()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var result = await _parentService.GetByIdAsync(invalidId);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task UpdateParent_ValidRequest_UpdatesParentSuccessfully()
    {
        // Arrange
        var request = CreateValidCreateParentRequest();
        var createdParent = await _parentService.CreateAsync(request);
        var updateRequest = CreateValidUpdateParentRequest();
        updateRequest.FirstName = "UpdatedFirstName";

        // Act
        var updatedParent = await _parentService.UpdateAsync(createdParent.Id, updateRequest);

        // Assert
        Assert.That(updatedParent, Is.Not.Null);
        Assert.That(updatedParent.FirstName, Is.EqualTo("UpdatedFirstName"));
        Assert.That(updatedParent.UpdatedAt, Is.GreaterThan(createdParent.CreatedAt));

        // Verify in database
        var savedParent = await DbContext.Parents.FindAsync(createdParent.Id);
        Assert.That(savedParent?.FirstName, Is.EqualTo("UpdatedFirstName"));
    }

    [Test]
    public async Task DeleteParent_ValidId_SoftDeletesParent()
    {
        // Arrange
        var request = CreateValidCreateParentRequest();
        var createdParent = await _parentService.CreateAsync(request);

        // Act
        var result = await _parentService.DeleteAsync(createdParent.Id);

        // Assert
        Assert.That(result, Is.True);

        // Verify soft delete in database
        var deletedParent = await DbContext.Parents.FindAsync(createdParent.Id);
        Assert.That(deletedParent?.IsActive, Is.False);
    }

    #endregion

    #region Parent-Child Relationship Tests

    [Test]
    public async Task AddChildToParent_ValidRequest_EstablishesRelationship()
    {
        // Arrange
        var parent = await CreateTestParentAsync();
        
        // Debug: Check if parent was created successfully
        Assert.That(parent, Is.Not.Null, "Parent should not be null");
        Assert.That(parent.Id, Is.Not.EqualTo(Guid.Empty), "Parent ID should not be empty");
        
        var student = await CreateTestStudentAsync();
        
        // Debug: Check if student was created successfully
        Assert.That(student, Is.Not.Null, "Student should not be null");
        Assert.That(student.Id, Is.Not.EqualTo(Guid.Empty), "Student ID should not be empty");

        // Act
        var result = await _parentService.AddChildAsync(parent.Id, student.Id);

        // Assert
        Assert.That(result, Is.True);

        // Verify relationship in database
        var updatedStudent = await DbContext.Students.FindAsync(student.Id);
        Assert.That(updatedStudent?.ParentId, Is.EqualTo(parent.Id));

        var parentWithChildren = await _parentService.GetWithChildrenByIdAsync(parent.Id);
        Assert.That(parentWithChildren?.Children.Count(), Is.EqualTo(1));
        Assert.That(parentWithChildren?.Children.First().Id, Is.EqualTo(student.Id));
    }

    [Test]
    public async Task AddChildToParent_StudentAlreadyHasParent_ThrowsException()
    {
        // Arrange
        var parent1 = await CreateTestParentAsync();
        var parent2 = await CreateTestParentAsync();
        var student = await CreateTestStudentAsync();

        // Add student to first parent
        await _parentService.AddChildAsync(parent1.Id, student.Id);

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _parentService.AddChildAsync(parent2.Id, student.Id));
        Assert.That(exception?.Message, Does.Contain("already has a parent assigned"));
    }

    [Test]
    public async Task RemoveChildFromParent_ValidRequest_RemovesRelationship()
    {
        // Arrange
        var parent = await CreateTestParentAsync();
        var student = await CreateTestStudentAsync();
        await _parentService.AddChildAsync(parent.Id, student.Id);

        // Act
        var result = await _parentService.RemoveChildAsync(parent.Id, student.Id);

        // Assert
        Assert.That(result, Is.True);

        // Verify relationship removed in database
        var updatedStudent = await DbContext.Students.FindAsync(student.Id);
        Assert.That(updatedStudent?.ParentId, Is.Null);

        var parentWithChildren = await _parentService.GetWithChildrenByIdAsync(parent.Id);
        Assert.That(parentWithChildren?.Children.Count(), Is.EqualTo(0));
    }

    #endregion

    #region Parent Search and Filter Tests

    [Test]
    public async Task SearchParentsByName_ValidQuery_ReturnsMatchingParents()
    {
        // Arrange
        var parent1 = await CreateTestParentAsync("John", "Doe");
        var parent2 = await CreateTestParentAsync("Jane", "Smith");
        var parent3 = await CreateTestParentAsync("Bob", "Johnson");

        // Act
        var searchResults = await _parentService.SearchByNameAsync("John");

        // Assert
        Assert.That(searchResults.Count(), Is.EqualTo(2)); // John Doe and Bob Johnson
        Assert.That(searchResults.Any(p => p.Id == parent1.Id), Is.True);
        Assert.That(searchResults.Any(p => p.Id == parent3.Id), Is.True);
    }

    [Test]
    public async Task GetParentsByType_ValidType_ReturnsMatchingParents()
    {
        // Arrange
        var primaryParent = await CreateTestParentAsync("John", "Doe", ParentType.Primary);
        var secondaryParent = await CreateTestParentAsync("Jane", "Doe", ParentType.Secondary);
        var guardian = await CreateTestParentAsync("Bob", "Guardian", ParentType.Guardian);

        // Act
        var primaryParents = await _parentService.GetByTypeAsync(ParentType.Primary);
        var secondaryParents = await _parentService.GetByTypeAsync(ParentType.Secondary);
        var guardians = await _parentService.GetByTypeAsync(ParentType.Guardian);

        // Assert
        Assert.That(primaryParents.Count(), Is.EqualTo(1));
        Assert.That(primaryParents.First().Id, Is.EqualTo(primaryParent.Id));
        
        Assert.That(secondaryParents.Count(), Is.EqualTo(1));
        Assert.That(secondaryParents.First().Id, Is.EqualTo(secondaryParent.Id));
        
        Assert.That(guardians.Count(), Is.EqualTo(1));
        Assert.That(guardians.First().Id, Is.EqualTo(guardian.Id));
    }

    [Test]
    public async Task GetParentsByCity_ValidCity_ReturnsMatchingParents()
    {
        // Arrange
        var parent1 = await CreateTestParentAsync("John", "Doe", city: "New York");
        var parent2 = await CreateTestParentAsync("Jane", "Smith", city: "New York");
        var parent3 = await CreateTestParentAsync("Bob", "Johnson", city: "Los Angeles");

        // Act
        var newYorkParents = await _parentService.GetByCityAsync("New York");
        var laParents = await _parentService.GetByCityAsync("Los Angeles");

        // Assert
        Assert.That(newYorkParents.Count(), Is.EqualTo(2));
        Assert.That(laParents.Count(), Is.EqualTo(1));
    }

    #endregion

    #region Parent Statistics Tests

    [Test]
    public async Task GetParentStatistics_WithTestData_ReturnsAccurateStatistics()
    {
        // Arrange
        var parent1 = await CreateTestParentAsync("John", "Doe", ParentType.Primary);
        var parent2 = await CreateTestParentAsync("Jane", "Smith", ParentType.Secondary);
        var parent3 = await CreateTestParentAsync("Bob", "Guardian", ParentType.Guardian);

        var student1 = await CreateTestStudentAsync();
        var student2 = await CreateTestStudentAsync();

        // Add children to parents
        await _parentService.AddChildAsync(parent1.Id, student1.Id);
        await _parentService.AddChildAsync(parent2.Id, student2.Id);

        // Act
        var statistics = await _parentService.GetStatisticsAsync();

        // Assert
        Assert.That(statistics.TotalParents, Is.EqualTo(3));
        Assert.That(statistics.PrimaryParents, Is.EqualTo(1));
        Assert.That(statistics.SecondaryParents, Is.EqualTo(1));
        Assert.That(statistics.Guardians, Is.EqualTo(1));
        Assert.That(statistics.ParentsWithChildren, Is.EqualTo(2));
        Assert.That(statistics.AverageChildrenPerParent, Is.EqualTo(1));
    }

    #endregion

    #region Parent Portal Tests

    [Test]
    public async Task GetParentWithChildren_ValidParent_ReturnsChildrenInformation()
    {
        // Arrange
        var parent = await CreateTestParentAsync();
        var student1 = await CreateTestStudentAsync("Alice", "Doe");
        var student2 = await CreateTestStudentAsync("Bob", "Doe");

        await _parentService.AddChildAsync(parent.Id, student1.Id);
        await _parentService.AddChildAsync(parent.Id, student2.Id);

        // Act
        var parentWithChildren = await _parentService.GetWithChildrenByIdAsync(parent.Id);

        // Assert
        Assert.That(parentWithChildren, Is.Not.Null);
        Assert.That(parentWithChildren?.Children.Count(), Is.EqualTo(2));
        Assert.That(parentWithChildren?.ChildrenCount, Is.EqualTo(2));
        
        var child1 = parentWithChildren?.Children.FirstOrDefault(c => c.FirstName == "Alice");
        var child2 = parentWithChildren?.Children.FirstOrDefault(c => c.FirstName == "Bob");
        
        Assert.That(child1, Is.Not.Null);
        Assert.That(child2, Is.Not.Null);
        Assert.That(child1?.FullName, Is.EqualTo("Alice Doe"));
        Assert.That(child2?.FullName, Is.EqualTo("Bob Doe"));
    }

    #endregion

    #region Validation Tests

    [Test]
    public async Task ValidateParentRequest_InvalidData_ReturnsValidationErrors()
    {
        // Arrange
        var invalidRequest = new CreateParentRequest
        {
            FirstName = "", // Invalid: empty
            LastName = "", // Invalid: empty
            Email = "invalid-email", // Invalid: not a valid email
            PhoneNumber = "", // Invalid: empty
            Address = "", // Invalid: empty
            DateOfBirth = DateTime.Today.AddDays(1) // Invalid: future date
        };

        // Act
        var (isValid, errors) = await _parentService.ValidateAsync(invalidRequest);

        // Assert
        Assert.That(isValid, Is.False);
        Assert.That(errors, Has.Count.GreaterThan(0));
        Assert.That(errors.Any(e => e.Contains("FirstName is required")), Is.True);
        Assert.That(errors.Any(e => e.Contains("LastName is required")), Is.True);
        Assert.That(errors.Any(e => e.Contains("Email format is invalid")), Is.True);
        Assert.That(errors.Any(e => e.Contains("PhoneNumber is required")), Is.True);
        Assert.That(errors.Any(e => e.Contains("Address is required")), Is.True);
        Assert.That(errors.Any(e => e.Contains("DateOfBirth must be in the past")), Is.True);
    }

    [Test]
    public async Task ValidateParentRequest_ValidData_ReturnsNoErrors()
    {
        // Arrange
        var validRequest = CreateValidCreateParentRequest();

        // Act
        var (isValid, errors) = await _parentService.ValidateAsync(validRequest);

        // Assert
        Assert.That(isValid, Is.True);
        Assert.That(errors.Count(), Is.EqualTo(0));
    }

    #endregion

    #region Helper Methods

    private async Task<ParentResponse> CreateTestParentAsync(string firstName = "Test", string lastName = "Parent", ParentType parentType = ParentType.Primary, string city = "TestCity")
    {
        var request = new CreateParentRequest
        {
            FirstName = firstName,
            LastName = lastName,
            Email = $"test.parent.{Guid.NewGuid():N}@example.com",
            PhoneNumber = "+1234567890",
            DateOfBirth = new DateTime(1980, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Address = "123 Test St",
            City = city,
            State = "TS",
            PostalCode = "12345",
            Country = "USA",
            Gender = Gender.Other,
            ParentType = parentType,
            IsEmergencyContact = true,
            IsAuthorizedToPickup = true
        };

        var result = await _parentService.CreateAsync(request);
        
        // Ensure the result has a valid ID
        if (result?.Id == null || result.Id == Guid.Empty)
        {
            throw new InvalidOperationException($"Failed to create parent. Result: {result}");
        }
        
        return result;
    }

    private async Task<Student> CreateTestStudentAsync(string firstName = "Test", string lastName = "Student")
    {
        // Create a user first
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = $"{firstName.ToLower()}.{lastName.ToLower()}@example.com",
            Role = UserRole.Student,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var student = new Student
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            FirstName = firstName,
            LastName = lastName,
            Email = $"{firstName.ToLower()}.{lastName.ToLower()}@example.com",
            PhoneNumber = "+1234567890",
            DateOfBirth = new DateTime(2010, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Address = "123 Test St",
            Gender = Gender.Other,
            RollNumber = $"student_{Guid.NewGuid():N}",
            EnrollmentDate = DateTime.UtcNow.AddYears(-2),
            Status = StudentStatus.Active,
            Grade = "10",
            Section = "A",
            CreatedAt = DateTime.UtcNow
        };

        DbContext.Users.Add(user);
        DbContext.Students.Add(student);
        await DbContext.SaveChangesAsync();
        return student;
    }

    private static CreateParentRequest CreateValidCreateParentRequest()
    {
        return new CreateParentRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = $"john.doe.{Guid.NewGuid():N}@example.com",
            PhoneNumber = "+1234567890",
            DateOfBirth = new DateTime(1980, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Address = "123 Main St",
            City = "New York",
            State = "NY",
            PostalCode = "10001",
            Country = "USA",
            Gender = Gender.Male,
            ParentType = ParentType.Primary,
            IsEmergencyContact = true,
            IsAuthorizedToPickup = true
        };
    }

    private static UpdateParentRequest CreateValidUpdateParentRequest()
    {
        return new UpdateParentRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = $"john.doe.{Guid.NewGuid():N}@example.com",
            PhoneNumber = "+1234567890",
            DateOfBirth = new DateTime(1980, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Address = "123 Main St",
            City = "New York",
            State = "NY",
            PostalCode = "10001",
            Country = "USA",
            Gender = Gender.Male,
            ParentType = ParentType.Primary,
            IsEmergencyContact = true,
            IsAuthorizedToPickup = true
        };
    }

    private async Task SeedTestDataAsync()
    {
        // Add any additional test data if needed
        await DbContext.SaveChangesAsync();
    }

    #endregion
}
