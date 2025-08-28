using NUnit.Framework;
using FluentAssertions;

namespace EduShield.Api.Tests.Unit;

[TestFixture]
public class SimpleTest
{
    [Test]
    public void SimpleTest_ShouldPass()
    {
        // Arrange
        var expected = 2;
        
        // Act
        var actual = 1 + 1;
        
        // Assert
        actual.Should().Be(expected);
    }
}

