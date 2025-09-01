using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using EduShield.Core.Dtos;
using EduShield.Core.Enums;
using EduShield.Core.Services;
using EduShield.Api.Controllers;
using EduShield.Api.Tests.Fixtures;
using EduShield.Api.Tests.Helpers;
using FluentAssertions;
using NUnit.Framework;
using Microsoft.AspNetCore.Authorization;
using System.Reflection;

namespace EduShield.Api.Tests.Unit;

/// <summary>
/// Tests for StudentPerformanceController
/// </summary>
[TestFixture]
[Category("Unit")]
public class StudentPerformanceControllerTests : BaseTestFixture
{
    private Mock<IStudentPerformanceService> _mockPerformanceService = null!;
    private Mock<Core.Interfaces.IStudentRepository> _mockStudentRepository = null!;
    private StudentPerformanceController _controller = null!;

    [SetUp]
    public override void Setup()
    {
        base.Setup();
        
        _mockPerformanceService = MockRepository.Create<IStudentPerformanceService>();
        var mockStudentRepository = MockRepository.Create<Core.Interfaces.IStudentRepository>();
        var logger = CreateMockLogger<StudentPerformanceController>();
        
        _controller = new StudentPerformanceController(_mockPerformanceService.Object, mockStudentRepository.Object, logger.Object);
        
        // Store the mock student repository for use in tests
        _mockStudentRepository = mockStudentRepository;
    }

    [Test]
    public void Controller_ShouldHaveCorrectRoute()
    {
        // Arrange & Act
        var routeAttribute = _controller.GetType().GetCustomAttributes(typeof(RouteAttribute), true)
            .FirstOrDefault() as RouteAttribute;

        // Assert
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("api/v1/student-performance");
    }

    [Test]
    public void Controller_ShouldRequireAuthorization()
    {
        // Arrange & Act
        // Check that the GetAllPerformance method has [Authorize] attribute
        var methodInfo = _controller.GetType().GetMethod("GetAllPerformance");
        var authorizeAttribute = methodInfo?.GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .FirstOrDefault() as AuthorizeAttribute;

        // Assert
        authorizeAttribute.Should().NotBeNull();
        authorizeAttribute!.Policy.Should().BeNull(); // No specific policy, just [Authorize]
    }

    [Test]
    public async Task GetAllPerformance_WhenAdmin_ShouldReturnAllRecords()
    {
        // Arrange
        TestAuthenticationHelper.SetupControllerContext(_controller, UserRole.Admin);

        var expectedPerformances = new List<StudentPerformanceDto>
        {
            CreateTestPerformanceDto(),
            CreateTestPerformanceDto()
        };

        var paginatedResponse = new Core.Dtos.PaginatedResponse<StudentPerformanceDto>(expectedPerformances, expectedPerformances.Count, 1, 10);
        _mockPerformanceService
            .Setup(x => x.GetPaginatedAsync(It.IsAny<Core.Dtos.StudentPerformanceFilterRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(paginatedResponse);

        // Act
        var result = await _controller.GetAllPerformance(subject: null, examType: null, fromDate: null, toDate: null, search: null, sortBy: null, sortOrder: "asc", page: 1, limit: 10, studentId: null, cancellationToken: CancellationToken.None);

        // Assert
        result.Should().BeOfType<ActionResult<Core.Dtos.PaginatedResponse<StudentPerformanceDto>>>();
        var actionResult = result as ActionResult<Core.Dtos.PaginatedResponse<StudentPerformanceDto>>;
        actionResult.Should().NotBeNull();
        actionResult!.Result.Should().BeOfType<OkObjectResult>();
        var okResult = actionResult.Result as OkObjectResult;
        var response = okResult!.Value.Should().BeAssignableTo<Core.Dtos.PaginatedResponse<StudentPerformanceDto>>().Subject;
        response.Data.Should().HaveCount(expectedPerformances.Count);
    }

    [Test]
    public async Task GetAllPerformance_WhenStudent_ShouldReturnOnlyOwnRecords()
    {
        // Arrange
        var studentId = TestAuthenticationHelper.CreateTestStudentId();
        TestAuthenticationHelper.SetupControllerContext(_controller, UserRole.Student, studentId);

        var expectedPerformances = new List<StudentPerformanceDto>
        {
            CreateTestPerformanceDto(studentId)
        };

        // Mock the student repository to return a student for the given user ID
        var testStudent = new Core.Entities.Student
        {
            Id = studentId,
            UserId = studentId,
            FirstName = "Test",
            LastName = "Student",
            RollNumber = "TEST001"
        };
        _mockStudentRepository
            .Setup(x => x.GetByUserIdAsync(studentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(testStudent);

        var paginatedResponse = new Core.Dtos.PaginatedResponse<StudentPerformanceDto>(expectedPerformances, expectedPerformances.Count, 1, 10);
        _mockPerformanceService
            .Setup(x => x.GetPaginatedAsync(It.IsAny<Core.Dtos.StudentPerformanceFilterRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(paginatedResponse);

        // Act
        var result = await _controller.GetAllPerformance(subject: null, examType: null, fromDate: null, toDate: null, search: null, sortBy: null, sortOrder: "asc", page: 1, limit: 10, studentId: null, cancellationToken: CancellationToken.None);

        // Assert
        result.Should().BeOfType<ActionResult<Core.Dtos.PaginatedResponse<StudentPerformanceDto>>>();
        var actionResult = result as ActionResult<Core.Dtos.PaginatedResponse<StudentPerformanceDto>>;
        actionResult.Should().NotBeNull();
        actionResult!.Result.Should().BeOfType<OkObjectResult>();
        var okResult = actionResult.Result as OkObjectResult;
        var response = okResult!.Value.Should().BeAssignableTo<Core.Dtos.PaginatedResponse<StudentPerformanceDto>>().Subject;
        response.Data.Should().HaveCount(1);
        response.Data.Should().AllSatisfy(p => p.StudentId.Should().Be(studentId));
    }

    [Test]
    public async Task CreatePerformance_WhenAdmin_ShouldSucceed()
    {
        // Arrange
        TestAuthenticationHelper.SetupControllerContext(_controller, UserRole.Admin);

        var request = CreateTestCreateRequest();

        var expectedPerformance = CreateTestPerformanceDto(request.StudentId);
        _mockPerformanceService
            .Setup(x => x.CreateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedPerformance);

        // Act
        var result = await _controller.CreatePerformance(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ActionResult<StudentPerformanceDto>>();
        var actionResult = result as ActionResult<StudentPerformanceDto>;
        actionResult.Should().NotBeNull();
        actionResult!.Result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = actionResult.Result as CreatedAtActionResult;
        var performance = createdResult!.Value.Should().BeOfType<StudentPerformanceDto>().Subject;
        performance.Id.Should().Be(expectedPerformance.Id);
    }

    [Test]
    public async Task CreatePerformance_WhenStudent_ShouldBeForbidden()
    {
        // Arrange
        TestAuthenticationHelper.SetupControllerContext(_controller, UserRole.Student);

        var request = CreateTestCreateRequest();

        // Act
        var result = await _controller.CreatePerformance(request, CancellationToken.None);

        // Assert
        // The controller now checks authorization and returns Forbid() for unauthorized users
        result.Should().BeOfType<ActionResult<StudentPerformanceDto>>();
        var actionResult = result as ActionResult<StudentPerformanceDto>;
        actionResult.Should().NotBeNull();
        // The result should be ForbidResult due to the authorization check in the controller
        actionResult!.Result.Should().BeOfType<ForbidResult>();
    }

    [Test]
    public async Task UpdatePerformance_WhenFaculty_ShouldSucceed()
    {
        // Arrange
        TestAuthenticationHelper.SetupControllerContext(_controller, UserRole.Faculty);

        var performanceId = TestAuthenticationHelper.CreateTestPerformanceId();
        var request = CreateTestUpdateRequest();

        var existingPerformance = CreateTestPerformanceDto();
        _mockPerformanceService
            .Setup(x => x.GetByIdAsync(performanceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPerformance);

        var expectedPerformance = CreateTestPerformanceDto();
        _mockPerformanceService
            .Setup(x => x.UpdateAsync(performanceId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedPerformance);

        // Act
        var result = await _controller.UpdatePerformance(performanceId, request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ActionResult<StudentPerformanceDto>>();
        var actionResult = result as ActionResult<StudentPerformanceDto>;
        actionResult.Should().NotBeNull();
        actionResult!.Result.Should().BeOfType<OkObjectResult>();
        var okResult = actionResult.Result as OkObjectResult;
        var performance = okResult!.Value.Should().BeOfType<StudentPerformanceDto>().Subject;
        performance.Id.Should().Be(expectedPerformance.Id);
    }

    [Test]
    public async Task DeletePerformance_WhenFaculty_ShouldBeForbidden()
    {
        // Arrange
        TestAuthenticationHelper.SetupControllerContext(_controller, UserRole.Faculty);

        var performanceId = TestAuthenticationHelper.CreateTestPerformanceId();

        // Act
        var result = await _controller.DeletePerformance(performanceId, CancellationToken.None);

        // Assert
        // The controller now checks authorization and returns Forbid() for unauthorized users
        // The result should be ForbidResult due to the authorization check in the controller
        result.Should().BeOfType<ForbidResult>();
    }

    [Test]
    public async Task DeletePerformance_WhenAdmin_ShouldSucceed()
    {
        // Arrange
        TestAuthenticationHelper.SetupControllerContext(_controller, UserRole.Admin);

        var performanceId = TestAuthenticationHelper.CreateTestPerformanceId();

        _mockPerformanceService
            .Setup(x => x.ExistsAsync(performanceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockPerformanceService
            .Setup(x => x.DeleteAsync(performanceId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeletePerformance(performanceId, CancellationToken.None);

        // Assert
        // The controller returns NoContent() which is a specific result type
        result.Should().BeOfType<NoContentResult>();
    }


}
