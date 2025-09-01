using Moq;
using NUnit.Framework;
using FluentAssertions;
using EduShield.Core.Services;
using EduShield.Core.Interfaces;
using EduShield.Core.Entities;
using EduShield.Core.Dtos;
using EduShield.Core.Enums;
using EduShield.Api.Tests.Fixtures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EduShield.Api.Tests.Unit;

[TestFixture]
public class FacultyServiceTests : BaseTestFixture
{
    private Mock<IFacultyRepository> _mockFacultyRepository = null!;
    private Mock<IUserRepository> _mockUserRepository = null!;
    private FacultyService _facultyService = null!;

    [SetUp]
    public override void Setup()
    {
        base.Setup();
        
        _mockFacultyRepository = MockRepository.Create<IFacultyRepository>();
        _mockUserRepository = MockRepository.Create<IUserRepository>();
        
        _facultyService = new FacultyService(_mockFacultyRepository.Object, _mockUserRepository.Object);
    }

    #region CreateAsync Tests

    [Test]
    public async Task CreateAsync_ValidRequest_CreatesFacultySuccessfully()
    {
        // Arrange
        var request = CreateValidCreateFacultyRequest();
        var expectedUser = CreateTestUser(UserRole.Faculty);
        var expectedFaculty = CreateTestFaculty(expectedUser.Id);
        // Override the faculty data to match the request
        expectedFaculty.FirstName = request.FirstName;
        expectedFaculty.LastName = request.LastName;
        expectedFaculty.Email = request.Email;
        expectedFaculty.PhoneNumber = request.PhoneNumber;
        expectedFaculty.DateOfBirth = request.DateOfBirth;
        expectedFaculty.Address = request.Address;
        expectedFaculty.Gender = request.Gender;
        expectedFaculty.Department = request.Department;
        expectedFaculty.Subject = request.Subject;
        expectedFaculty.HireDate = request.HireDate;

        _mockUserRepository
            .Setup(x => x.ExistsAsync(request.Email))
            .ReturnsAsync(false);

        _mockFacultyRepository
            .Setup(x => x.EmailExistsAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockUserRepository
            .Setup(x => x.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync(expectedUser);

        _mockFacultyRepository
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Faculty>());

        _mockFacultyRepository
            .Setup(x => x.CreateAsync(It.IsAny<Faculty>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedFaculty);

        // Act
        var result = await _facultyService.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be(request.FirstName);
        result.LastName.Should().Be(request.LastName);
        result.Email.Should().Be(request.Email);
        result.PhoneNumber.Should().Be(request.PhoneNumber);
        result.DateOfBirth.Should().Be(request.DateOfBirth);
        result.Address.Should().Be(request.Address);
        result.Gender.Should().Be(request.Gender);
        result.Department.Should().Be(request.Department);
        result.Subject.Should().Be(request.Subject);
        result.HireDate.Should().Be(request.HireDate);
        result.IsActive.Should().BeTrue();
        result.UserId.Should().Be(expectedFaculty.UserId);

        _mockUserRepository.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Once);
        _mockFacultyRepository.Verify(x => x.CreateAsync(It.IsAny<Faculty>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task CreateAsync_DuplicateEmailInUserTable_ThrowsException()
    {
        // Arrange
        var request = CreateValidCreateFacultyRequest();

        _mockUserRepository
            .Setup(x => x.ExistsAsync(request.Email))
            .ReturnsAsync(true);

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _facultyService.CreateAsync(request));

        exception.Message.Should().Contain("already exists");
        _mockUserRepository.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Never);
        _mockFacultyRepository.Verify(x => x.CreateAsync(It.IsAny<Faculty>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task CreateAsync_DuplicateEmailInFacultyTable_ThrowsException()
    {
        // Arrange
        var request = CreateValidCreateFacultyRequest();

        _mockUserRepository
            .Setup(x => x.ExistsAsync(request.Email))
            .ReturnsAsync(false);

        _mockFacultyRepository
            .Setup(x => x.EmailExistsAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _facultyService.CreateAsync(request));

        exception.Message.Should().Contain("already exists");
        _mockUserRepository.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Never);
        _mockFacultyRepository.Verify(x => x.CreateAsync(It.IsAny<Faculty>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task CreateAsync_FutureHireDate_ThrowsException()
    {
        // Arrange
        var request = CreateValidCreateFacultyRequest();
        request.HireDate = DateTime.Today.AddDays(1);

        // Setup mock to avoid strict mock failures
        _mockUserRepository
            .Setup(x => x.ExistsAsync(request.Email))
            .ReturnsAsync(false);

        _mockFacultyRepository
            .Setup(x => x.EmailExistsAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _facultyService.CreateAsync(request));

        exception.Message.Should().Contain("cannot be in the future");
    }

    [Test]
    public async Task CreateAsync_FutureDateOfBirth_ThrowsException()
    {
        // Arrange
        var request = CreateValidCreateFacultyRequest();
        request.DateOfBirth = DateTime.Today.AddDays(1);

        // Setup mock to avoid strict mock failures
        _mockUserRepository
            .Setup(x => x.ExistsAsync(request.Email))
            .ReturnsAsync(false);

        _mockFacultyRepository
            .Setup(x => x.EmailExistsAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _facultyService.CreateAsync(request));

        exception.Message.Should().Contain("cannot be in the future");
    }

    [Test]
    public async Task CreateAsync_UnderMinimumAge_ThrowsException()
    {
        // Arrange
        var request = CreateValidCreateFacultyRequest();
        request.DateOfBirth = DateTime.Today.AddYears(-17); // 17 years old

        // Setup mock to avoid strict mock failures
        _mockUserRepository
            .Setup(x => x.ExistsAsync(request.Email))
            .ReturnsAsync(false);

        _mockFacultyRepository
            .Setup(x => x.EmailExistsAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _facultyService.CreateAsync(request));

        exception.Message.Should().Contain("at least 18 years old");
    }

    #endregion

    #region GetByIdAsync Tests

    [Test]
    public async Task GetByIdAsync_ValidId_ReturnsFaculty()
    {
        // Arrange
        var facultyId = Guid.NewGuid();
        var expectedFaculty = CreateTestFaculty(facultyId);

        _mockFacultyRepository
            .Setup(x => x.GetByIdAsync(facultyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedFaculty);

        // Act
        var result = await _facultyService.GetByIdAsync(facultyId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(facultyId);
        result.FirstName.Should().Be(expectedFaculty.FirstName);
        result.LastName.Should().Be(expectedFaculty.LastName);
    }

    [Test]
    public async Task GetByIdAsync_InvalidId_ReturnsNull()
    {
        // Arrange
        var facultyId = Guid.NewGuid();

        _mockFacultyRepository
            .Setup(x => x.GetByIdAsync(facultyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Faculty?)null);

        // Act
        var result = await _facultyService.GetByIdAsync(facultyId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetByEmailAsync Tests

    [Test]
    public async Task GetByEmailAsync_ValidEmail_ReturnsFaculty()
    {
        // Arrange
        var email = "test@example.com";
        var expectedFaculty = CreateTestFaculty();
        expectedFaculty.Email = email; // Override the email to match the test

        _mockFacultyRepository
            .Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedFaculty);

        // Act
        var result = await _facultyService.GetByEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(email);
    }

    [Test]
    public async Task GetByEmailAsync_InvalidEmail_ReturnsNull()
    {
        // Arrange
        var email = "nonexistent@example.com";

        _mockFacultyRepository
            .Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Faculty?)null);

        // Act
        var result = await _facultyService.GetByEmailAsync(email);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetByEmployeeIdAsync Tests

    [Test]
    public async Task GetByEmployeeIdAsync_ValidEmployeeId_ReturnsFaculty()
    {
        // Arrange
        var employeeId = "FAC001";
        var expectedFaculty = CreateTestFaculty();
        expectedFaculty.EmployeeId = employeeId; // Override the employee ID to match the test

        _mockFacultyRepository
            .Setup(x => x.GetByEmployeeIdAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedFaculty);

        // Act
        var result = await _facultyService.GetByEmployeeIdAsync(employeeId);

        // Assert
        result.Should().NotBeNull();
        result!.EmployeeId.Should().Be(employeeId);
    }

    [Test]
    public async Task GetByEmployeeIdAsync_InvalidEmployeeId_ReturnsNull()
    {
        // Arrange
        var employeeId = "INVALID";

        _mockFacultyRepository
            .Setup(x => x.GetByEmployeeIdAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Faculty?)null);

        // Act
        var result = await _facultyService.GetByEmployeeIdAsync(employeeId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetAllAsync Tests

    [Test]
    public async Task GetAllAsync_ReturnsAllFaculty()
    {
        // Arrange
        var expectedFaculty = new List<Faculty>
        {
            CreateTestFaculty(),
            CreateTestFaculty()
        };

        _mockFacultyRepository
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedFaculty);

        // Act
        var result = await _facultyService.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }

    #endregion

    #region GetByDepartmentAsync Tests

    [Test]
    public async Task GetByDepartmentAsync_ValidDepartment_ReturnsFaculty()
    {
        // Arrange
        var department = "Computer Science";
        var expectedFaculty = new List<Faculty>
        {
            CreateTestFaculty(department: department),
            CreateTestFaculty(department: department)
        };

        _mockFacultyRepository
            .Setup(x => x.GetByDepartmentAsync(department, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedFaculty);

        // Act
        var result = await _facultyService.GetByDepartmentAsync(department);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(f => f.Department.Should().Be(department));
    }

    #endregion

    #region GetBySubjectAsync Tests

    [Test]
    public async Task GetBySubjectAsync_ValidSubject_ReturnsFaculty()
    {
        // Arrange
        var subject = "Mathematics";
        var expectedFaculty = new List<Faculty>
        {
            CreateTestFaculty(subject: subject),
            CreateTestFaculty(subject: subject)
        };

        _mockFacultyRepository
            .Setup(x => x.GetBySubjectAsync(subject, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedFaculty);

        // Act
        var result = await _facultyService.GetBySubjectAsync(subject);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(f => f.Subject.Should().Be(subject));
    }

    #endregion

    #region GetActiveAsync Tests

    [Test]
    public async Task GetActiveAsync_ReturnsActiveFaculty()
    {
        // Arrange
        var expectedFaculty = new List<Faculty>
        {
            CreateTestFaculty(isActive: true),
            CreateTestFaculty(isActive: true)
        };

        _mockFacultyRepository
            .Setup(x => x.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedFaculty);

        // Act
        var result = await _facultyService.GetActiveAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(f => f.IsActive.Should().BeTrue());
    }

    #endregion

    #region UpdateAsync Tests

    [Test]
    public async Task UpdateAsync_ValidRequest_UpdatesFacultySuccessfully()
    {
        // Arrange
        var facultyId = Guid.NewGuid();
        var existingFaculty = CreateTestFaculty(facultyId);
        var request = CreateValidUpdateFacultyRequest();

        _mockFacultyRepository
            .Setup(x => x.GetByIdAsync(facultyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingFaculty);

        _mockFacultyRepository
            .Setup(x => x.EmailExistsAsync(request.Email!, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockFacultyRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Faculty>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingFaculty);

        // Act
        var result = await _facultyService.UpdateAsync(facultyId, request);

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be(request.FirstName);
        result.LastName.Should().Be(request.LastName);
        result.Email.Should().Be(request.Email);

        _mockFacultyRepository.Verify(x => x.UpdateAsync(It.IsAny<Faculty>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task UpdateAsync_FacultyNotFound_ThrowsException()
    {
        // Arrange
        var facultyId = Guid.NewGuid();
        var request = CreateValidUpdateFacultyRequest();

        _mockFacultyRepository
            .Setup(x => x.GetByIdAsync(facultyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Faculty?)null);

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _facultyService.UpdateAsync(facultyId, request));

        exception.Message.Should().Contain("not found");
        _mockFacultyRepository.Verify(x => x.UpdateAsync(It.IsAny<Faculty>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task UpdateAsync_DuplicateEmail_ThrowsException()
    {
        // Arrange
        var facultyId = Guid.NewGuid();
        var existingFaculty = CreateTestFaculty(facultyId);
        var request = CreateValidUpdateFacultyRequest();
        request.Email = "duplicate@example.com";

        _mockFacultyRepository
            .Setup(x => x.GetByIdAsync(facultyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingFaculty);

        _mockFacultyRepository
            .Setup(x => x.EmailExistsAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _facultyService.UpdateAsync(facultyId, request));

        exception.Message.Should().Contain("already exists");
        _mockFacultyRepository.Verify(x => x.UpdateAsync(It.IsAny<Faculty>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task UpdateAsync_FutureHireDate_ThrowsException()
    {
        // Arrange
        var facultyId = Guid.NewGuid();
        var existingFaculty = CreateTestFaculty(facultyId);
        var request = CreateValidUpdateFacultyRequest();
        request.HireDate = DateTime.Today.AddDays(1);

        _mockFacultyRepository
            .Setup(x => x.GetByIdAsync(facultyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingFaculty);

        _mockFacultyRepository
            .Setup(x => x.EmailExistsAsync(request.Email!, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _facultyService.UpdateAsync(facultyId, request));

        exception.Message.Should().Contain("cannot be in the future");
    }

    [Test]
    public async Task UpdateAsync_FutureDateOfBirth_ThrowsException()
    {
        // Arrange
        var facultyId = Guid.NewGuid();
        var existingFaculty = CreateTestFaculty(facultyId);
        var request = CreateValidUpdateFacultyRequest();
        request.DateOfBirth = DateTime.Today.AddDays(1);

        _mockFacultyRepository
            .Setup(x => x.GetByIdAsync(facultyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingFaculty);

        _mockFacultyRepository
            .Setup(x => x.EmailExistsAsync(request.Email!, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _facultyService.UpdateAsync(facultyId, request));

        exception.Message.Should().Contain("cannot be in the future");
    }

    [Test]
    public async Task UpdateAsync_UnderMinimumAge_ThrowsException()
    {
        // Arrange
        var facultyId = Guid.NewGuid();
        var existingFaculty = CreateTestFaculty(facultyId);
        var request = CreateValidUpdateFacultyRequest();
        request.DateOfBirth = DateTime.Today.AddYears(-17); // 17 years old

        _mockFacultyRepository
            .Setup(x => x.GetByIdAsync(facultyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingFaculty);

        _mockFacultyRepository
            .Setup(x => x.EmailExistsAsync(request.Email!, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _facultyService.UpdateAsync(facultyId, request));

        exception.Message.Should().Contain("at least 18 years old");
    }

    [Test]
    public async Task UpdateAsync_InvalidUserId_ThrowsException()
    {
        // Arrange
        var facultyId = Guid.NewGuid();
        var existingFaculty = CreateTestFaculty(facultyId);
        var request = CreateValidUpdateFacultyRequest();
        var invalidUserId = Guid.NewGuid();
        request.UserId = invalidUserId;

        _mockFacultyRepository
            .Setup(x => x.GetByIdAsync(facultyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingFaculty);

        _mockFacultyRepository
            .Setup(x => x.EmailExistsAsync(request.Email!, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(invalidUserId))
            .ReturnsAsync((User?)null);

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _facultyService.UpdateAsync(facultyId, request));

        exception.Message.Should().Contain("not found");
        _mockFacultyRepository.Verify(x => x.UpdateAsync(It.IsAny<Faculty>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region DeleteAsync Tests

    [Test]
    public async Task DeleteAsync_ValidId_DeletesFaculty()
    {
        // Arrange
        var facultyId = Guid.NewGuid();

        _mockFacultyRepository
            .Setup(x => x.ExistsAsync(facultyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockFacultyRepository
            .Setup(x => x.DeleteAsync(facultyId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _facultyService.DeleteAsync(facultyId);

        // Assert
        _mockFacultyRepository.Verify(x => x.DeleteAsync(facultyId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task DeleteAsync_FacultyNotFound_ThrowsException()
    {
        // Arrange
        var facultyId = Guid.NewGuid();

        _mockFacultyRepository
            .Setup(x => x.ExistsAsync(facultyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _facultyService.DeleteAsync(facultyId));

        exception.Message.Should().Contain("not found");
        _mockFacultyRepository.Verify(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region ExistsAsync Tests

    [Test]
    public async Task ExistsAsync_FacultyExists_ReturnsTrue()
    {
        // Arrange
        var facultyId = Guid.NewGuid();

        _mockFacultyRepository
            .Setup(x => x.ExistsAsync(facultyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _facultyService.ExistsAsync(facultyId);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public async Task ExistsAsync_FacultyDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var facultyId = Guid.NewGuid();

        _mockFacultyRepository
            .Setup(x => x.ExistsAsync(facultyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _facultyService.ExistsAsync(facultyId);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region EmailExistsAsync Tests

    [Test]
    public async Task EmailExistsAsync_EmailExists_ReturnsTrue()
    {
        // Arrange
        var email = "test@example.com";

        _mockFacultyRepository
            .Setup(x => x.EmailExistsAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _facultyService.EmailExistsAsync(email);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public async Task EmailExistsAsync_EmailDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var email = "nonexistent@example.com";

        _mockFacultyRepository
            .Setup(x => x.EmailExistsAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _facultyService.EmailExistsAsync(email);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region EmployeeIdExistsAsync Tests

    [Test]
    public async Task EmployeeIdExistsAsync_EmployeeIdExists_ReturnsTrue()
    {
        // Arrange
        var employeeId = "FAC001";

        _mockFacultyRepository
            .Setup(x => x.EmployeeIdExistsAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _facultyService.EmployeeIdExistsAsync(employeeId);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public async Task EmployeeIdExistsAsync_EmployeeIdDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var employeeId = "INVALID";

        _mockFacultyRepository
            .Setup(x => x.EmployeeIdExistsAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _facultyService.EmployeeIdExistsAsync(employeeId);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GetAssignedStudentsAsync Tests

    [Test]
    public async Task GetAssignedStudentsAsync_ValidFacultyId_ReturnsAssignedStudents()
    {
        // Arrange
        var facultyId = Guid.NewGuid();
        var faculty = CreateTestFaculty(facultyId);
        faculty.StudentFaculties = new List<StudentFaculty>
        {
            CreateTestStudentFaculty(facultyId, isActive: true),
            CreateTestStudentFaculty(facultyId, isActive: true)
        };

        _mockFacultyRepository
            .Setup(x => x.GetByIdAsync(facultyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(faculty);

        // Act
        var result = await _facultyService.GetAssignedStudentsAsync(facultyId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }

    [Test]
    public async Task GetAssignedStudentsAsync_FacultyNotFound_ReturnsEmptyList()
    {
        // Arrange
        var facultyId = Guid.NewGuid();

        _mockFacultyRepository
            .Setup(x => x.GetByIdAsync(facultyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Faculty?)null);

        // Act
        var result = await _facultyService.GetAssignedStudentsAsync(facultyId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Test]
    public async Task GetAssignedStudentsAsync_NoAssignedStudents_ReturnsEmptyList()
    {
        // Arrange
        var facultyId = Guid.NewGuid();
        var faculty = CreateTestFaculty(facultyId);
        faculty.StudentFaculties = null;

        _mockFacultyRepository
            .Setup(x => x.GetByIdAsync(facultyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(faculty);

        // Act
        var result = await _facultyService.GetAssignedStudentsAsync(facultyId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    #endregion

    #region Helper Methods

    private static CreateFacultyRequest CreateValidCreateFacultyRequest()
    {
        return new CreateFacultyRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            PhoneNumber = "+1234567890",
            DateOfBirth = new DateTime(1980, 1, 1),
            Address = "123 Main St",
            Gender = Gender.Male,
            Department = "Computer Science",
            Subject = "Programming",
            HireDate = DateTime.Today.AddDays(-30)
        };
    }

    private static UpdateFacultyRequest CreateValidUpdateFacultyRequest()
    {
        return new UpdateFacultyRequest
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane.smith@example.com",
            PhoneNumber = "+0987654321",
            DateOfBirth = new DateTime(1985, 2, 2),
            Address = "456 Oak Ave",
            Gender = Gender.Female,
            Department = "Mathematics",
            Subject = "Calculus",
            HireDate = DateTime.Today.AddDays(-60),
            IsActive = true
        };
    }

    private static Faculty CreateTestFaculty(Guid? id = null, string? department = "Computer Science", string? subject = "Programming", bool isActive = true)
    {
        return new Faculty
        {
            Id = id ?? Guid.NewGuid(),
            FirstName = "Test",
            LastName = "Faculty",
            Email = "test.faculty@example.com",
            PhoneNumber = "+1234567890",
            DateOfBirth = new DateTime(1980, 1, 1),
            Address = "123 Test St",
            Gender = Gender.Other,
            Department = department,
            Subject = subject,
            EmployeeId = "TEST001",
            HireDate = DateTime.Today.AddYears(-5),
            IsActive = isActive,
            UserId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            StudentFaculties = new List<StudentFaculty>()
        };
    }

    private static StudentFaculty CreateTestStudentFaculty(Guid facultyId, bool isActive = true)
    {
        return new StudentFaculty
        {
            FacultyId = facultyId,
            StudentId = Guid.NewGuid(),
            Student = CreateTestStudent(),
            AssignedDate = DateTime.UtcNow,
            IsActive = isActive,
            Notes = "Test assignment"
        };
    }

    private static Student CreateTestStudent()
    {
        return new Student
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "Student",
            Email = "test.student@example.com",
            PhoneNumber = "+1234567890",
            DateOfBirth = new DateTime(2005, 1, 1),
            Address = "123 Test St",
            Gender = Gender.Other,
            RollNumber = "TEST001",
            EnrollmentDate = DateTime.Today.AddYears(-1),
            Status = StudentStatus.Active,
            Grade = "10",
            Section = "A",
            UserId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    #endregion
}
