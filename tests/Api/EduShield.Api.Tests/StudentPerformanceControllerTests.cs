using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using EduShield.Core.Dtos;
using EduShield.Core.Enums;
using EduShield.Core.Services;
using EduShield.Api.Controllers;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;

namespace EduShield.Api.Tests;

/// <summary>
/// Tests for StudentPerformanceController
/// </summary>
public class StudentPerformanceControllerTests
{
    private readonly Mock<IStudentPerformanceService> _mockPerformanceService;
    private readonly Mock<ILogger<StudentPerformanceController>> _mockLogger;
    private readonly StudentPerformanceController _controller;

    public StudentPerformanceControllerTests()
    {
        _mockPerformanceService = new Mock<IStudentPerformanceService>();
        _mockLogger = new Mock<ILogger<StudentPerformanceController>>();
        _controller = new StudentPerformanceController(_mockPerformanceService.Object, _mockLogger.Object);
    }

    [Fact]
    public void Controller_ShouldHaveCorrectRoute()
    {
        // Arrange & Act
        var routeAttribute = _controller.GetType().GetCustomAttributes(typeof(RouteAttribute), true)
            .FirstOrDefault() as RouteAttribute;

        // Assert
        Assert.NotNull(routeAttribute);
        Assert.Equal("api/v1/student-performance", routeAttribute.Template);
    }

    [Fact]
    public void Controller_ShouldRequireAuthorization()
    {
        // Arrange & Act
        var authorizeAttribute = _controller.GetType().GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .FirstOrDefault() as AuthorizeAttribute;

        // Assert
        Assert.NotNull(authorizeAttribute);
    }

    [Fact]
    public async Task GetAllPerformance_WhenAdmin_ShouldReturnAllRecords()
    {
        // Arrange
        var adminUser = CreateUserWithRole("Admin");
        _controller.ControllerContext = CreateControllerContext(adminUser);

        var expectedPerformances = new List<StudentPerformanceDto>
        {
            CreateSamplePerformanceDto(),
            CreateSamplePerformanceDto()
        };

        _mockPerformanceService
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedPerformances);

        // Act
        var result = await _controller.GetAllPerformance(cancellationToken: CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var performances = Assert.IsAssignableFrom<IEnumerable<StudentPerformanceDto>>(okResult.Value);
        Assert.Equal(expectedPerformances.Count, performances.Count());
    }

    [Fact]
    public async Task GetAllPerformance_WhenStudent_ShouldReturnOnlyOwnRecords()
    {
        // Arrange
        var studentId = Guid.NewGuid();
        var studentUser = CreateUserWithRole("Student", studentId);
        _controller.ControllerContext = CreateControllerContext(studentUser);

        var expectedPerformances = new List<StudentPerformanceDto>
        {
            CreateSamplePerformanceDto(studentId)
        };

        _mockPerformanceService
            .Setup(x => x.GetByStudentIdAsync(studentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedPerformances);

        // Act
        var result = await _controller.GetAllPerformance(cancellationToken: CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var performances = Assert.IsAssignableFrom<IEnumerable<StudentPerformanceDto>>(okResult.Value);
        Assert.Single(performances);
        Assert.All(performances, p => Assert.Equal(studentId, p.StudentId));
    }

    [Fact]
    public async Task CreatePerformance_WhenAdmin_ShouldSucceed()
    {
        // Arrange
        var adminUser = CreateUserWithRole("Admin");
        _controller.ControllerContext = CreateControllerContext(adminUser);

        var request = new CreateStudentPerformanceRequest
        {
            StudentId = Guid.NewGuid(),
            Subject = "Mathematics",
            ExamType = ExamType.MidTerm,
            ExamDate = DateTime.Today.AddDays(-1),
            Score = 85.5m,
            MaxScore = 100m
        };

        var expectedPerformance = CreateSamplePerformanceDto(request.StudentId);
        _mockPerformanceService
            .Setup(x => x.CreateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedPerformance);

        // Act
        var result = await _controller.CreatePerformance(request, CancellationToken.None);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var performance = Assert.IsType<StudentPerformanceDto>(createdResult.Value);
        Assert.Equal(expectedPerformance.Id, performance.Id);
    }

    [Fact]
    public async Task CreatePerformance_WhenStudent_ShouldBeForbidden()
    {
        // Arrange
        var studentUser = CreateUserWithRole("Student");
        _controller.ControllerContext = CreateControllerContext(studentUser);

        var request = new CreateStudentPerformanceRequest
        {
            StudentId = Guid.NewGuid(),
            Subject = "Mathematics",
            ExamType = ExamType.MidTerm,
            ExamDate = DateTime.Today.AddDays(-1),
            Score = 85.5m,
            MaxScore = 100m
        };

        // Act
        var result = await _controller.CreatePerformance(request, CancellationToken.None);

        // Assert
        Assert.IsType<ForbidResult>(result.Result);
    }

    [Fact]
    public async Task UpdatePerformance_WhenFaculty_ShouldSucceed()
    {
        // Arrange
        var facultyUser = CreateUserWithRole("Faculty");
        _controller.ControllerContext = CreateControllerContext(facultyUser);

        var performanceId = Guid.NewGuid();
        var request = new UpdateStudentPerformanceRequest
        {
            Score = 90.0m,
            Comments = "Excellent work!"
        };

        var expectedPerformance = CreateSamplePerformanceDto();
        _mockPerformanceService
            .Setup(x => x.UpdateAsync(performanceId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedPerformance);

        // Act
        var result = await _controller.UpdatePerformance(performanceId, request, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var performance = Assert.IsType<StudentPerformanceDto>(okResult.Value);
        Assert.Equal(expectedPerformance.Id, performance.Id);
    }

    [Fact]
    public async Task DeletePerformance_WhenFaculty_ShouldBeForbidden()
    {
        // Arrange
        var facultyUser = CreateUserWithRole("Faculty");
        _controller.ControllerContext = CreateControllerContext(facultyUser);

        var performanceId = Guid.NewGuid();

        // Act
        var result = await _controller.DeletePerformance(performanceId, CancellationToken.None);

        // Assert
        Assert.IsType<ForbidResult>(result.Result);
    }

    [Fact]
    public async Task DeletePerformance_WhenAdmin_ShouldSucceed()
    {
        // Arrange
        var adminUser = CreateUserWithRole("Admin");
        _controller.ControllerContext = CreateControllerContext(adminUser);

        var performanceId = Guid.NewGuid();

        _mockPerformanceService
            .Setup(x => x.ExistsAsync(performanceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockPerformanceService
            .Setup(x => x.DeleteAsync(performanceId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeletePerformance(performanceId, CancellationToken.None);

        // Assert
        Assert.IsType<NoContentResult>(result.Result);
    }

    // Helper methods
    private static ClaimsPrincipal CreateUserWithRole(string role, Guid? userId = null)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Role, role),
            new(ClaimTypes.NameIdentifier, (userId ?? Guid.NewGuid()).ToString()),
            new(ClaimTypes.Email, "test@example.com")
        };

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
    }

    private static ControllerContext CreateControllerContext(ClaimsPrincipal user)
    {
        var httpContext = new DefaultHttpContext
        {
            User = user
        };

        return new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    private static StudentPerformanceDto CreateSamplePerformanceDto(Guid? studentId = null)
    {
        return new StudentPerformanceDto
        {
            Id = Guid.NewGuid(),
            StudentId = studentId ?? Guid.NewGuid(),
            StudentFirstName = "John",
            StudentLastName = "Doe",
            Subject = "Mathematics",
            ExamType = ExamType.MidTerm,
            ExamDate = DateTime.Today.AddDays(-1),
            Score = 85.5m,
            MaxScore = 100m,
            ExamTitle = "Mid-Term Mathematics Exam",
            Comments = "Good understanding of algebra concepts",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}
