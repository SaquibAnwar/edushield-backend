using Moq;
using NUnit.Framework;
using FluentAssertions;
using EduShield.Core.Services;
using EduShield.Core.Interfaces;
using EduShield.Core.Entities;
using EduShield.Core.Dtos;
using EduShield.Core.Enums;
using EduShield.Core.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EduShield.Api.Tests.Unit;

[TestFixture]
public class StudentPerformanceServiceTests
{
    private Mock<IStudentPerformanceRepository> _mockPerformanceRepository = null!;
    private Mock<IStudentRepository> _mockStudentRepository = null!;
    private Mock<IUserRepository> _mockUserRepository = null!;
    private Mock<IEncryptionService> _mockEncryptionService = null!;
    private StudentPerformanceService _service = null!;

    [SetUp]
    public void Setup()
    {
        _mockPerformanceRepository = new Mock<IStudentPerformanceRepository>();
        _mockStudentRepository = new Mock<IStudentRepository>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockEncryptionService = new Mock<IEncryptionService>();
        
        _service = new StudentPerformanceService(
            _mockPerformanceRepository.Object,
            _mockStudentRepository.Object,
            _mockUserRepository.Object,
            _mockEncryptionService.Object);
    }

    [Test]
    public async Task CreateAsync_ValidRequest_CreatesPerformanceSuccessfully()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var request = new CreateStudentPerformanceRequest
        {
            StudentId = studentId,
            Subject = "Mathematics",
            ExamType = ExamType.MidTerm,
            ExamDate = DateTime.Today,
            Score = 85,
            MaxScore = 100,
            ExamTitle = "Midterm Exam",
            Comments = "Good performance"
        };

        var student = CreateTestStudent(studentId);
        var performance = CreateTestPerformance(studentId, request.Subject, request.ExamType, request.ExamDate);
        var encryptedScore = "encrypted_85";

        _mockStudentRepository
            .Setup(x => x.GetByIdAsync(studentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(student);

        _mockEncryptionService
            .Setup(x => x.EncryptDecimal(request.Score))
            .Returns(encryptedScore);

        _mockPerformanceRepository
            .Setup(x => x.CreateAsync(It.IsAny<StudentPerformance>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(performance);

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.StudentId.Should().Be(studentId);
        result.Subject.Should().Be("Mathematics");
        result.ExamType.Should().Be(ExamType.MidTerm);

        _mockPerformanceRepository.Verify(x => x.CreateAsync(It.IsAny<StudentPerformance>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockEncryptionService.Verify(x => x.EncryptDecimal(request.Score), Times.Once);
    }

    [Test]
    public async Task CreateAsync_StudentNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var request = new CreateStudentPerformanceRequest
        {
            StudentId = studentId,
            Subject = "Mathematics",
            ExamType = ExamType.MidTerm,
            ExamDate = DateTime.Today,
            Score = 85
        };

        _mockStudentRepository
            .Setup(x => x.GetByIdAsync(studentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Student?)null);

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service.CreateAsync(request));
        
        exception.Message.Should().Contain("Student with ID");
        exception.Message.Should().Contain("not found");
    }

    [Test]
    public async Task CreateAsync_FutureExamDate_ThrowsInvalidOperationException()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var request = new CreateStudentPerformanceRequest
        {
            StudentId = studentId,
            Subject = "Mathematics",
            ExamType = ExamType.MidTerm,
            ExamDate = DateTime.Today.AddDays(1),
            Score = 85
        };

        var student = CreateTestStudent(studentId);

        _mockStudentRepository
            .Setup(x => x.GetByIdAsync(studentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(student);

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service.CreateAsync(request));
        
        exception.Message.Should().Contain("Exam date cannot be in the future");
    }

    [Test]
    public async Task CreateAsync_NegativeScore_ThrowsInvalidOperationException()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var request = new CreateStudentPerformanceRequest
        {
            StudentId = studentId,
            Subject = "Mathematics",
            ExamType = ExamType.MidTerm,
            ExamDate = DateTime.Today,
            Score = -5
        };

        var student = CreateTestStudent(studentId);

        _mockStudentRepository
            .Setup(x => x.GetByIdAsync(studentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(student);

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service.CreateAsync(request));
        
        exception.Message.Should().Contain("Score cannot be negative");
    }

    [Test]
    public async Task CreateAsync_ScoreExceedsMaxScore_ThrowsInvalidOperationException()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var request = new CreateStudentPerformanceRequest
        {
            StudentId = studentId,
            Subject = "Mathematics",
            ExamType = ExamType.MidTerm,
            ExamDate = DateTime.Today,
            Score = 105,
            MaxScore = 100
        };

        var student = CreateTestStudent(studentId);

        _mockStudentRepository
            .Setup(x => x.GetByIdAsync(studentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(student);

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service.CreateAsync(request));
        
        exception.Message.Should().Contain("Score cannot exceed maximum score");
    }

    [Test]
    public async Task GetByIdAsync_ValidId_ReturnsPerformance()
    {
        // Arrange
        var performanceId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var performance = CreateTestPerformance(studentId, "Mathematics", ExamType.MidTerm, DateTime.Today);
        performance.Id = performanceId; // Set the ID explicitly
        var decryptedScore = 85.0m;

        _mockPerformanceRepository
            .Setup(x => x.GetByIdAsync(performanceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(performance);

        _mockEncryptionService
            .Setup(x => x.DecryptDecimal(performance.EncryptedScore))
            .Returns(decryptedScore);

        // Act
        var result = await _service.GetByIdAsync(performanceId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(performanceId);
        result.Score.Should().Be(85.0m);
        result.Subject.Should().Be("Mathematics");

        _mockEncryptionService.Verify(x => x.DecryptDecimal(performance.EncryptedScore), Times.Once);
    }

    [Test]
    public async Task GetByIdAsync_InvalidId_ReturnsNull()
    {
        // Arrange
        var performanceId = Guid.NewGuid();

        _mockPerformanceRepository
            .Setup(x => x.GetByIdAsync(performanceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((StudentPerformance?)null);

        // Act
        var result = await _service.GetByIdAsync(performanceId);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task GetByStudentIdAsync_ValidStudentId_ReturnsPerformances()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var performances = new List<StudentPerformance>
        {
            CreateTestPerformance(studentId, "Mathematics", ExamType.MidTerm, DateTime.Today),
            CreateTestPerformance(studentId, "Physics", ExamType.Final, DateTime.Today.AddDays(-7))
        };

        var decryptedScore = 85.0m;

        _mockPerformanceRepository
            .Setup(x => x.GetByStudentIdAsync(studentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(performances);

        _mockEncryptionService
            .Setup(x => x.DecryptDecimal(It.IsAny<string>()))
            .Returns(decryptedScore);

        // Act
        var result = await _service.GetByStudentIdAsync(studentId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.First().StudentId.Should().Be(studentId);
    }

    [Test]
    public async Task GetBySubjectAsync_ValidSubject_ReturnsPerformances()
    {
        // Arrange
        var subject = "Mathematics";
        var performances = new List<StudentPerformance>
        {
            CreateTestPerformance(Guid.NewGuid(), subject, ExamType.MidTerm, DateTime.Today),
            CreateTestPerformance(Guid.NewGuid(), subject, ExamType.Final, DateTime.Today.AddDays(-7))
        };

        var decryptedScore = 85.0m;

        _mockPerformanceRepository
            .Setup(x => x.GetBySubjectAsync(subject, It.IsAny<CancellationToken>()))
            .ReturnsAsync(performances);

        _mockEncryptionService
            .Setup(x => x.DecryptDecimal(It.IsAny<string>()))
            .Returns(decryptedScore);

        // Act
        var result = await _service.GetBySubjectAsync(subject);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.All(p => p.Subject == subject).Should().BeTrue();
    }

    [Test]
    public async Task GetByExamTypeAsync_ValidExamType_ReturnsPerformances()
    {
        // Arrange
        var examType = ExamType.MidTerm;
        var performances = new List<StudentPerformance>
        {
            CreateTestPerformance(Guid.NewGuid(), "Mathematics", examType, DateTime.Today),
            CreateTestPerformance(Guid.NewGuid(), "Physics", examType, DateTime.Today.AddDays(-7))
        };

        var decryptedScore = 85.0m;

        _mockPerformanceRepository
            .Setup(x => x.GetByExamTypeAsync(examType, It.IsAny<CancellationToken>()))
            .ReturnsAsync(performances);

        _mockEncryptionService
            .Setup(x => x.DecryptDecimal(It.IsAny<string>()))
            .Returns(decryptedScore);

        // Act
        var result = await _service.GetByExamTypeAsync(examType);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.All(p => p.ExamType == examType).Should().BeTrue();
    }

    [Test]
    public async Task GetByDateRangeAsync_ValidDateRange_ReturnsPerformances()
    {
        // Arrange
        var startDate = DateTime.Today.AddDays(-30);
        var endDate = DateTime.Today;
        var performances = new List<StudentPerformance>
        {
            CreateTestPerformance(Guid.NewGuid(), "Mathematics", ExamType.MidTerm, DateTime.Today.AddDays(-15)),
            CreateTestPerformance(Guid.NewGuid(), "Physics", ExamType.Final, DateTime.Today.AddDays(-7))
        };

        var decryptedScore = 85.0m;

        _mockPerformanceRepository
            .Setup(x => x.GetByDateRangeAsync(startDate, endDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(performances);

        _mockEncryptionService
            .Setup(x => x.DecryptDecimal(It.IsAny<string>()))
            .Returns(decryptedScore);

        // Act
        var result = await _service.GetByDateRangeAsync(startDate, endDate);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }

    [Test]
    public async Task UpdateAsync_ValidRequest_UpdatesPerformanceSuccessfully()
    {
        // Arrange
        var performanceId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var existingPerformance = CreateTestPerformance(studentId, "Mathematics", ExamType.MidTerm, DateTime.Today);
        var request = new UpdateStudentPerformanceRequest
        {
            Subject = "Advanced Mathematics",
            Score = 90,
            MaxScore = 100,
            Comments = "Updated comments"
        };

        var updatedPerformance = CreateTestPerformance(studentId, "Advanced Mathematics", ExamType.MidTerm, DateTime.Today);
        updatedPerformance.Comments = "Updated comments"; // Set the updated comments
        var encryptedScore = "encrypted_90";

        _mockPerformanceRepository
            .Setup(x => x.GetByIdAsync(performanceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPerformance);

        _mockEncryptionService
            .Setup(x => x.EncryptDecimal(request.Score!.Value))
            .Returns(encryptedScore);

        _mockPerformanceRepository
            .Setup(x => x.UpdateAsync(existingPerformance, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedPerformance);

        // Act
        var result = await _service.UpdateAsync(performanceId, request);

        // Assert
        result.Should().NotBeNull();
        result.Subject.Should().Be("Advanced Mathematics");
        result.Comments.Should().Be("Updated comments");

        _mockPerformanceRepository.Verify(x => x.UpdateAsync(existingPerformance, It.IsAny<CancellationToken>()), Times.Once);
        _mockEncryptionService.Verify(x => x.EncryptDecimal(request.Score!.Value), Times.Once);
    }

    [Test]
    public async Task UpdateAsync_PerformanceNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var performanceId = Guid.NewGuid();
        var request = new UpdateStudentPerformanceRequest
        {
            Subject = "Advanced Mathematics"
        };

        _mockPerformanceRepository
            .Setup(x => x.GetByIdAsync(performanceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((StudentPerformance?)null);

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service.UpdateAsync(performanceId, request));
        
        exception.Message.Should().Contain("Performance record with ID");
        exception.Message.Should().Contain("not found");
    }

    [Test]
    public async Task UpdateAsync_FutureExamDate_ThrowsInvalidOperationException()
    {
        // Arrange
        var performanceId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var existingPerformance = CreateTestPerformance(studentId, "Mathematics", ExamType.MidTerm, DateTime.Today);
        var request = new UpdateStudentPerformanceRequest
        {
            ExamDate = DateTime.Today.AddDays(1)
        };

        _mockPerformanceRepository
            .Setup(x => x.GetByIdAsync(performanceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPerformance);

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service.UpdateAsync(performanceId, request));
        
        exception.Message.Should().Contain("Exam date cannot be in the future");
    }

    [Test]
    public async Task UpdateAsync_NegativeScore_ThrowsInvalidOperationException()
    {
        // Arrange
        var performanceId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var existingPerformance = CreateTestPerformance(studentId, "Mathematics", ExamType.MidTerm, DateTime.Today);
        var request = new UpdateStudentPerformanceRequest
        {
            Score = -5
        };

        _mockPerformanceRepository
            .Setup(x => x.GetByIdAsync(performanceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPerformance);

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service.UpdateAsync(performanceId, request));
        
        exception.Message.Should().Contain("Score cannot be negative");
    }

    [Test]
    public async Task UpdateAsync_ScoreExceedsMaxScore_ThrowsInvalidOperationException()
    {
        // Arrange
        var performanceId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var existingPerformance = CreateTestPerformance(studentId, "Mathematics", ExamType.MidTerm, DateTime.Today);
        var request = new UpdateStudentPerformanceRequest
        {
            Score = 105,
            MaxScore = 100
        };

        _mockPerformanceRepository
            .Setup(x => x.GetByIdAsync(performanceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPerformance);

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service.UpdateAsync(performanceId, request));
        
        exception.Message.Should().Contain("Score cannot exceed maximum score");
    }

    [Test]
    public async Task DeleteAsync_ValidId_DeletesPerformanceSuccessfully()
    {
        // Arrange
        var performanceId = Guid.NewGuid();

        _mockPerformanceRepository
            .Setup(x => x.ExistsAsync(performanceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockPerformanceRepository
            .Setup(x => x.DeleteAsync(performanceId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.DeleteAsync(performanceId);

        // Assert
        _mockPerformanceRepository.Verify(x => x.DeleteAsync(performanceId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task DeleteAsync_PerformanceNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var performanceId = Guid.NewGuid();

        _mockPerformanceRepository
            .Setup(x => x.ExistsAsync(performanceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service.DeleteAsync(performanceId));
        
        exception.Message.Should().Contain("Performance record with ID");
        exception.Message.Should().Contain("not found");
    }

    [Test]
    public async Task ExistsAsync_ValidId_ReturnsTrue()
    {
        // Arrange
        var performanceId = Guid.NewGuid();

        _mockPerformanceRepository
            .Setup(x => x.ExistsAsync(performanceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.ExistsAsync(performanceId);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public async Task ExistsAsync_InvalidId_ReturnsFalse()
    {
        // Arrange
        var performanceId = Guid.NewGuid();

        _mockPerformanceRepository
            .Setup(x => x.ExistsAsync(performanceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _service.ExistsAsync(performanceId);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public async Task GetStudentStatisticsAsync_ValidStudentId_ReturnsStatistics()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var statistics = new { totalExams = 5, averageScore = 85.5 };

        _mockStudentRepository
            .Setup(x => x.ExistsAsync(studentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockPerformanceRepository
            .Setup(x => x.GetStudentStatisticsAsync(studentId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(statistics);

        // Act
        var result = await _service.GetStudentStatisticsAsync(studentId);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(statistics);
    }

    [Test]
    public async Task GetStudentStatisticsAsync_StudentNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var studentId = Guid.NewGuid();

        _mockStudentRepository
            .Setup(x => x.ExistsAsync(studentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service.GetStudentStatisticsAsync(studentId));
        
        exception.Message.Should().Contain("Student with ID");
        exception.Message.Should().Contain("not found");
    }

    [Test]
    public async Task GetPaginatedAsync_ValidFilter_ReturnsPaginatedResults()
    {
        // Arrange
        var filter = new StudentPerformanceFilterRequest
        {
            Page = 1,
            Limit = 10,
            SortBy = "subject",
            SortOrder = "asc"
        };

        var performances = new List<StudentPerformance>
        {
            CreateTestPerformance(Guid.NewGuid(), "Mathematics", ExamType.MidTerm, DateTime.Today),
            CreateTestPerformance(Guid.NewGuid(), "Physics", ExamType.Final, DateTime.Today.AddDays(-7))
        };

        var decryptedScore = 85.0m;

        _mockPerformanceRepository
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(performances);

        _mockEncryptionService
            .Setup(x => x.DecryptDecimal(It.IsAny<string>()))
            .Returns(decryptedScore);

        // Act
        var result = await _service.GetPaginatedAsync(filter);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Test]
    public async Task GetPaginatedAsync_WithStudentIdFilter_FiltersCorrectly()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var filter = new StudentPerformanceFilterRequest
        {
            Page = 1,
            Limit = 10,
            StudentId = studentId
        };

        var performances = new List<StudentPerformance>
        {
            CreateTestPerformance(studentId, "Mathematics", ExamType.MidTerm, DateTime.Today),
            CreateTestPerformance(Guid.NewGuid(), "Physics", ExamType.Final, DateTime.Today.AddDays(-7))
        };

        var decryptedScore = 85.0m;

        _mockPerformanceRepository
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(performances);

        _mockEncryptionService
            .Setup(x => x.DecryptDecimal(It.IsAny<string>()))
            .Returns(decryptedScore);

        // Act
        var result = await _service.GetPaginatedAsync(filter);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(1);
        result.Data.First().StudentId.Should().Be(studentId);
    }

    [Test]
    public async Task GetPaginatedAsync_WithSubjectFilter_FiltersCorrectly()
    {
        // Arrange
        var subject = "Mathematics";
        var filter = new StudentPerformanceFilterRequest
        {
            Page = 1,
            Limit = 10,
            Subject = subject
        };

        var performances = new List<StudentPerformance>
        {
            CreateTestPerformance(Guid.NewGuid(), subject, ExamType.MidTerm, DateTime.Today),
            CreateTestPerformance(Guid.NewGuid(), "Physics", ExamType.Final, DateTime.Today.AddDays(-7))
        };

        var decryptedScore = 85.0m;

        _mockPerformanceRepository
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(performances);

        _mockEncryptionService
            .Setup(x => x.DecryptDecimal(It.IsAny<string>()))
            .Returns(decryptedScore);

        // Act
        var result = await _service.GetPaginatedAsync(filter);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(1);
        result.Data.First().Subject.Should().Be(subject);
    }

    [Test]
    public async Task GetPaginatedAsync_WithDateRangeFilter_FiltersCorrectly()
    {
        // Arrange
        var fromDate = DateTime.Today.AddDays(-30);
        var toDate = DateTime.Today;
        var filter = new StudentPerformanceFilterRequest
        {
            Page = 1,
            Limit = 10,
            FromDate = fromDate,
            ToDate = toDate
        };

        var performances = new List<StudentPerformance>
        {
            CreateTestPerformance(Guid.NewGuid(), "Mathematics", ExamType.MidTerm, DateTime.Today.AddDays(-15)),
            CreateTestPerformance(Guid.NewGuid(), "Physics", ExamType.Final, DateTime.Today.AddDays(-7))
        };

        var decryptedScore = 85.0m;

        _mockPerformanceRepository
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(performances);

        _mockEncryptionService
            .Setup(x => x.DecryptDecimal(It.IsAny<string>()))
            .Returns(decryptedScore);

        // Act
        var result = await _service.GetPaginatedAsync(filter);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(2);
    }

    [Test]
    public async Task GetPaginatedAsync_WithSearchFilter_FiltersCorrectly()
    {
        // Arrange
        var searchTerm = "Math";
        var filter = new StudentPerformanceFilterRequest
        {
            Page = 1,
            Limit = 10,
            Search = searchTerm
        };

        var performances = new List<StudentPerformance>
        {
            CreateTestPerformance(Guid.NewGuid(), "Mathematics", ExamType.MidTerm, DateTime.Today),
            CreateTestPerformance(Guid.NewGuid(), "Physics", ExamType.Final, DateTime.Today.AddDays(-7))
        };

        var decryptedScore = 85.0m;

        _mockPerformanceRepository
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(performances);

        _mockEncryptionService
            .Setup(x => x.DecryptDecimal(It.IsAny<string>()))
            .Returns(decryptedScore);

        // Act
        var result = await _service.GetPaginatedAsync(filter);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(1);
        result.Data.First().Subject.Should().Contain(searchTerm);
    }

    [Test]
    public async Task GetPaginatedAsync_WithSorting_SortsCorrectly()
    {
        // Arrange
        var filter = new StudentPerformanceFilterRequest
        {
            Page = 1,
            Limit = 10,
            SortBy = "subject",
            SortOrder = "desc"
        };

        var performances = new List<StudentPerformance>
        {
            CreateTestPerformance(Guid.NewGuid(), "Physics", ExamType.Final, DateTime.Today.AddDays(-7)),
            CreateTestPerformance(Guid.NewGuid(), "Mathematics", ExamType.MidTerm, DateTime.Today)
        };

        var decryptedScore = 85.0m;

        _mockPerformanceRepository
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(performances);

        _mockEncryptionService
            .Setup(x => x.DecryptDecimal(It.IsAny<string>()))
            .Returns(decryptedScore);

        // Act
        var result = await _service.GetPaginatedAsync(filter);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(2);
        result.Data.First().Subject.Should().Be("Physics"); // Should be first due to desc sorting
    }

    #region Helper Methods
    private static Student CreateTestStudent(Guid id)
    {
        return new Student
        {
            Id = id,
            FirstName = "Test",
            LastName = "Student",
            Email = "test.student@example.com",
            RollNumber = "TEST001",
            Grade = "12",
            Section = "A"
        };
    }

    private static StudentPerformance CreateTestPerformance(Guid studentId, string subject, ExamType examType, DateTime examDate)
    {
        return new StudentPerformance
        {
            Id = Guid.NewGuid(),
            StudentId = studentId,
            Subject = subject,
            ExamType = examType,
            ExamDate = examDate,
            MaxScore = 100,
            ExamTitle = $"{subject} {examType}",
            Comments = "Test performance",
            EncryptedScore = "encrypted_score",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Student = CreateTestStudent(studentId)
        };
    }
    #endregion
}
