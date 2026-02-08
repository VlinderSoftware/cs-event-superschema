using Xunit;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using EventSuperSchema;

namespace EventSuperSchema.Tests;

public class SendEventFunctionTests
{
    [Fact]
    public void GetSendEventFunction_ShouldReturnCallableFunction()
    {
        // Arrange
        SendFunction send = (evt) => { };
        var pid = Guid.NewGuid().ToString();

        // Act
        var sendEvent = SendEventFunction.GetSendEventFunction(send, pid);

        // Assert
        sendEvent.Should().NotBeNull();
    }

    [Fact]
    public void SendEvent_WithMinimalParameters_ShouldFormatCorrectly()
    {
        // Arrange
        JObject? capturedEvent = null;
        SendFunction send = (evt) => capturedEvent = evt;
        var pid = Guid.NewGuid().ToString();
        var sendEvent = SendEventFunction.GetSendEventFunction(send, pid);

        // Act
        sendEvent("TestEvent", null, null, null, null);

        // Assert
        capturedEvent.Should().NotBeNull();
        capturedEvent!["id"].Should().NotBeNull();
        capturedEvent["type"]?.ToString().Should().Be("TestEvent");
        capturedEvent["metadata"]?.Should().NotBeNull();
        capturedEvent["metadata"]!["pid"]?.ToString().Should().Be(pid);
        capturedEvent["metadata"]!["cid"].Should().NotBeNull();
        capturedEvent["metadata"]!["tid"].Should().NotBeNull();
    }

    [Fact]
    public void SendEvent_WithEventData_ShouldIncludeDataField()
    {
        // Arrange
        JObject? capturedEvent = null;
        SendFunction send = (evt) => capturedEvent = evt;
        var pid = Guid.NewGuid().ToString();
        var sendEvent = SendEventFunction.GetSendEventFunction(send, pid);

        var eventData = new { field1 = "value1", field2 = 42 };

        // Act
        sendEvent("TestEvent", eventData, null, null, null);

        // Assert
        capturedEvent.Should().NotBeNull();
        capturedEvent!["data"].Should().NotBeNull();
        capturedEvent["data"]!["field1"]?.ToString().Should().Be("value1");
        capturedEvent["data"]!["field2"]?.Value<int>().Should().Be(42);
    }

    [Fact]
    public void SendEvent_WithCid_ShouldUseProviledCid()
    {
        // Arrange
        JObject? capturedEvent = null;
        SendFunction send = (evt) => capturedEvent = evt;
        var pid = Guid.NewGuid().ToString();
        var cid = Guid.NewGuid().ToString();
        var sendEvent = SendEventFunction.GetSendEventFunction(send, pid);

        // Act
        sendEvent("TestEvent", null, cid, null, null);

        // Assert
        capturedEvent.Should().NotBeNull();
        capturedEvent!["metadata"]!["cid"]?.ToString().Should().Be(cid);
    }

    [Fact]
    public void SendEvent_WithUid_ShouldIncludeUid()
    {
        // Arrange
        JObject? capturedEvent = null;
        SendFunction send = (evt) => capturedEvent = evt;
        var pid = Guid.NewGuid().ToString();
        var uid = Guid.NewGuid().ToString();
        var sendEvent = SendEventFunction.GetSendEventFunction(send, pid);

        // Act
        sendEvent("TestEvent", null, null, uid, null);

        // Assert
        capturedEvent.Should().NotBeNull();
        capturedEvent!["metadata"]!["uid"]?.ToString().Should().Be(uid);
    }

    [Fact]
    public void SendEvent_WithToken_ShouldIncludeToken()
    {
        // Arrange
        JObject? capturedEvent = null;
        SendFunction send = (evt) => capturedEvent = evt;
        var pid = Guid.NewGuid().ToString();
        var token = "test-token-123";
        var sendEvent = SendEventFunction.GetSendEventFunction(send, pid);

        // Act
        sendEvent("TestEvent", null, null, null, token);

        // Assert
        capturedEvent.Should().NotBeNull();
        capturedEvent!["metadata"]!["token"]?.ToString().Should().Be(token);
    }

    [Fact]
    public void SendEvent_WithoutUid_ShouldNotIncludeUidField()
    {
        // Arrange
        JObject? capturedEvent = null;
        SendFunction send = (evt) => capturedEvent = evt;
        var pid = Guid.NewGuid().ToString();
        var sendEvent = SendEventFunction.GetSendEventFunction(send, pid);

        // Act
        sendEvent("TestEvent", null, null, null, null);

        // Assert
        capturedEvent.Should().NotBeNull();
        capturedEvent!["metadata"]!["uid"].Should().BeNull();
    }

    [Fact]
    public void SendEvent_WithoutToken_ShouldNotIncludeTokenField()
    {
        // Arrange
        JObject? capturedEvent = null;
        SendFunction send = (evt) => capturedEvent = evt;
        var pid = Guid.NewGuid().ToString();
        var sendEvent = SendEventFunction.GetSendEventFunction(send, pid);

        // Act
        sendEvent("TestEvent", null, null, null, null);

        // Assert
        capturedEvent.Should().NotBeNull();
        capturedEvent!["metadata"]!["token"].Should().BeNull();
    }

    [Fact]
    public void SendEvent_WithDataPreprocessor_ShouldApplyPreprocessor()
    {
        // Arrange
        JObject? capturedEvent = null;
        SendFunction send = (evt) => capturedEvent = evt;
        var pid = Guid.NewGuid().ToString();
        
        var preprocessors = new Dictionary<string, DataPreprocessor>
        {
            ["TestEvent"] = (data) => new { processed = true, original = data }
        };

        var sendEvent = SendEventFunction.GetSendEventFunction(send, pid, preprocessors);

        // Act
        sendEvent("TestEvent", "raw-data", null, null, null);

        // Assert
        capturedEvent.Should().NotBeNull();
        capturedEvent!["data"].Should().NotBeNull();
        capturedEvent["data"]!["processed"]?.Value<bool>().Should().BeTrue();
        capturedEvent["data"]!["original"]?.ToString().Should().Be("raw-data");
    }

    [Fact]
    public void SendEvent_WithDefaultPreprocessor_ShouldApplyDefaultWhenNoMatch()
    {
        // Arrange
        JObject? capturedEvent = null;
        SendFunction send = (evt) => capturedEvent = evt;
        var pid = Guid.NewGuid().ToString();
        
        var preprocessors = new Dictionary<string, DataPreprocessor>
        {
            ["__default__"] = (data) => new { default_processed = true, value = data }
        };

        var sendEvent = SendEventFunction.GetSendEventFunction(send, pid, preprocessors);

        // Act
        sendEvent("UnknownEvent", "test-data", null, null, null);

        // Assert
        capturedEvent.Should().NotBeNull();
        capturedEvent!["data"].Should().NotBeNull();
        capturedEvent["data"]!["default_processed"]?.Value<bool>().Should().BeTrue();
    }

    [Fact]
    public void SendEvent_GeneratesValidUuids()
    {
        // Arrange
        JObject? capturedEvent = null;
        SendFunction send = (evt) => capturedEvent = evt;
        var pid = Guid.NewGuid().ToString();
        var sendEvent = SendEventFunction.GetSendEventFunction(send, pid);

        // Act
        sendEvent("TestEvent", null, null, null, null);

        // Assert
        capturedEvent.Should().NotBeNull();
        
        var id = capturedEvent!["id"]?.ToString();
        var cid = capturedEvent["metadata"]!["cid"]?.ToString();
        var tid = capturedEvent["metadata"]!["tid"]?.ToString();

        id.Should().NotBeNullOrEmpty();
        cid.Should().NotBeNullOrEmpty();
        tid.Should().NotBeNullOrEmpty();

        // Validate they're proper GUIDs
        Guid.TryParse(id, out _).Should().BeTrue();
        Guid.TryParse(cid, out _).Should().BeTrue();
        Guid.TryParse(tid, out _).Should().BeTrue();
        Guid.TryParse(pid, out _).Should().BeTrue();
    }

    [Fact]
    public void SendEvent_CidAndTidDefaultToEventId()
    {
        // Arrange
        JObject? capturedEvent = null;
        SendFunction send = (evt) => capturedEvent = evt;
        var pid = Guid.NewGuid().ToString();
        var sendEvent = SendEventFunction.GetSendEventFunction(send, pid);

        // Act
        sendEvent("TestEvent", null, null, null, null);

        // Assert
        capturedEvent.Should().NotBeNull();
        var id = capturedEvent!["id"]?.ToString();
        var cid = capturedEvent["metadata"]!["cid"]?.ToString();
        var tid = capturedEvent["metadata"]!["tid"]?.ToString();

        cid.Should().Be(id);
        tid.Should().Be(id);
    }

    [Fact]
    public void SendEvent_GeneratesSchemaValidEvent()
    {
        // Arrange
        JObject? capturedEvent = null;
        SendFunction send = (evt) => capturedEvent = evt;
        var pid = Guid.NewGuid().ToString();
        var sendEvent = SendEventFunction.GetSendEventFunction(send, pid);

        var eventData = new { test = "data" };

        // Act
        sendEvent("TestEvent", eventData, null, null, null);

        // Assert
        capturedEvent.Should().NotBeNull();
        SuperSchema.IsValid(capturedEvent!).Should().BeTrue();
    }

    [Fact]
    public void SendEvent_WithAllParameters_ShouldIncludeAllFields()
    {
        // Arrange
        JObject? capturedEvent = null;
        SendFunction send = (evt) => capturedEvent = evt;
        var pid = Guid.NewGuid().ToString();
        var cid = Guid.NewGuid().ToString();
        var uid = Guid.NewGuid().ToString();
        var token = "auth-token";
        var sendEvent = SendEventFunction.GetSendEventFunction(send, pid);

        var eventData = new { field = "value" };

        // Act
        sendEvent("TestEvent", eventData, cid, uid, token);

        // Assert
        capturedEvent.Should().NotBeNull();
        capturedEvent!["type"]?.ToString().Should().Be("TestEvent");
        capturedEvent["metadata"]!["cid"]?.ToString().Should().Be(cid);
        capturedEvent["metadata"]!["pid"]?.ToString().Should().Be(pid);
        capturedEvent["metadata"]!["uid"]?.ToString().Should().Be(uid);
        capturedEvent["metadata"]!["token"]?.ToString().Should().Be(token);
        capturedEvent["data"].Should().NotBeNull();
    }
}

