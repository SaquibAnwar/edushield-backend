using EduShield.Api.Controllers;
using EduShield.Core.Dtos;
using EduShield.Core.Enums;
using EduShield.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.Security.Claims;

namespace EduShield.Api.Tests.Unit;

[TestFixture]
public class StudentFeeControllerTests
{
    private Mock<IStudentFeeService> _mockFeeService;
    private Mock<ILogger<StudentFeeController>> _mockLogger;
    private StudentFeeController _controller;
    private ClaimsPrincipal _adminUser;
    private ClaimsPrincipal _studentUser;
    private ClaimsPrincipal _parentUser;

    [SetUp]
    public void Setup()
    {
        _mockFeeService = new Mock<IStudentFeeService>();
        _mockLogger = new Mock<ILogger<StudentFeeController>>();
        _controller = new StudentFeeController(_mockFeeService.Object, _mockLogger.Object);

        // Setup test users with proper claims
        _adminUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, UserRole.Admin.ToString()),
            new Claim("role", UserRole.Admin.ToString()),
            new Claim("userId", Guid.NewGuid().ToString())
        }, "test"));

        _studentUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, UserRole.Student.ToString()),
            new Claim("role", UserRole.Student.ToString()),
            new Claim("userId", Guid.NewGuid().ToString())
        }, "test"));

        _parentUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, UserRole.Parent.ToString()),
            new Claim("role", UserRole.Parent.ToString()),
            new Claim("userId", Guid.NewGuid().ToString())
        }, "test"));
    }

    [Test]
    public async Task GetAllFees_AdminUser_ReturnsAllFees()
    {
        // Arrange
        var expectedFees = CreateSampleFees();
        _mockFeeService.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedFees);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _adminUser }
        };

        // Act
        var result = await _controller.GetAllFees(null, null, null, null, null, CancellationToken.None);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        var returnedFees = okResult?.Value as IEnumerable<StudentFeeDto>;
        Assert.That(returnedFees, Is.Not.Null);
        Assert.That(returnedFees.Count(), Is.EqualTo(expectedFees.Count()));
    }

    [Test]
    public async Task GetAllFees_StudentUser_ReturnsOnlyOwnFees()
    {
        // Arrange
        var studentId = Guid.Parse(_studentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
        var expectedFees = CreateSampleFees().Where(f => f.StudentId == studentId).ToList();
        
        _mockFeeService.Setup(x => x.GetByStudentIdAsync(studentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedFees);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _studentUser }
        };

        // Act
        var result = await _controller.GetAllFees(null, null, null, null, null, CancellationToken.None);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        var returnedFees = okResult?.Value as IEnumerable<StudentFeeDto>;
        Assert.That(returnedFees, Is.Not.Null);
        Assert.That(returnedFees.Count(), Is.EqualTo(expectedFees.Count));
    }

    [Test]
    public async Task GetFeeById_ExistingFee_ReturnsFee()
    {
        // Arrange
        var feeId = Guid.NewGuid();
        var expectedFee = CreateSampleFee(feeId);
        _mockFeeService.Setup(x => x.GetByIdAsync(feeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedFee);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _adminUser }
        };

        // Act
        var result = await _controller.GetFeeById(feeId, CancellationToken.None);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        var returnedFee = okResult?.Value as StudentFeeDto;
        Assert.That(returnedFee, Is.Not.Null);
        Assert.That(returnedFee.Id, Is.EqualTo(feeId));
    }

    [Test]
    public async Task GetFeeById_NonExistentFee_ReturnsNotFound()
    {
        // Arrange
        var feeId = Guid.NewGuid();
        _mockFeeService.Setup(x => x.GetByIdAsync(feeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((StudentFeeDto?)null);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _adminUser }
        };

        // Act
        var result = await _controller.GetFeeById(feeId, CancellationToken.None);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task CreateFee_ValidRequest_ReturnsCreatedFee()
    {
        // Arrange
        var request = new CreateStudentFeeRequest
        {
            StudentId = Guid.NewGuid(),
            FeeType = FeeType.Tuition,
            Term = "2024-Q1",
            TotalAmount = 5000m,
            DueDate = DateTime.Today.AddMonths(1)
        };

        var createdFee = CreateSampleFee(Guid.NewGuid());
        _mockFeeService.Setup(x => x.CreateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdFee);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _adminUser }
        };

        // Act
        var result = await _controller.CreateFee(request, CancellationToken.None);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<CreatedAtActionResult>());
        var createdResult = result.Result as CreatedAtActionResult;
        var returnedFee = createdResult?.Value as StudentFeeDto;
        Assert.That(returnedFee, Is.Not.Null);
    }

    [Test]
    public async Task CreateFee_InvalidRequest_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateStudentFeeRequest
        {
            StudentId = Guid.NewGuid(),
            FeeType = FeeType.Tuition,
            Term = "2024-Q1",
            TotalAmount = 5000m,
            DueDate = DateTime.Today.AddMonths(1)
        };

        _mockFeeService.Setup(x => x.CreateAsync(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Student not found"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _adminUser }
        };

        // Act
        var result = await _controller.CreateFee(request, CancellationToken.None);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task UpdateFee_ValidRequest_ReturnsUpdatedFee()
    {
        // Arrange
        var feeId = Guid.NewGuid();
        var request = new UpdateStudentFeeRequest
        {
            TotalAmount = 6000m,
            Notes = "Updated notes"
        };

        var updatedFee = CreateSampleFee(feeId);
        _mockFeeService.Setup(x => x.UpdateAsync(feeId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedFee);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _adminUser }
        };

        // Act
        var result = await _controller.UpdateFee(feeId, request, CancellationToken.None);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        var returnedFee = okResult?.Value as StudentFeeDto;
        Assert.That(returnedFee, Is.Not.Null);
    }

    [Test]
    public async Task DeleteFee_ExistingFee_ReturnsNoContent()
    {
        // Arrange
        var feeId = Guid.NewGuid();
        _mockFeeService.Setup(x => x.ExistsAsync(feeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockFeeService.Setup(x => x.DeleteAsync(feeId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _adminUser }
        };

        // Act
        var result = await _controller.DeleteFee(feeId, CancellationToken.None);

        // Assert
        Assert.That(result, Is.InstanceOf<NoContentResult>());
    }

    [Test]
    public async Task DeleteFee_NonExistentFee_ReturnsNotFound()
    {
        // Arrange
        var feeId = Guid.NewGuid();
        _mockFeeService.Setup(x => x.ExistsAsync(feeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _adminUser }
        };

        // Act
        var result = await _controller.DeleteFee(feeId, CancellationToken.None);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task MakePayment_ValidPayment_ReturnsPaymentResult()
    {
        // Arrange
        var feeId = Guid.NewGuid();
        var studentId = Guid.Parse(_studentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
        var request = new PaymentRequest
        {
            Amount = 1000m,
            PaymentMethod = "Credit Card"
        };

        var paymentResult = new PaymentResult
        {
            Success = true,
            TransactionId = Guid.NewGuid().ToString(),
            AmountPaid = 1000m,
            NewAmountDue = 4000m,
            NewPaymentStatus = "Partial",
            PaymentDate = DateTime.UtcNow
        };

        // Create a fee that belongs to the student user
        var sampleFee = CreateSampleFee(feeId);
        sampleFee.StudentId = studentId; // Ensure the fee belongs to the student

        _mockFeeService.Setup(x => x.GetByIdAsync(feeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sampleFee);
        _mockFeeService.Setup(x => x.MakePaymentAsync(feeId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(paymentResult);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _studentUser }
        };

        // Act
        var result = await _controller.MakePayment(feeId, request, CancellationToken.None);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        var returnedResult = okResult?.Value as PaymentResult;
        Assert.That(returnedResult, Is.Not.Null);
        Assert.That(returnedResult.Success, Is.True);
    }

    [Test]
    public async Task MakePayment_FailedPayment_ReturnsBadRequest()
    {
        // Arrange
        var feeId = Guid.NewGuid();
        var studentId = Guid.Parse(_studentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
        var request = new PaymentRequest
        {
            Amount = 1000m,
            PaymentMethod = "Credit Card"
        };

        var paymentResult = new PaymentResult
        {
            Success = false,
            ErrorMessage = "Payment failed",
            PaymentDate = DateTime.UtcNow
        };

        // Create a fee that belongs to the student user
        var sampleFee = CreateSampleFee(feeId);
        sampleFee.StudentId = studentId; // Ensure the fee belongs to the student

        _mockFeeService.Setup(x => x.GetByIdAsync(feeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sampleFee);
        _mockFeeService.Setup(x => x.MakePaymentAsync(feeId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(paymentResult);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _studentUser }
        };

        // Act
        var result = await _controller.MakePayment(feeId, request, CancellationToken.None);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task GetByFeeType_ValidType_ReturnsFees()
    {
        // Arrange
        var feeType = FeeType.Tuition;
        var expectedFees = CreateSampleFees().Where(f => f.FeeType == feeType).ToList();
        
        _mockFeeService.Setup(x => x.GetByFeeTypeAsync(feeType, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedFees);

        // Act
        var result = await _controller.GetByFeeType(feeType, CancellationToken.None);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        var returnedFees = okResult?.Value as IEnumerable<StudentFeeDto>;
        Assert.That(returnedFees, Is.Not.Null);
        Assert.That(returnedFees.All(f => f.FeeType == feeType), Is.True);
    }

    [Test]
    public async Task GetByTerm_ValidTerm_ReturnsFees()
    {
        // Arrange
        var term = "2024-Q1";
        var expectedFees = CreateSampleFees().Where(f => f.Term == term).ToList();
        
        _mockFeeService.Setup(x => x.GetByTermAsync(term, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedFees);

        // Act
        var result = await _controller.GetByTerm(term, CancellationToken.None);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        var returnedFees = okResult?.Value as IEnumerable<StudentFeeDto>;
        Assert.That(returnedFees, Is.Not.Null);
        Assert.That(returnedFees.All(f => f.Term == term), Is.True);
    }

    [Test]
    public async Task GetOverdueFees_AdminUser_ReturnsOverdueFees()
    {
        // Arrange
        var expectedFees = CreateSampleFees().Where(f => f.IsOverdue).ToList();
        _mockFeeService.Setup(x => x.GetOverdueAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedFees);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _adminUser }
        };

        // Act
        var result = await _controller.GetOverdueFees(CancellationToken.None);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        var returnedFees = okResult?.Value as IEnumerable<StudentFeeDto>;
        Assert.That(returnedFees, Is.Not.Null);
        Assert.That(returnedFees.All(f => f.IsOverdue), Is.True);
    }

    [Test]
    public async Task CalculateLateFees_AdminUser_ReturnsSuccess()
    {
        // Arrange
        var updatedCount = 5;
        _mockFeeService.Setup(x => x.CalculateLateFeesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedCount);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _adminUser }
        };

        // Act
        var result = await _controller.CalculateLateFees(CancellationToken.None);

        // Assert
        Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
        var okResult = result.Result as OkObjectResult;
        var response = okResult?.Value as dynamic;
        Assert.That(response, Is.Not.Null);
    }

    private static StudentFeeDto CreateSampleFee(Guid id)
    {
        return new StudentFeeDto
        {
            Id = id,
            StudentId = Guid.NewGuid(),
            StudentFirstName = "John",
            StudentLastName = "Doe",
            StudentRollNumber = "ST001",
            FeeType = FeeType.Tuition,
            Term = "2024-Q1",
            TotalAmount = 5000m,
            AmountPaid = 0m,
            AmountDue = 5000m,
            PaymentStatus = PaymentStatus.Pending,
            DueDate = DateTime.UtcNow.AddMonths(1),
            FineAmount = 0m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private static List<StudentFeeDto> CreateSampleFees()
    {
        return new List<StudentFeeDto>
        {
            CreateSampleFee(Guid.NewGuid()),
            CreateSampleFee(Guid.NewGuid()),
            CreateSampleFee(Guid.NewGuid())
        };
    }
}
