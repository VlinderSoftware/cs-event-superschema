# C# Event SuperSchema Library

A .NET library for event formatting, validation, and dispatching using a common superschema. This library provides a standardized way to structure, validate, and route events in distributed systems.

## Features

- **Schema Validation**: Validate events against a JSON schema using Newtonsoft.Json.Schema
- **Event Dispatching**: Route events to appropriate handlers based on event type
- **Event Formatting**: Format events with automatic metadata management
- **Type-based Routing**: Support for exact match, base type matching, and default handlers
- **JWE/JWS Support**: Placeholder implementations for encrypted and signed events
- **Comprehensive Testing**: Full test suite using xUnit and FluentAssertions

## Installation

### NuGet Packages

The library depends on:
- `Newtonsoft.Json` (13.0.3+)
- `Newtonsoft.Json.Schema` (3.0.15+)

### Requirements

- .NET 8.0 or later

### Building from Source

```bash
# Clone and navigate to the directory
cd cs-event-superschema

# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run tests
dotnet test
```

## Event SuperSchema

All events must conform to the following JSON schema:

```json
{
  "type": "object",
  "properties": {
    "id": { "type": "string", "format": "uuid" },
    "type": { "type": "string" },
    "metadata": {
      "type": "object",
      "properties": {
        "cid": { "type": "string", "format": "uuid" },
        "tid": { "type": "string", "format": "uuid" },
        "pid": { "type": "string", "format": "uuid" },
        "uid": { "type": "string", "format": "uuid" },
        "token": { "type": "string" }
      },
      "required": [ "cid", "pid" ]
    },
    "data": { "type": "object" }
  },
  "required": [ "id", "type", "metadata" ]
}
```

### Field Descriptions

- **id**: Unique event identifier (UUID)
- **type**: Event type identifier (string)
- **metadata**: Event metadata object
  - **cid**: Correlation ID (UUID, required)
  - **tid**: Transaction ID (UUID)
  - **pid**: Producer ID (UUID, required)
  - **uid**: User ID (UUID, optional)
  - **token**: Authentication token (string, optional)
- **data**: Event payload (object, optional)

## Usage

### 1. Schema Validation

```csharp
using EventSuperSchema;
using Newtonsoft.Json.Linq;

// Create an event
var myEvent = new JObject
{
    ["id"] = Guid.NewGuid().ToString(),
    ["type"] = "UserCreated",
    ["metadata"] = new JObject
    {
        ["cid"] = Guid.NewGuid().ToString(),
        ["pid"] = Guid.NewGuid().ToString()
    }
};

// Validate the event
bool isValid = SuperSchema.IsValid(myEvent);

// Get validation errors
bool isValidWithErrors = SuperSchema.IsValid(myEvent, out IList<string> errors);
if (!isValidWithErrors)
{
    foreach (var error in errors)
    {
        Console.WriteLine(error);
    }
}
```

### 2. Event Dispatching

The event dispatcher validates incoming events and routes them to the appropriate handler based on the event type.

```csharp
using EventSuperSchema;
using Newtonsoft.Json.Linq;

// Define error handler
void HandleError(ErrorMessage error)
{
    Console.WriteLine($"Error: {error.Error} - {error.Message}");
}

// Define event handlers
var handlers = new Dictionary<string, EventHandler>
{
    ["UserCreated"] = (err, evt) =>
    {
        Console.WriteLine($"User created: {evt["data"]?["username"]}");
    },
    ["OrderPlaced:v1"] = (err, evt) =>
    {
        Console.WriteLine($"Order placed: {evt["data"]?["orderId"]}");
    },
    ["__default__"] = (err, evt) =>
    {
        Console.WriteLine($"Unhandled event type: {evt["type"]}");
    }
};

// Create dispatcher
var dispatcher = EventDispatcher.GetEventDispatcher(HandleError, handlers);

// Dispatch events
dispatcher(myEvent);
```

#### Event Type Matching Rules

The dispatcher uses the following priority for matching handlers:

1. **Exact Match**: `"UserCreated:v1"` matches handler `"UserCreated:v1"`
2. **Base Type Match**: `"UserCreated:v1"` matches handler `"UserCreated"` (everything before last `:`)
3. **Default Handler**: Falls back to `"__default__"` if no match found

### 3. Event Formatting and Sending

The send event function handles event formatting and metadata management automatically.

```csharp
using EventSuperSchema;
using Newtonsoft.Json.Linq;

// Define send function (e.g., to event bus)
void SendToEventBus(JObject evt)
{
    // Send to Kafka, RabbitMQ, Azure Service Bus, etc.
    Console.WriteLine($"Sending event: {evt}");
}

// Create producer ID
var producerId = Guid.NewGuid().ToString();

// Optional: Define data preprocessors
var preprocessors = new Dictionary<string, DataPreprocessor>
{
    ["UserCreated"] = (data) =>
    {
        // Transform data to match expected schema
        var user = (dynamic)data;
        return new
        {
            username = user.Username,
            email = user.Email,
            createdAt = DateTime.UtcNow
        };
    },
    ["__default__"] = (data) => data  // Pass through by default
};

// Create send function
var sendEvent = SendEventFunction.GetSendEventFunction(
    SendToEventBus,
    producerId,
    preprocessors
);

// Send events
sendEvent("UserCreated", new { Username = "john_doe", Email = "john@example.com" });

// With optional metadata
var correlationId = Guid.NewGuid().ToString();
var userId = Guid.NewGuid().ToString();
sendEvent("OrderPlaced", 
    new { OrderId = "ORD-123", Amount = 99.99 },
    correlationId,
    userId,
    "auth-token-123"
);
```

### 4. Complete Example

```csharp
using EventSuperSchema;
using Newtonsoft.Json.Linq;

class Program
{
    static void Main()
    {
        // Setup
        var producerId = Guid.NewGuid().ToString();
        var events = new List<JObject>();

        // Create send function that collects events
        void SendToEventBus(JObject evt) => events.Add(evt);

        // Create event sender
        var sendEvent = SendEventFunction.GetSendEventFunction(
            SendToEventBus,
            producerId
        );

        // Create event dispatcher
        void HandleError(ErrorMessage err)
        {
            Console.WriteLine($"[ERROR] {err.Error}: {err.Message}");
        }

        var handlers = new Dictionary<string, EventHandler>
        {
            ["UserCreated"] = (err, evt) =>
            {
                var username = evt["data"]?["username"]?.ToString();
                Console.WriteLine($"Processing user creation: {username}");
            }
        };

        var dispatcher = EventDispatcher.GetEventDispatcher(HandleError, handlers);

        // Send an event
        sendEvent("UserCreated", new { username = "alice", email = "alice@example.com" });

        // Process the event
        foreach (var evt in events)
        {
            dispatcher(evt);
        }
    }
}
```

## JWE and JWS Support

The library includes placeholder implementations for encrypted (JWE) and signed (JWS) events.

### JWE Event Dispatcher

```csharp
// Implement your decryption function
JObject DecryptJwe(string jweToken)
{
    // Use Microsoft.IdentityModel.JsonWebTokens or similar
    // This is a placeholder
    throw new NotImplementedException("Implement JWE decryption");
}

var jweDispatcher = JweEventDispatcher.GetJweEventDispatcher(
    HandleError,
    handlers,
    DecryptJwe
);

// Dispatch encrypted event
jweDispatcher("eyJhbGc...encrypted-jwe-token");
```

### JWS Event Dispatcher

```csharp
// Implement your verification function
JObject VerifyJws(string jwsToken)
{
    // Use Microsoft.IdentityModel.JsonWebTokens or similar
    // This is a placeholder
    throw new NotImplementedException("Implement JWS verification");
}

var jwsDispatcher = JwsEventDispatcher.GetJwsEventDispatcher(
    HandleError,
    handlers,
    VerifyJws
);

// Dispatch signed event
jwsDispatcher("eyJhbGc...signed-jws-token");
```

### JWE/JWS Send Functions

```csharp
// Encryption function
string EncryptEvent(JObject evt)
{
    // Implement encryption
    throw new NotImplementedException("Implement JWE encryption");
}

var jweSendEvent = JweSendEventFunction.GetJweSendEventFunction(
    (encryptedToken) => Console.WriteLine(encryptedToken),
    producerId,
    EncryptEvent
);

// Signing function
string SignEvent(JObject evt)
{
    // Implement signing
    throw new NotImplementedException("Implement JWS signing");
}

var jwsSendEvent = JwsSendEventFunction.GetJwsSendEventFunction(
    (signedToken) => Console.WriteLine(signedToken),
    producerId,
    SignEvent
);
```

## API Reference

### SuperSchema

- `static JSchema Schema` - The JSON schema for events
- `static bool IsValid(JObject eventData)` - Validate an event
- `static bool IsValid(JObject eventData, out IList<string> errors)` - Validate with error details

### EventDispatcher

- `static Action<JObject> GetEventDispatcher(ErrorHandler err, Dictionary<string, EventHandler> handlers)` - Create an event dispatcher

### SendEventFunction

- `static Action<string, object?, string?, string?, string?> GetSendEventFunction(SendFunction send, string pid, Dictionary<string, DataPreprocessor>? dataPreprocessors = null)` - Create a send event function

### Types

- `ErrorMessage` - Error message with Error and Message properties
- `ErrorHandler` - Delegate: `void (ErrorMessage error)`
- `EventHandler` - Delegate: `void (ErrorHandler err, JObject eventData)`
- `DataPreprocessor` - Delegate: `object (object data)`
- `SendFunction` - Delegate: `void (JObject eventData)`

## Testing

Run the test suite:

```bash
dotnet test
```

Run with coverage:

```bash
dotnet test /p:CollectCoverage=true
```

Run specific test:

```bash
dotnet test --filter "FullyQualifiedName~SuperSchemaTests"
```

## Project Structure

```
cs-event-superschema/
├── EventSuperSchema.sln                     # Solution file
├── src/
│   └── EventSuperSchema/
│       ├── EventSuperSchema.csproj          # Main project
│       ├── Types.cs                         # Common types and delegates
│       ├── SuperSchema.cs                   # Schema definition and validation
│       ├── EventDispatcher.cs               # Event dispatcher factory
│       ├── SendEventFunction.cs             # Send event function factory
│       ├── JweEventDispatcher.cs            # JWE encrypted dispatcher
│       ├── JwsEventDispatcher.cs            # JWS signed dispatcher
│       ├── JweSendEventFunction.cs          # JWE encrypted sender
│       └── JwsSendEventFunction.cs          # JWS signed sender
├── tests/
│   └── EventSuperSchema.Tests/
│       ├── EventSuperSchema.Tests.csproj    # Test project
│       ├── SuperSchemaTests.cs              # Schema tests
│       ├── EventDispatcherTests.cs          # Dispatcher tests
│       └── SendEventFunctionTests.cs        # Send function tests
├── .gitignore                               # Git ignore file
└── README.md                                # This file
```

## Contributing

Contributions are welcome! Please ensure:

1. All tests pass
2. Code follows C# naming conventions
3. XML documentation is provided for public APIs
4. New features include corresponding tests

## License

See LICENSE file for details.

## Related Implementations

- Python: `/workspace/python-event-superschema/`
- Node.js/TypeScript: `/workspace/node-event-superschema/`
- C++: `/workspace/cpp-event-superschema/`
