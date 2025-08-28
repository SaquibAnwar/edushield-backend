using EduShield.Api.Controllers;
using EduShield.Core.Dtos;
using EduShield.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.Security.Claims;

namespace EduShield.Api.Tests.Unit;

[TestFixture]
public class ParentStudentAssignmentControllerTests
{
    private Mock<IParentStudentAssignmentService> _mockService;
    private Mock<ILogger<ParentStudentAssignmentController>> _mockLogger;
    private ParentStudentAssignmentController _controller;

    [SetUp]
    public void SetUp()
    {
        _mockService = new Mock<IParentStudentAssignmentService>();
        _mockLogger = new Mock<ILogger<ParentStudentAssignmentController>>();
        _controller = new ParentStudentAssignmentController(_mockService.Object, _mockLogger.Object);

        // Setup controller context with admin user
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new(ClaimTypes.Email, "admin@test.com"),
            new("role", "Admin")
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = principal
            }
        };
    }

    [Test]
    public async Task GetAllAssignments_ReturnsOkWithAssignments()
    {
        // Arrange
        var assignments = new List<ParentStudentAssignmentDto>
        {
            new()
            {
                ParentId = Guid.NewGuid(),
                StudentId = Guid.NewGuid(),
                ParentFullName = "John Doe",
                StudentFullName = "Jane Doe",
                Relationship = "Father",
                IsPrimaryContact = true,
                IsActive = true
            }
        };

        _mockService.Setup(x => x.GetAllAssignmentsAsync())
            .ReturnsAsync(assignments);

        // Act
        var result = await _controller.GetAllAssignments();

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var returnedAssignments = okResult.Value as IEnumerable<ParentStudentAssignmentDto>;
        Assert.That(returnedAssignments, Is.Not.Null);
        Assert.That(returnedAssignments.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task GetAssignment_ValidIds_ReturnsOkWithAssignment()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var assignment = new ParentStudentAssignmentDto
        {
            ParentId = parentId,
            StudentId = studentId,
            ParentFullName = "John Doe",
            StudentFullName = "Jane Doe",
            Relationship = "Father",
            IsPrimaryContact = true,
            IsActive = true
        };

        _mockService.Setup(x => x.GetAssignmentAsync(parentId, studentId))
            .ReturnsAsync(assignment);

        // Act
        var result = await _controller.GetAssignment(parentId, studentId);

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var returnedAssignment = okResult.Value as ParentStudentAssignmentDto;
        Assert.That(returnedAssignment, Is.Not.Null);
        Assert.That(returnedAssignment.ParentId, Is.EqualTo(parentId));
        Assert.That(returnedAssignment.StudentId, Is.EqualTo(studentId));
    }

    [Test]
    public async Task GetAssignment_AssignmentNotFound_ReturnsNotFound()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var studentId = Guid.NewGuid();

        _mockService.Setup(x => x.GetAssignmentAsync(parentId, studentId))
            .ReturnsAsync((ParentStudentAssignmentDto?)null);

        // Act
        var result = await _controller.GetAssignment(parentId, studentId);

        // Assert
        var notFoundResult = result.Result as NotFoundObjectResult;
        Assert.That(notFoundResult, Is.Not.Null);
        Assert.That(notFoundResult.Value, Is.Not.Null);
    }

    [Test]
    public async Task CreateAssignment_ValidData_ReturnsCreatedAtAction()
    {
        // Arrange
        var createDto = new CreateParentStudentAssignmentDto
        {
            ParentId = Guid.NewGuid(),
            StudentId = Guid.NewGuid(),
            Relationship = "Father",
            IsPrimaryContact = true,
            IsAuthorizedToPickup = true,
            IsEmergencyContact = true,
            Notes = "Primary guardian"
        };

        var createdAssignment = new ParentStudentAssignmentDto
        {
            ParentId = createDto.ParentId,
            StudentId = createDto.StudentId,
            ParentFullName = "John Doe",
            StudentFullName = "Jane Doe",
            Relationship = createDto.Relationship,
            IsPrimaryContact = createDto.IsPrimaryContact,
            IsAuthorizedToPickup = createDto.IsAuthorizedToPickup,
            IsEmergencyContact = createDto.IsEmergencyContact,
            IsActive = true,
            Notes = createDto.Notes
        };

        _mockService.Setup(x => x.CreateAssignmentAsync(createDto))
            .ReturnsAsync(createdAssignment);

        // Act
        var result = await _controller.CreateAssignment(createDto);

        // Assert
        var createdResult = result.Result as CreatedAtActionResult;
        Assert.That(createdResult, Is.Not.Null);
        var returnedAssignment = createdResult.Value as ParentStudentAssignmentDto;
        Assert.That(returnedAssignment, Is.Not.Null);
        Assert.That(returnedAssignment.ParentId, Is.EqualTo(createDto.ParentId));
        Assert.That(returnedAssignment.StudentId, Is.EqualTo(createDto.StudentId));
        Assert.That(returnedAssignment.Relationship, Is.EqualTo(createDto.Relationship));
    }

    [Test]
    public async Task CreateAssignment_InvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        var createDto = new CreateParentStudentAssignmentDto(); // Invalid - missing required fields
        _controller.ModelState.AddModelError("ParentId", "Parent ID is required");

        // Act
        var result = await _controller.CreateAssignment(createDto);

        // Assert
        var badRequestResult = result.Result as BadRequestObjectResult;
        Assert.That(badRequestResult, Is.Not.Null);
    }

    [Test]
    public async Task CreateAssignment_ParentNotFound_ReturnsBadRequest()
    {
        // Arrange
        var createDto = new CreateParentStudentAssignmentDto
        {
            ParentId = Guid.NewGuid(),
            StudentId = Guid.NewGuid(),
            Relationship = "Father"
        };

        _mockService.Setup(x => x.CreateAssignmentAsync(createDto))
            .ThrowsAsync(new ArgumentException("Parent not found"));

        // Act
        var result = await _controller.CreateAssignment(createDto);

        // Assert
        var badRequestResult = result.Result as BadRequestObjectResult;
        Assert.That(badRequestResult, Is.Not.Null);
        Assert.That(badRequestResult.Value, Is.Not.Null);
    }

    [Test]
    public async Task CreateAssignment_AssignmentAlreadyExists_ReturnsConflict()
    {
        // Arrange
        var createDto = new CreateParentStudentAssignmentDto
        {
            ParentId = Guid.NewGuid(),
            StudentId = Guid.NewGuid(),
            Relationship = "Father"
        };

        _mockService.Setup(x => x.CreateAssignmentAsync(createDto))
            .ThrowsAsync(new InvalidOperationException("Assignment already exists"));

        // Act
        var result = await _controller.CreateAssignment(createDto);

        // Assert
        var conflictResult = result.Result as ConflictObjectResult;
        Assert.That(conflictResult, Is.Not.Null);
        Assert.That(conflictResult.Value, Is.Not.Null);
    }

    [Test]
    public async Task UpdateAssignment_ValidData_ReturnsOkWithUpdatedAssignment()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var updateDto = new UpdateParentStudentAssignmentDto
        {
            Relationship = "Guardian",
            IsPrimaryContact = true,
            Notes = "Updated notes"
        };

        var updatedAssignment = new ParentStudentAssignmentDto
        {
            ParentId = parentId,
            StudentId = studentId,
            ParentFullName = "John Doe",
            StudentFullName = "Jane Doe",
            Relationship = "Guardian",
            IsPrimaryContact = true,
            IsActive = true,
            Notes = "Updated notes"
        };

        _mockService.Setup(x => x.UpdateAssignmentAsync(parentId, studentId, updateDto))
            .ReturnsAsync(updatedAssignment);

        // Act
        var result = await _controller.UpdateAssignment(parentId, studentId, updateDto);

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var returnedAssignment = okResult.Value as ParentStudentAssignmentDto;
        Assert.That(returnedAssignment, Is.Not.Null);
        Assert.That(returnedAssignment.Relationship, Is.EqualTo("Guardian"));
        Assert.That(returnedAssignment.IsPrimaryContact, Is.True);
        Assert.That(returnedAssignment.Notes, Is.EqualTo("Updated notes"));
    }

    [Test]
    public async Task UpdateAssignment_AssignmentNotFound_ReturnsNotFound()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var updateDto = new UpdateParentStudentAssignmentDto
        {
            Relationship = "Guardian"
        };

        _mockService.Setup(x => x.UpdateAssignmentAsync(parentId, studentId, updateDto))
            .ThrowsAsync(new ArgumentException("Assignment not found"));

        // Act
        var result = await _controller.UpdateAssignment(parentId, studentId, updateDto);

        // Assert
        var notFoundResult = result.Result as NotFoundObjectResult;
        Assert.That(notFoundResult, Is.Not.Null);
        Assert.That(notFoundResult.Value, Is.Not.Null);
    }

    [Test]
    public async Task DeleteAssignment_ValidIds_ReturnsNoContent()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var studentId = Guid.NewGuid();

        _mockService.Setup(x => x.DeleteAssignmentAsync(parentId, studentId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteAssignment(parentId, studentId);

        // Assert
        Assert.That(result, Is.TypeOf<NoContentResult>());
    }

    [Test]
    public async Task DeleteAssignment_AssignmentNotFound_ReturnsNotFound()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var studentId = Guid.NewGuid();

        _mockService.Setup(x => x.DeleteAssignmentAsync(parentId, studentId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteAssignment(parentId, studentId);

        // Assert
        var notFoundResult = result as NotFoundObjectResult;
        Assert.That(notFoundResult, Is.Not.Null);
        Assert.That(notFoundResult.Value, Is.Not.Null);
    }

    [Test]
    public async Task SetPrimaryContact_ValidIds_ReturnsOkWithSuccessMessage()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var studentId = Guid.NewGuid();

        _mockService.Setup(x => x.SetPrimaryContactAsync(parentId, studentId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.SetPrimaryContact(parentId, studentId);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.Value, Is.Not.Null);
    }

    [Test]
    public async Task SetPrimaryContact_AssignmentNotFound_ReturnsNotFound()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var studentId = Guid.NewGuid();

        _mockService.Setup(x => x.SetPrimaryContactAsync(parentId, studentId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.SetPrimaryContact(parentId, studentId);

        // Assert
        var notFoundResult = result as NotFoundObjectResult;
        Assert.That(notFoundResult, Is.Not.Null);
        Assert.That(notFoundResult.Value, Is.Not.Null);
    }

    [Test]
    public async Task CreateBulkAssignments_ValidData_ReturnsCreatedAtAction()
    {
        // Arrange
        var bulkDto = new BulkParentStudentAssignmentDto
        {
            ParentId = Guid.NewGuid(),
            StudentIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() },
            Relationship = "Father",
            IsPrimaryContact = false,
            IsAuthorizedToPickup = true,
            IsEmergencyContact = true,
            Notes = "Bulk assignment"
        };

        var createdAssignments = new List<ParentStudentAssignmentDto>
        {
            new()
            {
                ParentId = bulkDto.ParentId,
                StudentId = bulkDto.StudentIds[0],
                Relationship = bulkDto.Relationship,
                IsActive = true
            },
            new()
            {
                ParentId = bulkDto.ParentId,
                StudentId = bulkDto.StudentIds[1],
                Relationship = bulkDto.Relationship,
                IsActive = true
            }
        };

        _mockService.Setup(x => x.CreateBulkAssignmentsAsync(bulkDto))
            .ReturnsAsync(createdAssignments);

        // Act
        var result = await _controller.CreateBulkAssignments(bulkDto);

        // Assert
        var createdResult = result.Result as CreatedAtActionResult;
        Assert.That(createdResult, Is.Not.Null);
        var returnedAssignments = createdResult.Value as IEnumerable<ParentStudentAssignmentDto>;
        Assert.That(returnedAssignments, Is.Not.Null);
        Assert.That(returnedAssignments.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task GetStatistics_ReturnsOkWithStatistics()
    {
        // Arrange
        var totalCount = 10;
        var activeCount = 8;
        var relationshipTypes = new Dictionary<string, int>
        {
            { "Father", 5 },
            { "Mother", 3 }
        };

        _mockService.Setup(x => x.GetTotalAssignmentsCountAsync())
            .ReturnsAsync(totalCount);
        _mockService.Setup(x => x.GetActiveAssignmentsCountAsync())
            .ReturnsAsync(activeCount);
        _mockService.Setup(x => x.GetAssignmentsByRelationshipTypeAsync())
            .ReturnsAsync(relationshipTypes);

        // Act
        var result = await _controller.GetStatistics();

        // Assert
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.Value, Is.Not.Null);
    }

    [Test]
    public async Task ValidateAssignment_ValidIds_ReturnsOkWithValidationResult()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        var studentId = Guid.NewGuid();

        _mockService.Setup(x => x.CanAssignParentToStudentAsync(parentId, studentId))
            .ReturnsAsync(true);
        _mockService.Setup(x => x.IsParentAssignedToStudentAsync(parentId, studentId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.ValidateAssignment(parentId, studentId);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.Value, Is.Not.Null);
    }
}