using Xunit;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using EventSuperSchema;

namespace EventSuperSchema.Tests;

public class EventDispatcherTests
{
    [Fact]
    public void GetEventDispatcher_ShouldReturnCallableFunction()
    {
        // Arrange
        ErrorHandler err = (error) => { };
        var handlers = new Dictionary<string, EventHandler>();

        // Act
        var dispatcher = EventDispatcher.GetEventDispatcher(err, handlers);

        // Assert
        dispatcher.Should().NotBeNull();
    }

    [Fact]
    public void Dispatcher_WithInvalidEvent_ShouldCallErrorHandler()
    {
        // Arrange
        ErrorMessage? capturedError = null;
        ErrorHandler err = (error) => capturedError = error;
        var handlers = new Dictionary<string, EventHandler>();
        var dispatcher = EventDispatcher.GetEventDispatcher(err, handlers);
        var invalidEvent = new JObject();

        // Act
        dispatcher(invalidEvent);

        // Assert
        capturedError.Should().NotBeNull();
        capturedError!.Error.Should().Be("SchemaMismatchError");
        capturedError.Message.Should().Contain("does not match event schema");
    }

    [Fact]
    public void Dispatcher_WithValidEvent_ShouldNotCallErrorHandler()
    {
        // Arrange
        var errorCalled = false;
        ErrorHandler err = (error) => errorCalled = true;
        var handlers = new Dictionary<string, EventHandler>();
        var dispatcher = EventDispatcher.GetEventDispatcher(err, handlers);
        
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
        dispatcher(validEvent);

        // Assert
        errorCalled.Should().BeFalse();
    }

    [Fact]
    public void Dispatcher_WithExactMatch_ShouldCallSpecificHandler()
    {
        // Arrange
        var handlerCalled = false;
        JObject? capturedEvent = null;

        ErrorHandler err = (error) => { };
        var handlers = new Dictionary<string, EventHandler>
        {
            ["TestEvent:v1"] = (e, evt) =>
            {
                handlerCalled = true;
                capturedEvent = evt;
            }
        };

        var dispatcher = EventDispatcher.GetEventDispatcher(err, handlers);
        var validEvent = new JObject
        {
            ["id"] = Guid.NewGuid().ToString(),
            ["type"] = "TestEvent:v1",
            ["metadata"] = new JObject
            {
                ["cid"] = Guid.NewGuid().ToString(),
                ["pid"] = Guid.NewGuid().ToString()
            }
        };

        // Act
        dispatcher(validEvent);

        // Assert
        handlerCalled.Should().BeTrue();
        capturedEvent.Should().NotBeNull();
        capturedEvent!["type"]?.ToString().Should().Be("TestEvent:v1");
    }

    [Fact]
    public void Dispatcher_WithBaseTypeMatch_ShouldCallBaseHandler()
    {
        // Arrange
        var handlerCalled = false;
        JObject? capturedEvent = null;

        ErrorHandler err = (error) => { };
        var handlers = new Dictionary<string, EventHandler>
        {
            ["TestEvent"] = (e, evt) =>
            {
                handlerCalled = true;
                capturedEvent = evt;
            }
        };

        var dispatcher = EventDispatcher.GetEventDispatcher(err, handlers);
        var validEvent = new JObject
        {
            ["id"] = Guid.NewGuid().ToString(),
            ["type"] = "TestEvent:v1",
            ["metadata"] = new JObject
            {
                ["cid"] = Guid.NewGuid().ToString(),
                ["pid"] = Guid.NewGuid().ToString()
            }
        };

        // Act
        dispatcher(validEvent);

        // Assert
        handlerCalled.Should().BeTrue();
        capturedEvent.Should().NotBeNull();
    }

    [Fact]
    public void Dispatcher_WithDefaultHandler_ShouldCallDefaultWhenNoMatch()
    {
        // Arrange
        var defaultHandlerCalled = false;
        JObject? capturedEvent = null;

        ErrorHandler err = (error) => { };
        var handlers = new Dictionary<string, EventHandler>
        {
            ["__default__"] = (e, evt) =>
            {
                defaultHandlerCalled = true;
                capturedEvent = evt;
            }
        };

        var dispatcher = EventDispatcher.GetEventDispatcher(err, handlers);
        var validEvent = new JObject
        {
            ["id"] = Guid.NewGuid().ToString(),
            ["type"] = "UnhandledEvent",
            ["metadata"] = new JObject
            {
                ["cid"] = Guid.NewGuid().ToString(),
                ["pid"] = Guid.NewGuid().ToString()
            }
        };

        // Act
        dispatcher(validEvent);

        // Assert
        defaultHandlerCalled.Should().BeTrue();
        capturedEvent.Should().NotBeNull();
    }

    [Fact]
    public void Dispatcher_ExactMatchTakesPrecedenceOverBaseMatch()
    {
        // Arrange
        var exactHandlerCalled = false;
        var baseHandlerCalled = false;

        ErrorHandler err = (error) => { };
        var handlers = new Dictionary<string, EventHandler>
        {
            ["TestEvent"] = (e, evt) => baseHandlerCalled = true,
            ["TestEvent:v1"] = (e, evt) => exactHandlerCalled = true
        };

        var dispatcher = EventDispatcher.GetEventDispatcher(err, handlers);
        var validEvent = new JObject
        {
            ["id"] = Guid.NewGuid().ToString(),
            ["type"] = "TestEvent:v1",
            ["metadata"] = new JObject
            {
                ["cid"] = Guid.NewGuid().ToString(),
                ["pid"] = Guid.NewGuid().ToString()
            }
        };

        // Act
        dispatcher(validEvent);

        // Assert
        exactHandlerCalled.Should().BeTrue();
        baseHandlerCalled.Should().BeFalse();
    }

    [Fact]
    public void Dispatcher_WithNoMatchingHandler_ShouldNotThrow()
    {
        // Arrange
        ErrorHandler err = (error) => { };
        var handlers = new Dictionary<string, EventHandler>
        {
            ["DifferentEvent"] = (e, evt) => { }
        };

        var dispatcher = EventDispatcher.GetEventDispatcher(err, handlers);
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
        Action act = () => dispatcher(validEvent);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Dispatcher_HandlerReceivesErrorHandler()
    {
        // Arrange
        ErrorHandler? capturedErrorHandler = null;
        ErrorHandler err = (error) => { };
        
        var handlers = new Dictionary<string, EventHandler>
        {
            ["TestEvent"] = (e, evt) => capturedErrorHandler = e
        };

        var dispatcher = EventDispatcher.GetEventDispatcher(err, handlers);
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
        dispatcher(validEvent);

        // Assert
        capturedErrorHandler.Should().NotBeNull();
        capturedErrorHandler.Should().Be(err);
    }
}

