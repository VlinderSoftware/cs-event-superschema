using Newtonsoft.Json.Linq;

namespace EventSuperSchema;

/// <summary>
/// Represents an error message with an error code and description
/// </summary>
public class ErrorMessage
{
    /// <summary>
    /// Gets or sets the error code
    /// </summary>
    public string Error { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the error message description
    /// </summary>
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Delegate for handling errors
/// </summary>
public delegate void ErrorHandler(ErrorMessage error);

/// <summary>
/// Delegate for handling events
/// </summary>
public delegate void EventHandler(ErrorHandler err, JObject eventData);

/// <summary>
/// Delegate for preprocessing event data
/// </summary>
public delegate object DataPreprocessor(object data);

/// <summary>
/// Delegate for sending formatted events
/// </summary>
public delegate void SendFunction(JObject eventData);
