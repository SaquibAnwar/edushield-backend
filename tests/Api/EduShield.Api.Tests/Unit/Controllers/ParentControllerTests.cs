using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using EduShield.Api.Controllers;
using EduShield.Core.Dtos;
using EduShield.Core.Interfaces;
using EduShield.Core.Enums;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;

namespace EduShield.Api.Tests.Unit;

[TestFixture]
public class ParentControllerTests
{
    private Mock<IParentService> _mockParentService;
    private Mock<ILogger<ParentController>> _mockLogger;
    private ParentController _controller;
    private ClaimsPrincipal _adminUser;
    private ClaimsPrincipal _parentUser;

    [SetUp]
    public void Setup()
    {
        _mockParentService = new Mock<IParentService>();
        _mockLogger = new Mock<ILogger<ParentController>>();
        _controller = new ParentController(_mockParentService.Object, _mockLogger.Object);

        // Setup test users with proper claims
        _adminUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, UserRole.Admin.ToString()),
            new Claim("role", UserRole.Admin.ToString()),
            new Claim("userId", Guid.NewGuid().ToString())
        }, "test"));

        _parentUser = new ClaimsPrincipal(new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, UserRole.Parent.ToString()),
            new Claim("role", UserRole.Parent.ToString()),
            new Claim("userId", Guid.NewGuid().ToString())
        }, "test")));

        // Set the controller's User property to admin user by default
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = _adminUser
            }
        };
    }

    #region GetAllParents Tests

    [Test]
    public async Task GetAllParents_ReturnsOkResult()
    {
        // Arrange
        var parents = new List<ParentResponse> { CreateMockParentResponse() };
        _mockParentService.Setup(x => x.GetAllAsync()).ReturnsAsync(parents);

        // Act
        var result = await _controller.GetAllParents();

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult?.Value, Is.EqualTo(parents));
        _mockParentService.Verify(x => x.GetAllAsync(), Times.Once);
    }

    [Test]
    public async Task GetAllParents_ServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        _mockParentService.Setup(x => x.GetAllAsync()).ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetAllParents();

        // Assert
        Assert.That(result.Result, Is.InstanceOf<ObjectResult>());
        var objectResult = result.Result as ObjectResult;
        Assert.That(objectResult?.StatusCode, Is.EqualTo(500));
        Assert.That(objectResult?.Value, Is.Not.Null);
    }

    #endregion

    #region GetParentById Tests

    [Test]
    public async Task GetParentById_ValidId_ReturnsOkResult()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var parent = CreateMockParentResponse();
        _mockParentService.Setup(x => x.GetByIdAsync(parentId)).ReturnsAsync(parent);

        // Act
        var result = await _controller.GetParentById(parentId);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult?.Value, Is.EqualTo(parent));
        _mockParentService.Verify(x => x.GetByIdAsync(parentId), Times.Once);
    }

    [Test]
    public async Task GetParentById_ParentNotFound_ReturnsNotFound()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        _mockParentService.Setup(x => x.GetByIdAsync(parentId)).ReturnsAsync((ParentResponse?)null);

        // Act
        var result = await _controller.GetParentById(parentId);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
        var notFoundResult = result.Result as NotFoundObjectResult;
        Assert.That(notFoundResult?.Value, Is.Not.Null);
    }

    #endregion

    #region GetParentWithChildren Tests

    [Test]
    public async Task GetParentWithChildren_ValidId_ReturnsOkResult()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var parent = CreateMockParentResponse();
        parent.Children = new List<ParentChildInfo> { CreateMockChildInfo() };
        _mockParentService.Setup(x => x.GetWithChildrenByIdAsync(parentId)).ReturnsAsync(parent);

        // Act
        var result = await _controller.GetParentWithChildren(parentId);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult?.Value, Is.EqualTo(parent));
        Assert.That(((ParentResponse)okResult.Value).Children, Has.Count.EqualTo(1));
    }

    #endregion

    #region CreateParent Tests

    [Test]
    public async Task CreateParent_ValidRequest_ReturnsCreatedResult()
    {
        // Arrange
        var request = CreateMockCreateParentRequest();
        var createdParent = CreateMockParentResponse();
        _mockParentService.Setup(x => x.CreateAsync(request)).ReturnsAsync(createdParent);

        // Act
        var result = await _controller.CreateParent(request);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<CreatedAtActionResult>());
        var createdAtResult = result.Result as CreatedAtActionResult;
        Assert.That(createdAtResult?.Value, Is.EqualTo(createdParent));
        _mockParentService.Verify(x => x.CreateAsync(request), Times.Once);
    }

    [Test]
    public async Task CreateParent_ValidationError_ReturnsBadRequest()
    {
        // Arrange
        var request = CreateMockCreateParentRequest();
        _mockParentService.Setup(x => x.CreateAsync(request))
            .ThrowsAsync(new ArgumentException("Validation failed: FirstName is required."));

        // Act
        var result = await _controller.CreateParent(request);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
        var badRequestResult = result.Result as BadRequestObjectResult;
        Assert.That(badRequestResult?.Value, Is.Not.Null);
    }

    #endregion

    #region UpdateParent Tests

    [Test]
    public async Task UpdateParent_ValidRequest_ReturnsOkResult()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var request = CreateMockUpdateParentRequest();
        var updatedParent = CreateMockParentResponse();
        _mockParentService.Setup(x => x.UpdateAsync(parentId, request)).ReturnsAsync(updatedParent);

        // Act
        var result = await _controller.UpdateParent(parentId, request);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult?.Value, Is.EqualTo(updatedParent));
        _mockParentService.Verify(x => x.UpdateAsync(parentId, request), Times.Once);
    }

    [Test]
    public async Task UpdateParent_ParentNotFound_ReturnsNotFound()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var request = CreateMockUpdateParentRequest();
        _mockParentService.Setup(x => x.UpdateAsync(parentId, request))
            .ThrowsAsync(new KeyNotFoundException("Parent not found."));

        // Act
        var result = await _controller.UpdateParent(parentId, request);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
        var notFoundResult = result.Result as NotFoundObjectResult;
        Assert.That(notFoundResult?.Value, Is.Not.Null);
    }

    #endregion

    #region DeleteParent Tests

    [Test]
    public async Task DeleteParent_ValidId_ReturnsOkResult()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        _mockParentService.Setup(x => x.DeleteAsync(parentId)).ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteParent(parentId);

        // Assert
        Assert.That(result, Is.InstanceOf<NoContentResult>());
        _mockParentService.Verify(x => x.DeleteAsync(parentId), Times.Once);
    }

    [Test]
    public async Task DeleteParent_ParentNotFound_ReturnsNotFound()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        _mockParentService.Setup(x => x.DeleteAsync(parentId)).ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteParent(parentId);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        _mockParentService.Verify(x => x.DeleteAsync(parentId), Times.Once);
    }

    #endregion

    #region SearchParents Tests

    [Test]
    public async Task SearchParents_ValidQuery_ReturnsOkResult()
    {
        // Arrange
        var searchTerm = "John";
        var parents = new List<ParentResponse> { CreateMockParentResponse() };
        _mockParentService.Setup(x => x.SearchByNameAsync(searchTerm)).ReturnsAsync(parents);

        // Act
        var result = await _controller.SearchParents(searchTerm);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult?.Value, Is.EqualTo(parents));
        _mockParentService.Verify(x => x.SearchByNameAsync(searchTerm), Times.Once);
    }

    [Test]
    public async Task SearchParents_EmptyQuery_ReturnsBadRequest()
    {
        // Arrange
        var searchTerm = "";

        // Act
        var result = await _controller.SearchParents(searchTerm);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
        var badRequestResult = result.Result as BadRequestObjectResult;
        Assert.That(badRequestResult?.Value, Is.Not.Null);
    }

    #endregion

    #region GetParentStatistics Tests

    [Test]
    public async Task GetParentStatistics_ReturnsOkResult()
    {
        // Arrange
        var statistics = new ParentStatistics
        {
            TotalParents = 100,
            ActiveParents = 95,
            ParentsWithChildren = 80,
            AverageChildrenPerParent = 2
        };
        _mockParentService.Setup(x => x.GetStatisticsAsync()).ReturnsAsync(statistics);

        // Act
        var result = await _controller.GetParentStatistics();

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult?.Value, Is.EqualTo(statistics));
        _mockParentService.Verify(x => x.GetStatisticsAsync(), Times.Once);
    }

    #endregion

    #region Helper Methods

    private static ParentResponse CreateMockParentResponse()
    {
        return new ParentResponse
        {
            Id = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            PhoneNumber = "+1234567890",
            DateOfBirth = new DateTime(1980, 1, 1),
            Address = "123 Main St",
            City = "New York",
            State = "NY",
            PostalCode = "10001",
            Country = "USA",
            Gender = Gender.Male,
            Occupation = "Engineer",
            Employer = "Tech Corp",
            ParentType = ParentType.Primary,
            IsEmergencyContact = true,
            IsAuthorizedToPickup = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            FullName = "John Doe",
            Age = 44,
            FullAddress = "123 Main St, New York, NY 10001, USA",
            ChildrenCount = 2,
            IsPrimaryParent = true,
            Children = new List<ParentChildInfo>()
        };
    }

    private static ParentChildInfo CreateMockChildInfo()
    {
        return new ParentChildInfo
        {
            Id = Guid.NewGuid(),
            FirstName = "Jane",
            LastName = "Doe",
            RollNumber = "student_001",
            Grade = "10th Grade",
            Section = "A",
            Status = StudentStatus.Active,
            EnrollmentDate = DateTime.Today.AddYears(-2),
            FullName = "Jane Doe",
            Age = 16,
            IsEnrolled = true
        };
    }

    private static CreateParentRequest CreateMockCreateParentRequest()
    {
        return new CreateParentRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            PhoneNumber = "+1234567890",
            DateOfBirth = new DateTime(1980, 1, 1),
            Address = "123 Main St",
            City = "New York",
            State = "NY",
            PostalCode = "10001",
            Country = "USA",
            Gender = Gender.Male,
            Occupation = "Engineer",
            Employer = "Tech Corp",
            ParentType = ParentType.Primary,
            IsEmergencyContact = true,
            IsAuthorizedToPickup = true
        };
    }

    private static UpdateParentRequest CreateMockUpdateParentRequest()
    {
        return new UpdateParentRequest
        {
            FirstName = "John",
            LastName = "Doe",
            PhoneNumber = "+1234567890",
            Address = "123 Main St",
            City = "New York",
            State = "NY",
            PostalCode = "10001",
            Occupation = "Senior Engineer",
            Employer = "Tech Corp"
        };
    }

    #endregion
}
