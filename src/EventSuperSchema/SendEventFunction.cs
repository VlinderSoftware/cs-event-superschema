using Newtonsoft.Json.Linq;

namespace EventSuperSchema;

/// <summary>
/// Factory for creating send event functions
/// </summary>
public static class SendEventFunction
{
    /// <summary>
    /// Get a function to send properly formatted events
    /// </summary>
    /// <param name="send">Generic function to send events on the event bus</param>
    /// <param name="pid">Producer ID (UUID)</param>
    /// <param name="dataPreprocessors">Optional dictionary mapping event types to data preprocessors.
    /// Preprocessors convert event data to serializable objects. Include '__default__' for fallback</param>
    /// <returns>A function to send events with automatic formatting</returns>
    public static Action<string, object?, string?, string?, string?> GetSendEventFunction(
        SendFunction send,
        string pid,
        Dictionary<string, DataPreprocessor>? dataPreprocessors = null)
    {
        var preprocessors = dataPreprocessors ?? new Dictionary<string, DataPreprocessor>();
        
        // Ensure default preprocessor exists
        if (!preprocessors.ContainsKey("__default__"))
        {
            preprocessors["__default__"] = data => data;
        }

        var formatEvent = GetFormatEventFunction(preprocessors, pid);

        return (string eventType, object? eventData, string? cid, string? uid, string? token) =>
        {
            var formattedEvent = formatEvent(eventType, eventData, cid, null, null, uid, token);
            send(formattedEvent);
        };
    }

    private static Func<string, object?, string?, string?, string?, string?, string?, JObject> GetFormatEventFunction(
        Dictionary<string, DataPreprocessor> dataPreprocessors,
        string pid)
    {
        return (string eventType, object? data, string? cid, string? eventId, string? tid, string? uid, string? token) =>
        {
            object? formattedData = null;

            if (data != null)
            {
                var preprocessor = dataPreprocessors.ContainsKey(eventType)
                    ? dataPreprocessors[eventType]
                    : dataPreprocessors["__default__"];
                
                formattedData = preprocessor(data);
            }

            var generatedEventId = eventId ?? Guid.NewGuid().ToString();
            
            var metadata = new JObject
            {
                ["cid"] = cid ?? generatedEventId,
                ["tid"] = tid ?? generatedEventId,
                ["pid"] = pid
            };

            if (!string.IsNullOrEmpty(uid))
            {
                metadata["uid"] = uid;
            }

            if (!string.IsNullOrEmpty(token))
            {
                metadata["token"] = token;
            }

            var formattedEvent = new JObject
            {
                ["id"] = generatedEventId,
                ["type"] = eventType,
                ["metadata"] = metadata
            };

            if (formattedData != null)
            {
                formattedEvent["data"] = JToken.FromObject(formattedData);
            }

            return formattedEvent;
        };
    }
}

