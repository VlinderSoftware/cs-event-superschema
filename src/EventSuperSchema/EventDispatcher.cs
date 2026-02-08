using Newtonsoft.Json.Linq;

namespace EventSuperSchema;

/// <summary>
/// Factory for creating event dispatchers
/// </summary>
public static class EventDispatcher
{
    /// <summary>
    /// Get an event dispatcher that validates events and routes them to appropriate handlers
    /// </summary>
    /// <param name="err">Error handler that receives error messages</param>
    /// <param name="handlers">Dictionary mapping event types to their handlers.
    /// Supports exact type matching, base type matching (before last ':'), and '__default__' fallback</param>
    /// <returns>A dispatcher function that accepts events as JObject</returns>
    public static Action<JObject> GetEventDispatcher(
        ErrorHandler err,
        Dictionary<string, EventHandler> handlers)
    {
        return (JObject eventData) =>
        {
            // Validate against super-schema
            if (!SuperSchema.IsValid(eventData))
            {
                err(new ErrorMessage
                {
                    Error = "SchemaMismatchError",
                    Message = "Event does not match event schema"
                });
                return;
            }

            var eventType = eventData["type"]?.ToString();
            if (string.IsNullOrEmpty(eventType))
            {
                err(new ErrorMessage
                {
                    Error = "InvalidEventType",
                    Message = "Event type is missing or invalid"
                });
                return;
            }

            // Try exact match first
            if (handlers.TryGetValue(eventType, out var handler))
            {
                handler(err, eventData);
                return;
            }

            // Try base event name (everything before last ':')
            var lastColonIndex = eventType.LastIndexOf(':');
            if (lastColonIndex > 0)
            {
                var baseEventName = eventType.Substring(0, lastColonIndex);
                if (handlers.TryGetValue(baseEventName, out handler))
                {
                    handler(err, eventData);
                    return;
                }
            }

            // Try default handler
            if (handlers.TryGetValue("__default__", out handler))
            {
                handler(err, eventData);
            }
        };
    }
}

