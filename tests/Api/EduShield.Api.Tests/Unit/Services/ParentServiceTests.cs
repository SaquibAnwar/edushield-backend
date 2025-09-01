using Moq;
using NUnit.Framework;
using FluentAssertions;
using EduShield.Core.Services;
using EduShield.Core.Interfaces;
using EduShield.Core.Entities;
using EduShield.Core.Dtos;
using EduShield.Core.Enums;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EduShield.Api.Tests.Unit;

[TestFixture]
public class ParentServiceTests
{
    private Mock<IParentRepository> _mockParentRepository = null!;
    private Mock<IStudentRepository> _mockStudentRepository = null!;
    private Mock<IMapper> _mockMapper = null!;
    private Mock<IUserRepository> _mockUserRepository = null!;
    private ParentService _service = null!;

    [SetUp]
    public void Setup()
    {
        _mockParentRepository = new Mock<IParentRepository>();
        _mockStudentRepository = new Mock<IStudentRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockUserRepository = new Mock<IUserRepository>();

        _service = new ParentService(
            _mockParentRepository.Object,
            _mockStudentRepository.Object,
            _mockMapper.Object,
            _mockUserRepository.Object);
    }

    #region GetAllAsync Tests

    [Test]
    public async Task GetAllAsync_ReturnsAllParents()
    {
        // Arrange
        var parents = new List<Parent>
        {
            CreateTestParent(Guid.NewGuid()),
            CreateTestParent(Guid.NewGuid())
        };

        _mockParentRepository
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(parents);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        _mockParentRepository.Verify(x => x.GetAllAsync(), Times.Once);
    }

    [Test]
    public async Task GetAllAsync_NoParents_ReturnsEmptyList()
    {
        // Arrange
        _mockParentRepository
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Parent>());

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();

        _mockParentRepository.Verify(x => x.GetAllAsync(), Times.Once);
    }

    #endregion

    #region GetByIdAsync Tests

    [Test]
    public async Task GetByIdAsync_ValidId_ReturnsParent()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var parent = CreateTestParent(parentId);

        _mockParentRepository
            .Setup(x => x.GetByIdAsync(parentId))
            .ReturnsAsync(parent);

        // Act
        var result = await _service.GetByIdAsync(parentId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(parentId);

        _mockParentRepository.Verify(x => x.GetByIdAsync(parentId), Times.Once);
    }

    [Test]
    public async Task GetByIdAsync_InvalidId_ReturnsNull()
    {
        // Arrange
        var parentId = Guid.NewGuid();

        _mockParentRepository
            .Setup(x => x.GetByIdAsync(parentId))
            .ReturnsAsync((Parent?)null);

        // Act
        var result = await _service.GetByIdAsync(parentId);

        // Assert
        result.Should().BeNull();

        _mockParentRepository.Verify(x => x.GetByIdAsync(parentId), Times.Once);
    }

    #endregion

    #region GetByEmailAsync Tests

    [Test]
    public async Task GetByEmailAsync_ValidEmail_ReturnsParent()
    {
        // Arrange
        var email = "test@example.com";
        var parent = CreateTestParent(Guid.NewGuid());

        _mockParentRepository
            .Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync(parent);

        // Act
        var result = await _service.GetByEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(email);

        _mockParentRepository.Verify(x => x.GetByEmailAsync(email), Times.Once);
    }

    [Test]
    public async Task GetByEmailAsync_InvalidEmail_ReturnsNull()
    {
        // Arrange
        var email = "nonexistent@example.com";

        _mockParentRepository
            .Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync((Parent?)null);

        // Act
        var result = await _service.GetByEmailAsync(email);

        // Assert
        result.Should().BeNull();

        _mockParentRepository.Verify(x => x.GetByEmailAsync(email), Times.Once);
    }

    #endregion

    #region GetByUserIdAsync Tests

    [Test]
    public async Task GetByUserIdAsync_ValidUserId_ReturnsParent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var parent = CreateTestParent(Guid.NewGuid());
        parent.UserId = userId; // Set the UserId to match the test expectation

        _mockParentRepository
            .Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(parent);

        // Act
        var result = await _service.GetByUserIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be(userId);

        _mockParentRepository.Verify(x => x.GetByUserIdAsync(userId), Times.Once);
    }

    [Test]
    public async Task GetByUserIdAsync_InvalidUserId_ReturnsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockParentRepository
            .Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync((Parent?)null);

        // Act
        var result = await _service.GetByUserIdAsync(userId);

        // Assert
        result.Should().BeNull();

        _mockParentRepository.Verify(x => x.GetByUserIdAsync(userId), Times.Once);
    }

    #endregion

    #region GetWithChildrenByIdAsync Tests

    [Test]
    public async Task GetWithChildrenByIdAsync_ValidId_ReturnsParentWithChildren()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var parent = CreateTestParent(parentId);
        parent.Children = new List<Student> { CreateTestStudent(Guid.NewGuid()) };

        _mockParentRepository
            .Setup(x => x.GetWithChildrenByIdAsync(parentId))
            .ReturnsAsync(parent);

        // Act
        var result = await _service.GetWithChildrenByIdAsync(parentId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(parentId);

        _mockParentRepository.Verify(x => x.GetWithChildrenByIdAsync(parentId), Times.Once);
    }

    #endregion

    #region GetByTypeAsync Tests

    [Test]
    public async Task GetByTypeAsync_ValidType_ReturnsParents()
    {
        // Arrange
        var parentType = ParentType.Primary;
        var parents = new List<Parent>
        {
            CreateTestParent(Guid.NewGuid(), parentType),
            CreateTestParent(Guid.NewGuid(), parentType)
        };

        _mockParentRepository
            .Setup(x => x.GetByTypeAsync(parentType))
            .ReturnsAsync(parents);

        // Act
        var result = await _service.GetByTypeAsync(parentType);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        _mockParentRepository.Verify(x => x.GetByTypeAsync(parentType), Times.Once);
    }

    #endregion

    #region GetByCityAsync Tests

    [Test]
    public async Task GetByCityAsync_ValidCity_ReturnsParents()
    {
        // Arrange
        var city = "New York";
        var parents = new List<Parent>
        {
            CreateTestParent(Guid.NewGuid(), city: city),
            CreateTestParent(Guid.NewGuid(), city: city)
        };

        _mockParentRepository
            .Setup(x => x.GetByCityAsync(city))
            .ReturnsAsync(parents);

        // Act
        var result = await _service.GetByCityAsync(city);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        _mockParentRepository.Verify(x => x.GetByCityAsync(city), Times.Once);
    }

    #endregion

    #region GetByStateAsync Tests

    [Test]
    public async Task GetByStateAsync_ValidState_ReturnsParents()
    {
        // Arrange
        var state = "California";
        var parents = new List<Parent>
        {
            CreateTestParent(Guid.NewGuid(), state: state),
            CreateTestParent(Guid.NewGuid(), state: state)
        };

        _mockParentRepository
            .Setup(x => x.GetByStateAsync(state))
            .ReturnsAsync(parents);

        // Act
        var result = await _service.GetByStateAsync(state);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        _mockParentRepository.Verify(x => x.GetByStateAsync(state), Times.Once);
    }

    #endregion

    #region SearchByNameAsync Tests

    [Test]
    public async Task SearchByNameAsync_ValidSearchTerm_ReturnsParents()
    {
        // Arrange
        var searchTerm = "John";
        var parents = new List<Parent>
        {
            CreateTestParent(Guid.NewGuid(), firstName: "John"),
            CreateTestParent(Guid.NewGuid(), firstName: "Johnny")
        };

        _mockParentRepository
            .Setup(x => x.SearchByNameAsync(searchTerm))
            .ReturnsAsync(parents);

        // Act
        var result = await _service.SearchByNameAsync(searchTerm);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        _mockParentRepository.Verify(x => x.SearchByNameAsync(searchTerm), Times.Once);
    }

    [Test]
    public async Task SearchByNameAsync_EmptySearchTerm_ReturnsEmptyList()
    {
        // Arrange
        var searchTerm = "";

        // Act
        var result = await _service.SearchByNameAsync(searchTerm);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();

        _mockParentRepository.Verify(x => x.SearchByNameAsync(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task SearchByNameAsync_WhitespaceSearchTerm_ReturnsEmptyList()
    {
        // Arrange
        var searchTerm = "   ";

        // Act
        var result = await _service.SearchByNameAsync(searchTerm);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();

        _mockParentRepository.Verify(x => x.SearchByNameAsync(It.IsAny<string>()), Times.Never);
    }

    #endregion

    #region GetEmergencyContactsAsync Tests

    [Test]
    public async Task GetEmergencyContactsAsync_ReturnsEmergencyContacts()
    {
        // Arrange
        var parents = new List<Parent>
        {
            CreateTestParent(Guid.NewGuid(), isEmergencyContact: true),
            CreateTestParent(Guid.NewGuid(), isEmergencyContact: true)
        };

        _mockParentRepository
            .Setup(x => x.GetEmergencyContactsAsync())
            .ReturnsAsync(parents);

        // Act
        var result = await _service.GetEmergencyContactsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        _mockParentRepository.Verify(x => x.GetEmergencyContactsAsync(), Times.Once);
    }

    #endregion

    #region GetAuthorizedForPickupAsync Tests

    [Test]
    public async Task GetAuthorizedForPickupAsync_ReturnsAuthorizedParents()
    {
        // Arrange
        var parents = new List<Parent>
        {
            CreateTestParent(Guid.NewGuid(), isAuthorizedToPickup: true),
            CreateTestParent(Guid.NewGuid(), isAuthorizedToPickup: true)
        };

        _mockParentRepository
            .Setup(x => x.GetAuthorizedForPickupAsync())
            .ReturnsAsync(parents);

        // Act
        var result = await _service.GetAuthorizedForPickupAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        _mockParentRepository.Verify(x => x.GetAuthorizedForPickupAsync(), Times.Once);
    }

    #endregion

    #region CreateAsync Tests

    [Test]
    public async Task CreateAsync_ValidRequest_CreatesParentSuccessfully()
    {
        // Arrange
        var request = new CreateParentRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            PhoneNumber = "123-456-7890",
            DateOfBirth = DateTime.Today.AddYears(-35),
            Address = "123 Main St",
            City = "Test City",
            State = "Test State",
            PostalCode = "12345",
            Country = "USA",
            Gender = Gender.Male,
            ParentType = ParentType.Primary,
            IsActive = true
        };



        var user = CreateTestUser(Guid.NewGuid());
        var parent = CreateTestParent(Guid.NewGuid(), ParentType.Primary, "Test City", "Test State", false, true, request.FirstName, request.LastName);

        _mockUserRepository
            .Setup(x => x.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync(user);

        _mockParentRepository
            .Setup(x => x.AddAsync(It.IsAny<Parent>()))
            .ReturnsAsync(parent);

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be(request.FirstName);
        result.LastName.Should().Be(request.LastName);

        _mockUserRepository.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Once);
        _mockParentRepository.Verify(x => x.AddAsync(It.IsAny<Parent>()), Times.Once);
    }

    [Test]
    public async Task CreateAsync_ValidationFails_ThrowsArgumentException()
    {
        // Arrange
        var request = new CreateParentRequest
        {
            FirstName = "", // Invalid - empty first name
            LastName = "Doe",
            Email = "invalid-email", // Invalid email format
            PhoneNumber = "123-456-7890"
        };

        // Act & Assert
        var exception = Assert.ThrowsAsync<ArgumentException>(
            async () => await _service.CreateAsync(request));

        exception.Message.Should().Contain("Validation failed");

        _mockUserRepository.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Never);
        _mockParentRepository.Verify(x => x.AddAsync(It.IsAny<Parent>()), Times.Never);
    }

    #endregion

    #region UpdateAsync Tests

    [Test]
    public async Task UpdateAsync_ValidRequest_UpdatesParentSuccessfully()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var existingParent = CreateTestParent(parentId);
        var updatedParent = CreateTestParent(parentId);
        updatedParent.FirstName = "Updated";

        var request = new UpdateParentRequest
        {
            FirstName = "Updated",
            LastName = "Doe",
            Email = "updated@example.com",
            PhoneNumber = "123-456-7890",
            Address = "123 Main St",
            DateOfBirth = DateTime.Today.AddYears(-35)
        };

        _mockParentRepository
            .Setup(x => x.GetByIdAsync(parentId))
            .ReturnsAsync(existingParent);

        _mockParentRepository
            .Setup(x => x.EmailExistsAsync(request.Email, parentId))
            .ReturnsAsync(false);

        _mockParentRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Parent>()))
            .ReturnsAsync(updatedParent);

        // Act
        var result = await _service.UpdateAsync(parentId, request);

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be("Updated");

        _mockParentRepository.Verify(x => x.GetByIdAsync(parentId), Times.Once);
        _mockParentRepository.Verify(x => x.UpdateAsync(It.IsAny<Parent>()), Times.Once);
    }

    [Test]
    public async Task UpdateAsync_ParentNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var request = new UpdateParentRequest
        {
            FirstName = "Updated",
            LastName = "Doe",
            Email = "updated@example.com",
            PhoneNumber = "123-456-7890",
            Address = "123 Main St",
            DateOfBirth = DateTime.Today.AddYears(-35)
        };

        _mockParentRepository
            .Setup(x => x.GetByIdAsync(parentId))
            .ReturnsAsync((Parent?)null);

        // Act & Assert
        var exception = Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _service.UpdateAsync(parentId, request));

        exception.Message.Should().Contain("Parent with ID");
        exception.Message.Should().Contain("not found");

        _mockParentRepository.Verify(x => x.UpdateAsync(It.IsAny<Parent>()), Times.Never);
    }

    [Test]
    public async Task UpdateAsync_EmailAlreadyExists_ThrowsInvalidOperationException()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var existingParent = CreateTestParent(parentId);
        var request = new UpdateParentRequest
        {
            FirstName = "Updated",
            LastName = "Doe",
            Email = "existing@example.com",
            PhoneNumber = "123-456-7890",
            Address = "123 Main St",
            DateOfBirth = DateTime.Today.AddYears(-35)
        };

        _mockParentRepository
            .Setup(x => x.GetByIdAsync(parentId))
            .ReturnsAsync(existingParent);

        _mockParentRepository
            .Setup(x => x.EmailExistsAsync(request.Email, parentId))
            .ReturnsAsync(true);

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service.UpdateAsync(parentId, request));

        exception.Message.Should().Contain("Parent with email");
        exception.Message.Should().Contain("already exists");

        _mockParentRepository.Verify(x => x.UpdateAsync(It.IsAny<Parent>()), Times.Never);
    }

    #endregion

    #region DeleteAsync Tests

    [Test]
    public async Task DeleteAsync_ValidId_DeletesParentSuccessfully()
    {
        // Arrange
        var parentId = Guid.NewGuid();

        _mockParentRepository
            .Setup(x => x.DeleteAsync(parentId))
            .ReturnsAsync(true);

        // Act
        var result = await _service.DeleteAsync(parentId);

        // Assert
        result.Should().BeTrue();

        _mockParentRepository.Verify(x => x.DeleteAsync(parentId), Times.Once);
    }

    [Test]
    public async Task DeleteAsync_InvalidId_ReturnsFalse()
    {
        // Arrange
        var parentId = Guid.NewGuid();

        _mockParentRepository
            .Setup(x => x.DeleteAsync(parentId))
            .ReturnsAsync(false);

        // Act
        var result = await _service.DeleteAsync(parentId);

        // Assert
        result.Should().BeFalse();

        _mockParentRepository.Verify(x => x.DeleteAsync(parentId), Times.Once);
    }

    #endregion

    #region AddChildAsync Tests

    [Test]
    public async Task AddChildAsync_ValidIds_AddsChildSuccessfully()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var childId = Guid.NewGuid();
        var parent = CreateTestParent(parentId);
        var student = CreateTestStudent(childId);

        _mockParentRepository
            .Setup(x => x.GetByIdAsync(parentId))
            .ReturnsAsync(parent);

        _mockStudentRepository
            .Setup(x => x.GetByIdAsync(childId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(student);

        _mockParentRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Parent>()))
            .ReturnsAsync(parent);

        _mockStudentRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Student>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(student);

        // Act
        var result = await _service.AddChildAsync(parentId, childId);

        // Assert
        result.Should().BeTrue();

        _mockParentRepository.Verify(x => x.GetByIdAsync(parentId), Times.Once);
        _mockStudentRepository.Verify(x => x.GetByIdAsync(childId, It.IsAny<CancellationToken>()), Times.Once);
        _mockParentRepository.Verify(x => x.UpdateAsync(It.IsAny<Parent>()), Times.Once);
        _mockStudentRepository.Verify(x => x.UpdateAsync(It.IsAny<Student>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task AddChildAsync_ParentNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var childId = Guid.NewGuid();

        _mockParentRepository
            .Setup(x => x.GetByIdAsync(parentId))
            .ReturnsAsync((Parent?)null);

        // Act & Assert
        var exception = Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _service.AddChildAsync(parentId, childId));

        exception.Message.Should().Contain("Parent with ID");
        exception.Message.Should().Contain("not found");

        _mockStudentRepository.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task AddChildAsync_StudentNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var childId = Guid.NewGuid();
        var parent = CreateTestParent(parentId);

        _mockParentRepository
            .Setup(x => x.GetByIdAsync(parentId))
            .ReturnsAsync(parent);

        _mockStudentRepository
            .Setup(x => x.GetByIdAsync(childId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Student?)null);

        // Act & Assert
        var exception = Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _service.AddChildAsync(parentId, childId));

        exception.Message.Should().Contain("Student with ID");
        exception.Message.Should().Contain("not found");

        _mockParentRepository.Verify(x => x.UpdateAsync(It.IsAny<Parent>()), Times.Never);
    }

    [Test]
    public async Task AddChildAsync_StudentAlreadyHasParent_ThrowsInvalidOperationException()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var childId = Guid.NewGuid();
        var parent = CreateTestParent(parentId);
        var student = CreateTestStudent(childId);
        student.ParentId = Guid.NewGuid(); // Already has a parent

        _mockParentRepository
            .Setup(x => x.GetByIdAsync(parentId))
            .ReturnsAsync(parent);

        _mockStudentRepository
            .Setup(x => x.GetByIdAsync(childId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(student);

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service.AddChildAsync(parentId, childId));

        exception.Message.Should().Contain("already has a parent assigned");

        _mockParentRepository.Verify(x => x.UpdateAsync(It.IsAny<Parent>()), Times.Never);
        _mockStudentRepository.Verify(x => x.UpdateAsync(It.IsAny<Student>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region RemoveChildAsync Tests

    [Test]
    public async Task RemoveChildAsync_ValidIds_RemovesChildSuccessfully()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var childId = Guid.NewGuid();
        var parent = CreateTestParent(parentId);
        var student = CreateTestStudent(childId);

        parent.Children = new List<Student> { student };

        _mockParentRepository
            .Setup(x => x.GetByIdAsync(parentId))
            .ReturnsAsync(parent);

        _mockStudentRepository
            .Setup(x => x.GetByIdAsync(childId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(student);

        _mockParentRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Parent>()))
            .ReturnsAsync(parent);

        _mockStudentRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Student>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(student);

        // Act
        var result = await _service.RemoveChildAsync(parentId, childId);

        // Assert
        result.Should().BeTrue();

        _mockParentRepository.Verify(x => x.GetByIdAsync(parentId), Times.Once);
        _mockParentRepository.Verify(x => x.UpdateAsync(It.IsAny<Parent>()), Times.Once);
        _mockStudentRepository.Verify(x => x.UpdateAsync(It.IsAny<Student>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task RemoveChildAsync_ParentNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var childId = Guid.NewGuid();

        _mockParentRepository
            .Setup(x => x.GetByIdAsync(parentId))
            .ReturnsAsync((Parent?)null);

        // Act & Assert
        var exception = Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _service.RemoveChildAsync(parentId, childId));

        exception.Message.Should().Contain("Parent with ID");
        exception.Message.Should().Contain("not found");

        _mockParentRepository.Verify(x => x.UpdateAsync(It.IsAny<Parent>()), Times.Never);
    }

    [Test]
    public async Task RemoveChildAsync_ParentDoesNotHaveChild_ThrowsInvalidOperationException()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var childId = Guid.NewGuid();
        var parent = CreateTestParent(parentId);
        parent.Children = new List<Student>(); // No children

        _mockParentRepository
            .Setup(x => x.GetByIdAsync(parentId))
            .ReturnsAsync(parent);

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service.RemoveChildAsync(parentId, childId));

        exception.Message.Should().Contain("Parent does not have child with ID");

        _mockParentRepository.Verify(x => x.UpdateAsync(It.IsAny<Parent>()), Times.Never);
    }

    #endregion

    #region GetStatisticsAsync Tests

    [Test]
    public async Task GetStatisticsAsync_ReturnsStatistics()
    {
        // Arrange
        var statistics = new ParentStatistics
        {
            TotalParents = 100,
            ActiveParents = 95,
            PrimaryParents = 80,
            SecondaryParents = 20
        };

        _mockParentRepository
            .Setup(x => x.GetStatisticsAsync())
            .ReturnsAsync(statistics);

        // Act
        var result = await _service.GetStatisticsAsync();

        // Assert
        result.Should().NotBeNull();
        result.TotalParents.Should().Be(100);
        result.ActiveParents.Should().Be(95);

        _mockParentRepository.Verify(x => x.GetStatisticsAsync(), Times.Once);
    }

    #endregion

    #region ValidateAsync Tests

    [Test]
    public async Task ValidateAsync_ValidRequest_ReturnsValid()
    {
        // Arrange
        var request = new CreateParentRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            PhoneNumber = "123-456-7890",
            Address = "123 Main St",
            DateOfBirth = DateTime.Today.AddYears(-35)
        };

        // Act
        var (isValid, errors) = await _service.ValidateAsync(request);

        // Assert
        isValid.Should().BeTrue();
        errors.Should().BeEmpty();
    }

    [Test]
    public async Task ValidateAsync_InvalidRequest_ReturnsErrors()
    {
        // Arrange
        var request = new CreateParentRequest
        {
            FirstName = "", // Invalid
            LastName = "", // Invalid
            Email = "invalid-email", // Invalid
            PhoneNumber = "", // Invalid
            Address = "", // Invalid
            DateOfBirth = DateTime.Today.AddDays(1) // Invalid - future date
        };

        // Act
        var (isValid, errors) = await _service.ValidateAsync(request);

        // Assert
        isValid.Should().BeFalse();
        errors.Should().NotBeEmpty();
        errors.Should().Contain(e => e.Contains("FirstName is required"));
        errors.Should().Contain(e => e.Contains("LastName is required"));
        errors.Should().Contain(e => e.Contains("Email format is invalid"));
        errors.Should().Contain(e => e.Contains("PhoneNumber is required"));
        errors.Should().Contain(e => e.Contains("Address is required"));
        errors.Should().Contain(e => e.Contains("DateOfBirth must be in the past"));
    }

    #endregion

    #region ValidateUpdateAsync Tests

    [Test]
    public async Task ValidateUpdateAsync_ValidRequest_ReturnsValid()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var request = new UpdateParentRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            PhoneNumber = "123-456-7890",
            Address = "123 Main St",
            DateOfBirth = DateTime.Today.AddYears(-35)
        };

        // Act
        var (isValid, errors) = await _service.ValidateUpdateAsync(parentId, request);

        // Assert
        isValid.Should().BeTrue();
        errors.Should().BeEmpty();
    }

    [Test]
    public async Task ValidateUpdateAsync_InvalidRequest_ReturnsErrors()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var request = new UpdateParentRequest
        {
            FirstName = "", // Invalid
            LastName = "", // Invalid
            Email = "invalid-email", // Invalid
            PhoneNumber = "", // Invalid
            Address = "", // Invalid
            DateOfBirth = DateTime.Today.AddDays(1) // Invalid - future date
        };

        // Act
        var (isValid, errors) = await _service.ValidateUpdateAsync(parentId, request);

        // Assert
        isValid.Should().BeFalse();
        errors.Should().NotBeEmpty();
        errors.Should().Contain(e => e.Contains("FirstName is required"));
        errors.Should().Contain(e => e.Contains("LastName is required"));
        errors.Should().Contain(e => e.Contains("Email format is invalid"));
        errors.Should().Contain(e => e.Contains("PhoneNumber is required"));
        errors.Should().Contain(e => e.Contains("Address is required"));
        errors.Should().Contain(e => e.Contains("DateOfBirth must be in the past"));
    }

    #endregion

    #region Helper Methods

    private static Parent CreateTestParent(Guid id, ParentType parentType = ParentType.Primary, string city = "Test City", string state = "Test State", bool isEmergencyContact = false, bool isAuthorizedToPickup = false, string firstName = "Test", string lastName = "Parent")
    {
        return new Parent
        {
            Id = id,
            FirstName = firstName,
            LastName = lastName,
            Email = "test@example.com",
            PhoneNumber = "123-456-7890",
            ParentType = parentType,
            City = city,
            State = state,
            IsEmergencyContact = isEmergencyContact,
            IsAuthorizedToPickup = isAuthorizedToPickup,
            IsActive = true,
            UserId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Children = new List<Student>()
        };
    }

    private static Student CreateTestStudent(Guid id)
    {
        return new Student
        {
            Id = id,
            FirstName = "Test",
            LastName = "Student",
            RollNumber = "ST001",
            Email = "student@example.com",
            ParentId = null
        };
    }

    private static User CreateTestUser(Guid id)
    {
        return new User
        {
            Id = id,
            Email = "test@example.com",
            Role = UserRole.Parent,
            IsActive = true
        };
    }

    #endregion
}
