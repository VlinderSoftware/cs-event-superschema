using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace EventSuperSchema;

/// <summary>
/// Provides the super-schema definition and validation for events
/// </summary>
public static class SuperSchema
{
    /// <summary>
    /// The JSON schema for validating events
    /// </summary>
    public static readonly JSchema Schema = JSchema.Parse(@"{
        ""type"": ""object"",
        ""properties"": {
            ""id"": { ""type"": ""string"", ""format"": ""uuid"" },
            ""type"": { ""type"": ""string"" },
            ""metadata"": {
                ""type"": ""object"",
                ""properties"": {
                    ""cid"": { ""type"": ""string"", ""format"": ""uuid"" },
                    ""tid"": { ""type"": ""string"", ""format"": ""uuid"" },
                    ""pid"": { ""type"": ""string"", ""format"": ""uuid"" },
                    ""uid"": { ""type"": ""string"", ""format"": ""uuid"" },
                    ""token"": { ""type"": ""string"" }
                },
                ""required"": [ ""cid"", ""pid"" ]
            },
            ""data"": { ""type"": ""object"" }
        },
        ""required"": [ ""id"", ""type"", ""metadata"" ]
    }");

    /// <summary>
    /// Validates an event against the super-schema
    /// </summary>
    /// <param name="eventData">The event to validate</param>
    /// <returns>True if the event is valid, false otherwise</returns>
    public static bool IsValid(JObject eventData)
    {
        return eventData.IsValid(Schema);
    }

    /// <summary>
    /// Validates an event against the super-schema and returns validation errors
    /// </summary>
    /// <param name="eventData">The event to validate</param>
    /// <param name="errors">Output parameter containing validation errors if any</param>
    /// <returns>True if the event is valid, false otherwise</returns>
    public static bool IsValid(JObject eventData, out IList<string> errors)
    {
        return eventData.IsValid(Schema, out errors);
    }
}

