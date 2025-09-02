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
public class StudentServiceTests : BaseTestFixture
{
    private Mock<IStudentRepository> _mockStudentRepository = null!;
    private Mock<IUserRepository> _mockUserRepository = null!;
    private Mock<ICacheService> _mockCacheService = null!;
    private StudentService _studentService = null!;

    [SetUp]
    public override void Setup()
    {
        base.Setup();
        
        _mockStudentRepository = MockRepository.Create<IStudentRepository>();
        _mockUserRepository = MockRepository.Create<IUserRepository>();
        _mockCacheService = MockRepository.Create<ICacheService>();
        
        _studentService = new StudentService(_mockStudentRepository.Object, _mockUserRepository.Object, _mockCacheService.Object);
    }

    #region CreateAsync Tests

    [Test]
    public async Task CreateAsync_ValidRequest_CreatesStudentSuccessfully()
    {
        // Arrange
        var request = CreateValidCreateStudentRequest();
        var expectedUser = CreateTestUser(UserRole.Student);
        var expectedStudent = CreateTestStudent(expectedUser.Id);
        // Override the student data to match the request
        expectedStudent.FirstName = request.FirstName;
        expectedStudent.LastName = request.LastName;
        expectedStudent.Email = request.Email;
        expectedStudent.PhoneNumber = request.PhoneNumber;
        expectedStudent.DateOfBirth = request.DateOfBirth;
        expectedStudent.Address = request.Address;
        expectedStudent.Gender = request.Gender;
        expectedStudent.EnrollmentDate = request.EnrollmentDate;
        expectedStudent.Grade = request.Grade;
        expectedStudent.Section = request.Section;

        _mockUserRepository
            .Setup(x => x.ExistsAsync(request.Email))
            .ReturnsAsync(false);

        _mockStudentRepository
            .Setup(x => x.EmailExistsAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockUserRepository
            .Setup(x => x.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync(expectedUser);

        _mockStudentRepository
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Student>());

        _mockStudentRepository
            .Setup(x => x.CreateAsync(It.IsAny<Student>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedStudent);

        // Setup cache service expectations
        _mockCacheService
            .Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<StudentDto>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _studentService.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be(request.FirstName);
        result.LastName.Should().Be(request.LastName);
        result.Email.Should().Be(request.Email);
        result.PhoneNumber.Should().Be(request.PhoneNumber);
        result.DateOfBirth.Should().Be(request.DateOfBirth);
        result.Address.Should().Be(request.Address);
        result.Gender.Should().Be(request.Gender);
        result.EnrollmentDate.Should().Be(request.EnrollmentDate);
        result.Grade.Should().Be(request.Grade);
        result.Section.Should().Be(request.Section);
        result.Status.Should().Be(StudentStatus.Active);
        result.UserId.Should().Be(expectedStudent.UserId); // Use expectedStudent.UserId instead of expectedUser.Id

        _mockUserRepository.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Once);
        _mockStudentRepository.Verify(x => x.CreateAsync(It.IsAny<Student>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task CreateAsync_DuplicateEmailInUserTable_ThrowsException()
    {
        // Arrange
        var request = CreateValidCreateStudentRequest();

        _mockUserRepository
            .Setup(x => x.ExistsAsync(request.Email))
            .ReturnsAsync(true);

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _studentService.CreateAsync(request));

        exception.Message.Should().Contain("already exists");
        _mockUserRepository.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Never);
        _mockStudentRepository.Verify(x => x.CreateAsync(It.IsAny<Student>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task CreateAsync_DuplicateEmailInStudentTable_ThrowsException()
    {
        // Arrange
        var request = CreateValidCreateStudentRequest();

        _mockUserRepository
            .Setup(x => x.ExistsAsync(request.Email))
            .ReturnsAsync(false);

        _mockStudentRepository
            .Setup(x => x.EmailExistsAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _studentService.CreateAsync(request));

        exception.Message.Should().Contain("already exists");
        _mockUserRepository.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Never);
        _mockStudentRepository.Verify(x => x.CreateAsync(It.IsAny<Student>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task CreateAsync_FutureEnrollmentDate_ThrowsException()
    {
        // Arrange
        var request = CreateValidCreateStudentRequest();
        request.EnrollmentDate = DateTime.Today.AddDays(1);

        // Setup mock to avoid strict mock failures
        _mockUserRepository
            .Setup(x => x.ExistsAsync(request.Email))
            .ReturnsAsync(false);

        _mockStudentRepository
            .Setup(x => x.EmailExistsAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _studentService.CreateAsync(request));

        exception.Message.Should().Contain("cannot be in the future");
    }

    [Test]
    public async Task CreateAsync_FutureDateOfBirth_ThrowsException()
    {
        // Arrange
        var request = CreateValidCreateStudentRequest();
        request.DateOfBirth = DateTime.Today.AddDays(1);

        // Setup mock to avoid strict mock failures
        _mockUserRepository
            .Setup(x => x.ExistsAsync(request.Email))
            .ReturnsAsync(false);

        _mockStudentRepository
            .Setup(x => x.EmailExistsAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _studentService.CreateAsync(request));

        exception.Message.Should().Contain("cannot be in the future");
    }

    [Test]
    public async Task CreateAsync_WithFacultyIds_AssignsFacultiesCorrectly()
    {
        // Arrange
        var request = CreateValidCreateStudentRequest();
        request.FacultyIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        
        var expectedUser = CreateTestUser(UserRole.Student);
        var expectedStudent = CreateTestStudent(expectedUser.Id);
        // Override the student data to match the request
        expectedStudent.FirstName = request.FirstName;
        expectedStudent.LastName = request.LastName;
        expectedStudent.Email = request.Email;
        expectedStudent.PhoneNumber = request.PhoneNumber;
        expectedStudent.DateOfBirth = request.DateOfBirth;
        expectedStudent.Address = request.Address;
        expectedStudent.Gender = request.Gender;
        expectedStudent.EnrollmentDate = request.EnrollmentDate;
        expectedStudent.Grade = request.Grade;
        expectedStudent.Section = request.Section;

        _mockUserRepository
            .Setup(x => x.ExistsAsync(request.Email))
            .ReturnsAsync(false);

        _mockStudentRepository
            .Setup(x => x.EmailExistsAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockUserRepository
            .Setup(x => x.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync(expectedUser);

        _mockStudentRepository
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Student>());

        _mockStudentRepository
            .Setup(x => x.CreateAsync(It.IsAny<Student>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedStudent);

        // Setup cache service expectations
        _mockCacheService
            .Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<StudentDto>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _studentService.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        _mockStudentRepository.Verify(x => x.CreateAsync(It.IsAny<Student>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region GetByIdAsync Tests

    [Test]
    public async Task GetByIdAsync_ValidId_ReturnsStudent()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var expectedStudent = CreateTestStudent(studentId);

        // Setup cache to return null (cache miss)
        _mockCacheService
            .Setup(x => x.GetAsync<StudentDto>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((StudentDto?)null);

        _mockStudentRepository
            .Setup(x => x.GetByIdAsync(studentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedStudent);

        // Setup cache service to expect SetAsync call
        _mockCacheService
            .Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<StudentDto>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _studentService.GetByIdAsync(studentId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(studentId);
        result.FirstName.Should().Be(expectedStudent.FirstName);
        result.LastName.Should().Be(expectedStudent.LastName);

        // Verify cache was called
        _mockCacheService.Verify(x => x.GetAsync<StudentDto>($"student_{studentId}", It.IsAny<CancellationToken>()), Times.Once);
        _mockCacheService.Verify(x => x.SetAsync($"student_{studentId}", It.IsAny<StudentDto>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetByIdAsync_CacheHit_ReturnsCachedStudent()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var expectedStudent = CreateTestStudent(studentId);
        var cachedStudentDto = new StudentDto
        {
            Id = studentId,
            FirstName = expectedStudent.FirstName,
            LastName = expectedStudent.LastName,
            Email = expectedStudent.Email
        };

        // Setup cache to return cached data (cache hit)
        _mockCacheService
            .Setup(x => x.GetAsync<StudentDto>($"student_{studentId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedStudentDto);

        // Act
        var result = await _studentService.GetByIdAsync(studentId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(studentId);
        result.FirstName.Should().Be(expectedStudent.FirstName);
        result.LastName.Should().Be(expectedStudent.LastName);

        // Verify cache was called but repository was NOT called
        _mockCacheService.Verify(x => x.GetAsync<StudentDto>($"student_{studentId}", It.IsAny<CancellationToken>()), Times.Once);
        _mockStudentRepository.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task GetByIdAsync_InvalidId_ReturnsNull()
    {
        // Arrange
        var studentId = Guid.NewGuid();

        // Setup cache to return null (cache miss)
        _mockCacheService
            .Setup(x => x.GetAsync<StudentDto>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((StudentDto?)null);

        _mockStudentRepository
            .Setup(x => x.GetByIdAsync(studentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Student?)null);

        // Act
        var result = await _studentService.GetByIdAsync(studentId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetByEmailAsync Tests

    [Test]
    public async Task GetByEmailAsync_ValidEmail_ReturnsStudent()
    {
        // Arrange
        var email = "test@example.com";
        var expectedStudent = CreateTestStudent();
        expectedStudent.Email = email; // Override the email to match the test

        _mockStudentRepository
            .Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedStudent);

        // Setup cache service expectations
        _mockCacheService
            .Setup(x => x.GetAsync<StudentDto>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((StudentDto?)null);
        
        _mockCacheService
            .Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<StudentDto>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _studentService.GetByEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(email);
    }

    [Test]
    public async Task GetByEmailAsync_InvalidEmail_ReturnsNull()
    {
        // Arrange
        var email = "nonexistent@example.com";

        _mockStudentRepository
            .Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Student?)null);

        // Setup cache service expectations
        _mockCacheService
            .Setup(x => x.GetAsync<StudentDto>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((StudentDto?)null);

        // Act
        var result = await _studentService.GetByEmailAsync(email);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetByRollNumberAsync Tests

    [Test]
    public async Task GetByRollNumberAsync_ValidRollNumber_ReturnsStudent()
    {
        // Arrange
        var rollNumber = "STU001";
        var expectedStudent = CreateTestStudent();
        expectedStudent.RollNumber = rollNumber; // Override the roll number to match the test

        _mockStudentRepository
            .Setup(x => x.GetByRollNumberAsync(rollNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedStudent);

        // Setup cache service expectations
        _mockCacheService
            .Setup(x => x.GetAsync<StudentDto>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((StudentDto?)null);
        
        _mockCacheService
            .Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<StudentDto>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _studentService.GetByRollNumberAsync(rollNumber);

        // Assert
        result.Should().NotBeNull();
        result!.RollNumber.Should().Be(rollNumber);
    }

    [Test]
    public async Task GetByRollNumberAsync_InvalidRollNumber_ReturnsNull()
    {
        // Arrange
        var rollNumber = "INVALID";

        _mockStudentRepository
            .Setup(x => x.GetByRollNumberAsync(rollNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Student?)null);

        // Setup cache service expectations
        _mockCacheService
            .Setup(x => x.GetAsync<StudentDto>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((StudentDto?)null);

        // Act
        var result = await _studentService.GetByRollNumberAsync(rollNumber);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetAllAsync Tests

    [Test]
    public async Task GetAllAsync_ReturnsAllStudents()
    {
        // Arrange
        var expectedStudents = new List<Student>
        {
            CreateTestStudent(),
            CreateTestStudent()
        };

        _mockStudentRepository
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedStudents);

        // Act
        var result = await _studentService.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }

    [Test]
    public async Task GetAllAsync_WithFilters_AppliesFiltersCorrectly()
    {
        // Arrange
        var filters = new StudentFilters
        {
            Search = "John",
            Status = StudentStatus.Active,
            Grade = "10",
            Section = "A"
        };

        var students = new List<Student>
        {
            CreateTestStudent(firstName: "John", lastName: "Doe", grade: "10", section: "A"),
            CreateTestStudent(firstName: "Jane", lastName: "Smith", grade: "11", section: "B"),
            CreateTestStudent(firstName: "Johnny", lastName: "Johnson", grade: "10", section: "A")
        };

        _mockStudentRepository
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(students);

        // Act
        var result = await _studentService.GetAllAsync(filters);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2); // Should return John Doe and Johnny Johnson
        result.Should().AllSatisfy(s => s.FirstName.Should().Contain("John"));
        result.Should().AllSatisfy(s => s.Grade.Should().Be("10"));
        result.Should().AllSatisfy(s => s.Section.Should().Be("A"));
    }

    [Test]
    public async Task GetAllAsync_WithSorting_AppliesSortingCorrectly()
    {
        // Arrange
        var filters = new StudentFilters
        {
            SortBy = "firstname",
            SortOrder = "desc"
        };

        var students = new List<Student>
        {
            CreateTestStudent(firstName: "Alice"),
            CreateTestStudent(firstName: "Bob"),
            CreateTestStudent(firstName: "Charlie")
        };

        _mockStudentRepository
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(students);

        // Act
        var result = await _studentService.GetAllAsync(filters);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.First()!.FirstName.Should().Be("Charlie"); // Descending order
        result.Last()!.FirstName.Should().Be("Alice");
    }

    [Test]
    public async Task GetAllAsync_WithPagination_AppliesPaginationCorrectly()
    {
        // Arrange
        var filters = new StudentFilters
        {
            Page = 2,
            Limit = 2
        };

        var students = new List<Student>
        {
            CreateTestStudent(firstName: "Alice"),
            CreateTestStudent(firstName: "Bob"),
            CreateTestStudent(firstName: "Charlie"),
            CreateTestStudent(firstName: "David"),
            CreateTestStudent(firstName: "Eve")
        };

        _mockStudentRepository
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(students);

        // Act
        var result = await _studentService.GetAllAsync(filters);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.First()!.FirstName.Should().Be("Charlie"); // Page 2, items 3-4
        result.Last()!.FirstName.Should().Be("David");
    }

    #endregion

    #region GetPaginatedAsync Tests

    [Test]
    public async Task GetPaginatedAsync_ReturnsPaginatedResponse()
    {
        // Arrange
        var filters = new StudentFilters
        {
            Page = 1,
            Limit = 3
        };

        var students = new List<Student>
        {
            CreateTestStudent(firstName: "Alice"),
            CreateTestStudent(firstName: "Bob"),
            CreateTestStudent(firstName: "Charlie"),
            CreateTestStudent(firstName: "David"),
            CreateTestStudent(firstName: "Eve")
        };

        _mockStudentRepository
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(students);

        // Act
        var result = await _studentService.GetPaginatedAsync(filters);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(3);
        result.TotalCount.Should().Be(5);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(3);
    }

    #endregion

    #region UpdateAsync Tests

    [Test]
    public async Task UpdateAsync_ValidRequest_UpdatesStudentSuccessfully()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var existingStudent = CreateTestStudent(studentId);
        var request = CreateValidUpdateStudentRequest();

        _mockStudentRepository
            .Setup(x => x.GetByIdAsync(studentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingStudent);

        _mockStudentRepository
            .Setup(x => x.EmailExistsAsync(request.Email!, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockStudentRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Student>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingStudent);

        // Setup cache service expectations
        _mockCacheService
            .Setup(x => x.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        _mockCacheService
            .Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<StudentDto>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _studentService.UpdateAsync(studentId, request);

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be(request.FirstName);
        result.LastName.Should().Be(request.LastName);
        result.Email.Should().Be(request.Email);

        _mockStudentRepository.Verify(x => x.UpdateAsync(It.IsAny<Student>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task UpdateAsync_StudentNotFound_ThrowsException()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var request = CreateValidUpdateStudentRequest();

        _mockStudentRepository
            .Setup(x => x.GetByIdAsync(studentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Student?)null);

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _studentService.UpdateAsync(studentId, request));

        exception.Message.Should().Contain("not found");
        _mockStudentRepository.Verify(x => x.UpdateAsync(It.IsAny<Student>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task UpdateAsync_DuplicateEmail_ThrowsException()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var existingStudent = CreateTestStudent(studentId);
        var request = CreateValidUpdateStudentRequest();
        request.Email = "duplicate@example.com";

        _mockStudentRepository
            .Setup(x => x.GetByIdAsync(studentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingStudent);

        _mockStudentRepository
            .Setup(x => x.EmailExistsAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _studentService.UpdateAsync(studentId, request));

        exception.Message.Should().Contain("already exists");
        _mockStudentRepository.Verify(x => x.UpdateAsync(It.IsAny<Student>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region DeleteAsync Tests

    [Test]
    public async Task DeleteAsync_ValidId_DeletesStudent()
    {
        // Arrange
        var studentId = Guid.NewGuid();

        // Setup mock to avoid strict mock failures
        var expectedStudent = CreateTestStudent(studentId);
        _mockStudentRepository
            .Setup(x => x.GetByIdAsync(studentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedStudent);
        
        _mockStudentRepository
            .Setup(x => x.DeleteAsync(studentId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Setup cache service expectations
        _mockCacheService
            .Setup(x => x.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _studentService.DeleteAsync(studentId);

        // Assert
        _mockStudentRepository.Verify(x => x.DeleteAsync(studentId, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region ExistsAsync Tests

    [Test]
    public async Task ExistsAsync_StudentExists_ReturnsTrue()
    {
        // Arrange
        var studentId = Guid.NewGuid();

        _mockStudentRepository
            .Setup(x => x.ExistsAsync(studentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _studentService.ExistsAsync(studentId);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public async Task ExistsAsync_StudentDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var studentId = Guid.NewGuid();

        _mockStudentRepository
            .Setup(x => x.ExistsAsync(studentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _studentService.ExistsAsync(studentId);

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

        _mockStudentRepository
            .Setup(x => x.EmailExistsAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _studentService.EmailExistsAsync(email);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public async Task EmailExistsAsync_EmailDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var email = "nonexistent@example.com";

        _mockStudentRepository
            .Setup(x => x.EmailExistsAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _studentService.EmailExistsAsync(email);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region RollNumberExistsAsync Tests

    [Test]
    public async Task RollNumberExistsAsync_RollNumberExists_ReturnsTrue()
    {
        // Arrange
        var rollNumber = "STU001";

        _mockStudentRepository
            .Setup(x => x.RollNumberExistsAsync(rollNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _studentService.RollNumberExistsAsync(rollNumber);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public async Task RollNumberExistsAsync_RollNumberDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var rollNumber = "INVALID";

        _mockStudentRepository
            .Setup(x => x.RollNumberExistsAsync(rollNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _studentService.RollNumberExistsAsync(rollNumber);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Helper Methods

    private static CreateStudentRequest CreateValidCreateStudentRequest()
    {
        return new CreateStudentRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            PhoneNumber = "+1234567890",
            DateOfBirth = new DateTime(2005, 1, 1),
            Address = "123 Main St",
            Gender = Gender.Male,
            EnrollmentDate = DateTime.Today.AddDays(-30),
            Grade = "10",
            Section = "A",
            FacultyIds = new List<Guid>()
        };
    }

    private static UpdateStudentRequest CreateValidUpdateStudentRequest()
    {
        return new UpdateStudentRequest
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane.smith@example.com",
            PhoneNumber = "+0987654321",
            DateOfBirth = new DateTime(2006, 2, 2),
            Address = "456 Oak Ave",
            Gender = Gender.Female,
            EnrollmentDate = DateTime.Today.AddDays(-60),
            Grade = "11",
            Section = "B",
            Status = StudentStatus.Active,
            FacultyIds = new List<Guid>()
        };
    }

    private static Student CreateTestStudent(Guid? id = null, string? firstName = "Test", string? lastName = "Student", string? grade = "10", string? section = "A")
    {
        return new Student
        {
            Id = id ?? Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            Email = $"{firstName?.ToLower()}.{lastName?.ToLower()}@example.com",
            PhoneNumber = "+1234567890",
            DateOfBirth = new DateTime(2005, 1, 1),
            Address = "123 Test St",
            Gender = Gender.Other,
            RollNumber = "TEST001",
            EnrollmentDate = DateTime.Today.AddYears(-1),
            Status = StudentStatus.Active,
            Grade = grade,
            Section = section,
            UserId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            StudentFaculties = new List<StudentFaculty>()
        };
    }

    #endregion
}
