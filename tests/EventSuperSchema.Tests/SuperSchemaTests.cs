using Xunit;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using EventSuperSchema;

namespace EventSuperSchema.Tests;

public class SuperSchemaTests
{
    [Fact]
    public void SuperSchema_ShouldHaveValidSchema()
    {
        // Arrange & Act
        var schema = SuperSchema.Schema;

        // Assert
        schema.Should().NotBeNull();
        schema.Type.Should().NotBeNull();
    }

    [Fact]
    public void IsValid_WithValidEvent_ShouldReturnTrue()
    {
        // Arrange
        var validEvent = new JObject
        {
            ["id"] = Guid.NewGuid().ToString(),
            ["type"] = "TestEvent",
            ["metadata"] = new JObject
            {
                ["cid"] = Guid.NewGuid().ToString(),
                ["pid"] = Guid.NewGuid().ToString()
            }
        };

        // Act
        var isValid = SuperSchema.IsValid(validEvent);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void IsValid_WithMissingId_ShouldReturnFalse()
    {
        // Arrange
        var invalidEvent = new JObject
        {
            ["type"] = "TestEvent",
            ["metadata"] = new JObject
            {
                ["cid"] = Guid.NewGuid().ToString(),
                ["pid"] = Guid.NewGuid().ToString()
            }
        };

        // Act
        var isValid = SuperSchema.IsValid(invalidEvent);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithMissingType_ShouldReturnFalse()
    {
        // Arrange
        var invalidEvent = new JObject
        {
            ["id"] = Guid.NewGuid().ToString(),
            ["metadata"] = new JObject
            {
                ["cid"] = Guid.NewGuid().ToString(),
                ["pid"] = Guid.NewGuid().ToString()
            }
        };

        // Act
        var isValid = SuperSchema.IsValid(invalidEvent);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithMissingMetadata_ShouldReturnFalse()
    {
        // Arrange
        var invalidEvent = new JObject
        {
            ["id"] = Guid.NewGuid().ToString(),
            ["type"] = "TestEvent"
        };

        // Act
        var isValid = SuperSchema.IsValid(invalidEvent);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithMissingCid_ShouldReturnFalse()
    {
        // Arrange
        var invalidEvent = new JObject
        {
            ["id"] = Guid.NewGuid().ToString(),
            ["type"] = "TestEvent",
            ["metadata"] = new JObject
            {
                ["pid"] = Guid.NewGuid().ToString()
            }
        };

        // Act
        var isValid = SuperSchema.IsValid(invalidEvent);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithMissingPid_ShouldReturnFalse()
    {
        // Arrange
        var invalidEvent = new JObject
        {
            ["id"] = Guid.NewGuid().ToString(),
            ["type"] = "TestEvent",
            ["metadata"] = new JObject
            {
                ["cid"] = Guid.NewGuid().ToString()
            }
        };

        // Act
        var isValid = SuperSchema.IsValid(invalidEvent);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithOptionalFields_ShouldReturnTrue()
    {
        // Arrange
        var validEvent = new JObject
        {
            ["id"] = Guid.NewGuid().ToString(),
            ["type"] = "TestEvent",
            ["metadata"] = new JObject
            {
                ["cid"] = Guid.NewGuid().ToString(),
                ["tid"] = Guid.NewGuid().ToString(),
                ["pid"] = Guid.NewGuid().ToString(),
                ["uid"] = Guid.NewGuid().ToString(),
                ["token"] = "test-token"
            },
            ["data"] = new JObject
            {
                ["field1"] = "value1",
                ["field2"] = 42
            }
        };

        // Act
        var isValid = SuperSchema.IsValid(validEvent);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void IsValid_WithErrors_ShouldReturnErrorMessages()
    {
        // Arrange
        var invalidEvent = new JObject
        {
            ["type"] = "TestEvent"
        };

        // Act
        var isValid = SuperSchema.IsValid(invalidEvent, out var errors);

        // Assert
        isValid.Should().BeFalse();
        errors.Should().NotBeEmpty();
    }
}
