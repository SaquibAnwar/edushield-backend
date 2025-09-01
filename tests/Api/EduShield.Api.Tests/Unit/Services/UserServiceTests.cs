using Moq;
using NUnit.Framework;
using FluentAssertions;
using EduShield.Core.Services;
using EduShield.Core.Interfaces;
using EduShield.Core.Entities;
using EduShield.Core.Dtos;
using EduShield.Core.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EduShield.Api.Tests.Unit;

[TestFixture]
public class UserServiceTests
{
    private Mock<IUserRepository> _mockUserRepository = null!;
    private Mock<ILogger<UserService>> _mockLogger = null!;
    private UserService _service = null!;

    [SetUp]
    public void Setup()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockLogger = new Mock<ILogger<UserService>>();
        _service = new UserService(_mockUserRepository.Object, _mockLogger.Object);
    }

    [Test]
    public async Task GetAllUsersAsync_ReturnsAllUsers()
    {
        // Arrange
        var users = new List<User>
        {
            CreateTestUser(Guid.NewGuid(), "john.doe@example.com", UserRole.Admin),
            CreateTestUser(Guid.NewGuid(), "jane.smith@example.com", UserRole.Faculty),
            CreateTestUser(Guid.NewGuid(), "bob.wilson@example.com", UserRole.Parent)
        };

        _mockUserRepository
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(users);

        // Act
        var result = await _service.GetAllUsersAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().BeEquivalentTo(users.Select(u => new UserDto
        {
            Id = u.Id,
            Email = u.Email,
            Name = u.Name,
            ProfilePictureUrl = u.ProfilePictureUrl,
            Role = u.Role,
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt,
            UpdatedAt = u.UpdatedAt,
            LastLoginAt = u.LastLoginAt
        }));

        _mockUserRepository.Verify(x => x.GetAllAsync(), Times.Once);
    }

    [Test]
    public async Task GetAllUsersAsync_EmptyList_ReturnsEmptyList()
    {
        // Arrange
        var users = new List<User>();

        _mockUserRepository
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(users);

        // Act
        var result = await _service.GetAllUsersAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();

        _mockUserRepository.Verify(x => x.GetAllAsync(), Times.Once);
    }

    [Test]
    public async Task GetUserByIdAsync_ValidId_ReturnsUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(userId, "john.doe@example.com", UserRole.Admin);

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        var result = await _service.GetUserByIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(userId);
        result.Email.Should().Be("john.doe@example.com");
        result.Role.Should().Be(UserRole.Admin);

        _mockUserRepository.Verify(x => x.GetByIdAsync(userId), Times.Once);
    }

    [Test]
    public async Task GetUserByIdAsync_InvalidId_ReturnsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _service.GetUserByIdAsync(userId);

        // Assert
        result.Should().BeNull();

        _mockUserRepository.Verify(x => x.GetByIdAsync(userId), Times.Once);
    }

    [Test]
    public async Task GetUserByEmailAsync_ValidEmail_ReturnsUser()
    {
        // Arrange
        var email = "john.doe@example.com";
        var user = CreateTestUser(Guid.NewGuid(), email, UserRole.Admin);

        _mockUserRepository
            .Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync(user);

        // Act
        var result = await _service.GetUserByEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(email);
        result.Role.Should().Be(UserRole.Admin);

        _mockUserRepository.Verify(x => x.GetByEmailAsync(email), Times.Once);
    }

    [Test]
    public async Task GetUserByEmailAsync_InvalidEmail_ReturnsNull()
    {
        // Arrange
        var email = "nonexistent@example.com";

        _mockUserRepository
            .Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _service.GetUserByEmailAsync(email);

        // Assert
        result.Should().BeNull();

        _mockUserRepository.Verify(x => x.GetByEmailAsync(email), Times.Once);
    }

    [Test]
    public async Task UpdateUserRoleAsync_ValidRequest_UpdatesRoleSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var newRole = UserRole.Faculty;
        var existingUser = CreateTestUser(userId, "john.doe@example.com", UserRole.Admin);
        var updatedUser = CreateTestUser(userId, "john.doe@example.com", newRole);

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(existingUser);

        _mockUserRepository
            .Setup(x => x.UpdateAsync(existingUser))
            .ReturnsAsync(updatedUser);

        // Act
        var result = await _service.UpdateUserRoleAsync(userId, newRole);

        // Assert
        result.Should().NotBeNull();
        result.Role.Should().Be(newRole);

        _mockUserRepository.Verify(x => x.GetByIdAsync(userId), Times.Once);
        _mockUserRepository.Verify(x => x.UpdateAsync(existingUser), Times.Once);
    }

    [Test]
    public async Task UpdateUserRoleAsync_UserNotFound_ReturnsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var newRole = UserRole.Faculty;

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _service.UpdateUserRoleAsync(userId, newRole);

        // Assert
        result.Should().BeNull();

        _mockUserRepository.Verify(x => x.GetByIdAsync(userId), Times.Once);
        _mockUserRepository.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Test]
    public async Task UpdateUserRoleAsync_SameRole_ReturnsUserWithoutUpdate()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingRole = UserRole.Admin;
        var existingUser = CreateTestUser(userId, "john.doe@example.com", existingRole);

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _service.UpdateUserRoleAsync(userId, existingRole);

        // Assert
        result.Should().NotBeNull();
        result.Role.Should().Be(existingRole);

        _mockUserRepository.Verify(x => x.GetByIdAsync(userId), Times.Once);
        _mockUserRepository.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Once);
    }

    [Test]
    public async Task UpdateUserStatusAsync_ValidRequest_UpdatesStatusSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var newStatus = false; // Inactive
        var existingUser = CreateTestUser(userId, "john.doe@example.com", UserRole.Admin);
        var updatedUser = CreateTestUser(userId, "john.doe@example.com", UserRole.Admin);
        updatedUser.IsActive = newStatus;

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(existingUser);

        _mockUserRepository
            .Setup(x => x.UpdateAsync(existingUser))
            .ReturnsAsync(updatedUser);

        // Act
        var result = await _service.UpdateUserStatusAsync(userId, newStatus);

        // Assert
        result.Should().NotBeNull();
        result.IsActive.Should().Be(newStatus);

        _mockUserRepository.Verify(x => x.GetByIdAsync(userId), Times.Once);
        _mockUserRepository.Verify(x => x.UpdateAsync(existingUser), Times.Once);
    }

    [Test]
    public async Task UpdateUserStatusAsync_UserNotFound_ReturnsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var newStatus = false;

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _service.UpdateUserStatusAsync(userId, newStatus);

        // Assert
        result.Should().BeNull();

        _mockUserRepository.Verify(x => x.GetByIdAsync(userId), Times.Once);
        _mockUserRepository.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Test]
    public async Task UpdateUserStatusAsync_SameStatus_ReturnsUserWithoutUpdate()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingStatus = true; // Active
        var existingUser = CreateTestUser(userId, "john.doe@example.com", UserRole.Admin);

        _mockUserRepository
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _service.UpdateUserStatusAsync(userId, existingStatus);

        // Assert
        result.Should().NotBeNull();
        result.IsActive.Should().Be(existingStatus);

        _mockUserRepository.Verify(x => x.GetByIdAsync(userId), Times.Once);
        _mockUserRepository.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Once);
    }

    [Test]
    public async Task CreateOrUpdateUserAsync_NewUser_CreatesUserSuccessfully()
    {
        // Arrange
        var email = "newuser@example.com";
        var name = "New User";

        var createdUser = CreateTestUser(Guid.NewGuid(), email, UserRole.Student);
        createdUser.Name = name;

        _mockUserRepository
            .Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync((User?)null);

        _mockUserRepository
            .Setup(x => x.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync(createdUser);

        // Act
        var result = await _service.CreateOrUpdateUserAsync(email, name);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be(email);
        result.Name.Should().Be(name);
        result.Role.Should().Be(UserRole.Student);

        _mockUserRepository.Verify(x => x.GetByEmailAsync(email), Times.Once);
        _mockUserRepository.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Once);
        _mockUserRepository.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Test]
    public async Task CreateOrUpdateUserAsync_ExistingUser_UpdatesUserSuccessfully()
    {
        // Arrange
        var email = "existing@example.com";
        var name = "Updated Name";

        var existingUser = CreateTestUser(Guid.NewGuid(), email, UserRole.Student);
        var updatedUser = CreateTestUser(existingUser.Id, email, UserRole.Student);
        updatedUser.Name = name;

        _mockUserRepository
            .Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync(existingUser);

        _mockUserRepository
            .Setup(x => x.UpdateAsync(existingUser))
            .ReturnsAsync(updatedUser);

        // Act
        var result = await _service.CreateOrUpdateUserAsync(email, name);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be(email);
        result.Name.Should().Be(name);

        _mockUserRepository.Verify(x => x.GetByEmailAsync(email), Times.Once);
        _mockUserRepository.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Never);
        _mockUserRepository.Verify(x => x.UpdateAsync(existingUser), Times.Once);
    }

    [Test]
    public async Task CreateOrUpdateUserAsync_ExistingUser_UpdatesOnlyProvidedFields()
    {
        // Arrange
        var email = "existing@example.com";
        var name = "Updated Name";

        var existingUser = CreateTestUser(Guid.NewGuid(), email, UserRole.Student);
        existingUser.Name = "Original Name";

        var updatedUser = CreateTestUser(existingUser.Id, email, UserRole.Student);
        updatedUser.Name = name;

        _mockUserRepository
            .Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync(existingUser);

        _mockUserRepository
            .Setup(x => x.UpdateAsync(existingUser))
            .ReturnsAsync(updatedUser);

        // Act
        var result = await _service.CreateOrUpdateUserAsync(email, name);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be(email);
        result.Name.Should().Be(name);

        _mockUserRepository.Verify(x => x.UpdateAsync(existingUser), Times.Once);
    }

    [Test]
    public async Task CreateOrUpdateUserAsync_WithGoogleId_CreatesUserWithGoogleId()
    {
        // Arrange
        var email = "googleuser@example.com";
        var name = "Google User";
        var googleId = "google123";
        var profilePictureUrl = "https://example.com/photo.jpg";

        var createdUser = CreateTestUser(Guid.NewGuid(), email, UserRole.Student);
        createdUser.GoogleId = googleId;
        createdUser.ProfilePictureUrl = profilePictureUrl;

        _mockUserRepository
            .Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync((User?)null);

        _mockUserRepository
            .Setup(x => x.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync(createdUser);

        // Act
        var result = await _service.CreateOrUpdateUserAsync(email, name, googleId, profilePictureUrl);

        // Assert
        result.Should().NotBeNull();
        result.GoogleId.Should().Be(googleId);
        result.ProfilePictureUrl.Should().Be(profilePictureUrl);

        _mockUserRepository.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Once);
    }

    #region Helper Methods
    private static User CreateTestUser(Guid id, string email, UserRole role)
    {
        return new User
        {
            Id = id,
            Email = email,
            Name = "Test User",
            Role = role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
    #endregion
}
