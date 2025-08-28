using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using EduShield.Core.Dtos;
using EduShield.Core.Entities;
using EduShield.Core.Enums;
using EduShield.Core.Interfaces;
using EduShield.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using EduShield.Api.Tests.Fixtures;
using FluentAssertions;

namespace EduShield.Api.Tests.Integration;

[TestFixture]
public class ParentStudentBidirectionalTests : IntegrationTestFixture
{
    private IParentStudentAssignmentService _assignmentService;
    private IParentRepository _parentRepository;
    private IStudentRepository _studentRepository;

    [SetUp]
    public override void Setup()
    {
        base.Setup();
        
        var scope = Factory.Services.CreateScope();
        
        _assignmentService = scope.ServiceProvider.GetRequiredService<IParentStudentAssignmentService>();
        _parentRepository = scope.ServiceProvider.GetRequiredService<IParentRepository>();
        _studentRepository = scope.ServiceProvider.GetRequiredService<IStudentRepository>();
        
        SetupAuthenticatedClient(UserRole.Admin);
    }

    [Test]
    public async Task CreateAssignment_AsPrimaryContact_UpdatesStudentParentId()
    {
        // Arrange
        var parent = await CreateTestParentAsync();
        var student = await CreateTestStudentAsync();
        
        var createDto = new CreateParentStudentAssignmentDto
        {
            ParentId = parent.Id,
            StudentId = student.Id,
            Relationship = "Father",
            IsPrimaryContact = true,
            IsAuthorizedToPickup = true,
            IsEmergencyContact = true,
            Notes = "Test assignment"
        };

        // Act
        var assignment = await _assignmentService.CreateAssignmentAsync(createDto);

        // Assert
        assignment.Should().NotBeNull();
        assignment.IsPrimaryContact.Should().BeTrue();
        
        // Verify bidirectional relationship - Student.ParentId should be updated
        var updatedStudent = await _studentRepository.GetByIdAsync(student.Id);
        updatedStudent.Should().NotBeNull();
        updatedStudent!.ParentId.Should().Be(parent.Id);
        
        // Verify the assignment exists in ParentStudent table
        var assignmentFromDb = await _assignmentService.GetAssignmentAsync(parent.Id, student.Id);
        assignmentFromDb.Should().NotBeNull();
        assignmentFromDb!.IsPrimaryContact.Should().BeTrue();
    }

    [Test]
    public async Task CreateAssignment_AsNonPrimaryContact_DoesNotUpdateStudentParentIdIfAlreadySet()
    {
        // Arrange
        var parent1 = await CreateTestParentAsync("John", "Doe");
        var parent2 = await CreateTestParentAsync("Jane", "Doe");
        var student = await CreateTestStudentAsync();
        
        // First, create primary contact assignment
        var primaryAssignment = new CreateParentStudentAssignmentDto
        {
            ParentId = parent1.Id,
            StudentId = student.Id,
            Relationship = "Father",
            IsPrimaryContact = true,
            IsAuthorizedToPickup = true,
            IsEmergencyContact = true
        };
        await _assignmentService.CreateAssignmentAsync(primaryAssignment);
        
        // Now create secondary assignment
        var secondaryAssignment = new CreateParentStudentAssignmentDto
        {
            ParentId = parent2.Id,
            StudentId = student.Id,
            Relationship = "Mother",
            IsPrimaryContact = false,
            IsAuthorizedToPickup = true,
            IsEmergencyContact = true
        };

        // Act
        var assignment = await _assignmentService.CreateAssignmentAsync(secondaryAssignment);

        // Assert
        assignment.Should().NotBeNull();
        assignment.IsPrimaryContact.Should().BeFalse();
        
        // Verify Student.ParentId remains with primary contact
        var updatedStudent = await _studentRepository.GetByIdAsync(student.Id);
        updatedStudent.Should().NotBeNull();
        updatedStudent!.ParentId.Should().Be(parent1.Id); // Should still be parent1
    }

    [Test]
    public async Task SetPrimaryContact_UpdatesStudentParentId()
    {
        // Arrange
        var parent1 = await CreateTestParentAsync("John", "Doe");
        var parent2 = await CreateTestParentAsync("Jane", "Doe");
        var student = await CreateTestStudentAsync();
        
        // Create two assignments, both non-primary initially
        await _assignmentService.CreateAssignmentAsync(new CreateParentStudentAssignmentDto
        {
            ParentId = parent1.Id,
            StudentId = student.Id,
            Relationship = "Father",
            IsPrimaryContact = true,
            IsAuthorizedToPickup = true,
            IsEmergencyContact = true
        });
        
        await _assignmentService.CreateAssignmentAsync(new CreateParentStudentAssignmentDto
        {
            ParentId = parent2.Id,
            StudentId = student.Id,
            Relationship = "Mother",
            IsPrimaryContact = false,
            IsAuthorizedToPickup = true,
            IsEmergencyContact = true
        });

        // Act - Set parent2 as primary contact
        var result = await _assignmentService.SetPrimaryContactAsync(parent2.Id, student.Id);

        // Assert
        result.Should().BeTrue();
        
        // Verify Student.ParentId is updated to parent2
        var updatedStudent = await _studentRepository.GetByIdAsync(student.Id);
        updatedStudent.Should().NotBeNull();
        updatedStudent!.ParentId.Should().Be(parent2.Id);
        
        // Verify parent1 is no longer primary
        var parent1Assignment = await _assignmentService.GetAssignmentAsync(parent1.Id, student.Id);
        parent1Assignment!.IsPrimaryContact.Should().BeFalse();
        
        // Verify parent2 is now primary
        var parent2Assignment = await _assignmentService.GetAssignmentAsync(parent2.Id, student.Id);
        parent2Assignment!.IsPrimaryContact.Should().BeTrue();
    }

    [Test]
    public async Task DeleteAssignment_PrimaryContact_UpdatesStudentParentIdToNextParent()
    {
        // Arrange
        var parent1 = await CreateTestParentAsync("John", "Doe");
        var parent2 = await CreateTestParentAsync("Jane", "Doe");
        var student = await CreateTestStudentAsync();
        
        // Create primary assignment
        await _assignmentService.CreateAssignmentAsync(new CreateParentStudentAssignmentDto
        {
            ParentId = parent1.Id,
            StudentId = student.Id,
            Relationship = "Father",
            IsPrimaryContact = true,
            IsAuthorizedToPickup = true,
            IsEmergencyContact = true
        });
        
        // Create secondary assignment
        await _assignmentService.CreateAssignmentAsync(new CreateParentStudentAssignmentDto
        {
            ParentId = parent2.Id,
            StudentId = student.Id,
            Relationship = "Mother",
            IsPrimaryContact = false,
            IsAuthorizedToPickup = true,
            IsEmergencyContact = true
        });

        // Act - Delete primary contact
        var result = await _assignmentService.DeleteAssignmentAsync(parent1.Id, student.Id);

        // Assert
        result.Should().BeTrue();
        
        // Verify Student.ParentId is updated to parent2 (automatically promoted)
        var updatedStudent = await _studentRepository.GetByIdAsync(student.Id);
        updatedStudent.Should().NotBeNull();
        updatedStudent!.ParentId.Should().Be(parent2.Id);
        
        // Verify parent2 is now primary contact
        var parent2Assignment = await _assignmentService.GetAssignmentAsync(parent2.Id, student.Id);
        parent2Assignment!.IsPrimaryContact.Should().BeTrue();
        
        // Verify parent1 assignment is deleted
        var parent1Assignment = await _assignmentService.GetAssignmentAsync(parent1.Id, student.Id);
        parent1Assignment.Should().BeNull();
    }

    [Test]
    public async Task DeleteAssignment_LastParent_ClearsStudentParentId()
    {
        // Arrange
        var parent = await CreateTestParentAsync();
        var student = await CreateTestStudentAsync();
        
        // Create assignment
        await _assignmentService.CreateAssignmentAsync(new CreateParentStudentAssignmentDto
        {
            ParentId = parent.Id,
            StudentId = student.Id,
            Relationship = "Father",
            IsPrimaryContact = true,
            IsAuthorizedToPickup = true,
            IsEmergencyContact = true
        });

        // Act - Delete the only parent assignment
        var result = await _assignmentService.DeleteAssignmentAsync(parent.Id, student.Id);

        // Assert
        result.Should().BeTrue();
        
        // Verify Student.ParentId is cleared
        var updatedStudent = await _studentRepository.GetByIdAsync(student.Id);
        updatedStudent.Should().NotBeNull();
        updatedStudent!.ParentId.Should().BeNull();
    }

    [Test]
    public async Task SyncLegacyData_CreatesParentStudentRelationships()
    {
        // Arrange - Create a student with legacy ParentId but no ParentStudent relationship
        var parent = await CreateTestParentAsync();
        var student = await CreateTestStudentAsync();
        
        // Manually set the legacy ParentId without creating ParentStudent relationship
        student.ParentId = parent.Id;
        await _studentRepository.UpdateAsync(student);

        // Act
        var syncCount = await _assignmentService.SyncLegacyParentStudentDataAsync();

        // Assert
        syncCount.Should().BeGreaterThan(0);
        
        // Verify ParentStudent relationship was created
        var assignment = await _assignmentService.GetAssignmentAsync(parent.Id, student.Id);
        assignment.Should().NotBeNull();
        assignment!.IsPrimaryContact.Should().BeTrue();
        assignment.Relationship.Should().Be("Parent");
        assignment.Notes.Should().Contain("Synced from legacy");
    }

    [Test]
    public async Task StudentWithParents_ReturnsCorrectParentInformation()
    {
        // Arrange
        var parent1 = await CreateTestParentAsync("John", "Doe");
        var parent2 = await CreateTestParentAsync("Jane", "Doe");
        var student = await CreateTestStudentAsync();
        
        // Create assignments
        await _assignmentService.CreateAssignmentAsync(new CreateParentStudentAssignmentDto
        {
            ParentId = parent1.Id,
            StudentId = student.Id,
            Relationship = "Father",
            IsPrimaryContact = true,
            IsAuthorizedToPickup = true,
            IsEmergencyContact = true
        });
        
        await _assignmentService.CreateAssignmentAsync(new CreateParentStudentAssignmentDto
        {
            ParentId = parent2.Id,
            StudentId = student.Id,
            Relationship = "Mother",
            IsPrimaryContact = false,
            IsAuthorizedToPickup = true,
            IsEmergencyContact = true
        });

        // Act
        var studentWithParents = await _assignmentService.GetStudentWithParentsAsync(student.Id);

        // Assert
        studentWithParents.Should().NotBeNull();
        studentWithParents!.AssignedParents.Should().HaveCount(2);
        
        var primaryParent = studentWithParents.AssignedParents.FirstOrDefault(p => p.IsPrimaryContact);
        primaryParent.Should().NotBeNull();
        primaryParent!.ParentId.Should().Be(parent1.Id);
        primaryParent.Relationship.Should().Be("Father");
        
        var secondaryParent = studentWithParents.AssignedParents.FirstOrDefault(p => !p.IsPrimaryContact);
        secondaryParent.Should().NotBeNull();
        secondaryParent!.ParentId.Should().Be(parent2.Id);
        secondaryParent.Relationship.Should().Be("Mother");
        
        // Verify the student's legacy ParentId field is set correctly
        var updatedStudent = await _studentRepository.GetByIdAsync(student.Id);
        updatedStudent!.ParentId.Should().Be(parent1.Id); // Should be the primary contact
    }

    #region Helper Methods

    private async Task<Parent> CreateTestParentAsync(string firstName = "Test", string lastName = "Parent")
    {
        var parent = new Parent
        {
            Id = Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            Email = $"{firstName.ToLower()}.{lastName.ToLower()}.{Guid.NewGuid():N}@example.com",
            PhoneNumber = "+1234567890",
            DateOfBirth = new DateTime(1980, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Address = "123 Test St",
            City = "TestCity",
            State = "TS",
            PostalCode = "12345",
            Country = "USA",
            Gender = Gender.Other,
            ParentType = ParentType.Primary,
            IsEmergencyContact = true,
            IsAuthorizedToPickup = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        DbContext.Parents.Add(parent);
        await DbContext.SaveChangesAsync();
        return parent;
    }

    private async Task<Student> CreateTestStudentAsync(string firstName = "Test", string lastName = "Student")
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = $"{firstName.ToLower()}.{lastName.ToLower()}.{Guid.NewGuid():N}@example.com",
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
            Email = $"{firstName.ToLower()}.{lastName.ToLower()}.{Guid.NewGuid():N}@example.com",
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

    #endregion
}