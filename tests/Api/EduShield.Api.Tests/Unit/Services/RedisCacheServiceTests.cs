using Moq;
using NUnit.Framework;
using FluentAssertions;
using EduShield.Core.Services;
using EduShield.Core.Interfaces;
using EduShield.Core.Dtos;
using EduShield.Core.Enums;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace EduShield.Api.Tests.Unit;

[TestFixture]
public class RedisCacheServiceTests
{
    private Mock<IDistributedCache> _mockDistributedCache = null!;
    private RedisCacheService _cacheService = null!;

    [SetUp]
    public void Setup()
    {
        _mockDistributedCache = new Mock<IDistributedCache>();
        _cacheService = new RedisCacheService(_mockDistributedCache.Object);
    }

    [Test]
    public async Task GetAsync_ValidKey_ReturnsDeserializedObject()
    {
        // Arrange
        var key = "test_key";
        var testObject = new { Id = Guid.NewGuid(), Name = "Test", Value = 42 };
        var serializedValue = JsonSerializer.Serialize(testObject);

        _mockDistributedCache
            .Setup(x => x.GetAsync(key, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Encoding.UTF8.GetBytes(serializedValue));

        // Act
        var result = await _cacheService.GetAsync<object>(key);

        // Assert
        result.Should().NotBeNull();
        _mockDistributedCache.Verify(x => x.GetAsync(key, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetAsync_KeyNotFound_ReturnsNull()
    {
        // Arrange
        var key = "nonexistent_key";

        _mockDistributedCache
            .Setup(x => x.GetAsync(key, It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        // Act
        var result = await _cacheService.GetAsync<object>(key);

        // Assert
        result.Should().BeNull();
        _mockDistributedCache.Verify(x => x.GetAsync(key, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetAsync_EmptyString_ReturnsNull()
    {
        // Arrange
        var key = "empty_key";

        _mockDistributedCache
            .Setup(x => x.GetAsync(key, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<byte>());

        // Act
        var result = await _cacheService.GetAsync<object>(key);

        // Assert
        result.Should().BeNull();
        _mockDistributedCache.Verify(x => x.GetAsync(key, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task SetAsync_ValidData_SetsCacheWithCorrectOptions()
    {
        // Arrange
        var key = "test_key";
        var testObject = new { Id = Guid.NewGuid(), Name = "Test", Value = 42 };
        var expiration = TimeSpan.FromMinutes(15);

        _mockDistributedCache
            .Setup(x => x.SetAsync(key, It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _cacheService.SetAsync(key, testObject, expiration);

        // Assert
        _mockDistributedCache.Verify(x => x.SetAsync(
            key, 
            It.IsAny<byte[]>(), 
            It.Is<DistributedCacheEntryOptions>(options => options.AbsoluteExpirationRelativeToNow == expiration),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task RemoveAsync_ValidKey_RemovesFromCache()
    {
        // Arrange
        var key = "test_key";

        _mockDistributedCache
            .Setup(x => x.RemoveAsync(key, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _cacheService.RemoveAsync(key);

        // Assert
        _mockDistributedCache.Verify(x => x.RemoveAsync(key, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ExistsAsync_KeyExists_ReturnsTrue()
    {
        // Arrange
        var key = "existing_key";
        var value = "some_value";

        _mockDistributedCache
            .Setup(x => x.GetAsync(key, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Encoding.UTF8.GetBytes(value));

        // Act
        var result = await _cacheService.ExistsAsync(key);

        // Assert
        result.Should().BeTrue();
        _mockDistributedCache.Verify(x => x.GetAsync(key, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ExistsAsync_KeyDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var key = "nonexistent_key";

        _mockDistributedCache
            .Setup(x => x.GetAsync(key, It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        // Act
        var result = await _cacheService.ExistsAsync(key);

        // Assert
        result.Should().BeFalse();
        _mockDistributedCache.Verify(x => x.GetAsync(key, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetAsync_RedisThrowsException_ReturnsNull()
    {
        // Arrange
        var key = "error_key";

        _mockDistributedCache
            .Setup(x => x.GetAsync(key, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Redis connection failed"));

        // Act
        var result = await _cacheService.GetAsync<object>(key);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task SetAsync_RedisThrowsException_DoesNotThrow()
    {
        // Arrange
        var key = "error_key";
        var testObject = new { Id = Guid.NewGuid(), Name = "Test" };
        var expiration = TimeSpan.FromMinutes(15);

        _mockDistributedCache
            .Setup(x => x.SetAsync(key, It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Redis connection failed"));

        // Act & Assert
        await _cacheService.Invoking(async x => await x.SetAsync(key, testObject, expiration))
            .Should().NotThrowAsync();
    }

    [Test]
    public async Task RemoveAsync_RedisThrowsException_DoesNotThrow()
    {
        // Arrange
        var key = "error_key";

        _mockDistributedCache
            .Setup(x => x.RemoveAsync(key, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Redis connection failed"));

        // Act & Assert
        await _cacheService.Invoking(async x => await x.RemoveAsync(key))
            .Should().NotThrowAsync();
    }

    [Test]
    public async Task ExistsAsync_RedisThrowsException_ReturnsFalse()
    {
        // Arrange
        var key = "error_key";

        _mockDistributedCache
            .Setup(x => x.GetAsync(key, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Redis connection failed"));

        // Act
        var result = await _cacheService.ExistsAsync(key);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public async Task GetAsync_InvalidJson_ReturnsNull()
    {
        // Arrange
        var key = "invalid_json_key";
        var invalidJson = "{ invalid json }";

        _mockDistributedCache
            .Setup(x => x.GetAsync(key, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Encoding.UTF8.GetBytes(invalidJson));

        // Act
        var result = await _cacheService.GetAsync<object>(key);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task SetAsync_SerializesCorrectly()
    {
        // Arrange
        var key = "serialization_test";
        var testObject = new { Id = Guid.NewGuid(), Name = "Test", Value = 42 };
        var expiration = TimeSpan.FromMinutes(15);

        byte[]? capturedValue = null;
        _mockDistributedCache
            .Setup(x => x.SetAsync(key, It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()))
            .Callback<string, byte[], DistributedCacheEntryOptions, CancellationToken>((k, v, o, c) => capturedValue = v)
            .Returns(Task.CompletedTask);

        // Act
        await _cacheService.SetAsync(key, testObject, expiration);

        // Assert
        capturedValue.Should().NotBeNull();
        var deserialized = JsonSerializer.Deserialize<object>(Encoding.UTF8.GetString(capturedValue!));
        deserialized.Should().NotBeNull();
    }
}