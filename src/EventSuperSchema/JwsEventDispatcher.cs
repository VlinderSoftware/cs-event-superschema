using Newtonsoft.Json.Linq;

namespace EventSuperSchema;

/// <summary>
/// Factory for creating JWS (JSON Web Signature) signed event dispatchers
/// </summary>
public static class JwsEventDispatcher
{
    /// <summary>
    /// Get a JWS-signed event dispatcher
    /// </summary>
    /// <param name="err">Error handler</param>
    /// <param name="handlers">Event handlers</param>
    /// <param name="verifyFunction">Function to verify and extract payload from JWS tokens</param>
    /// <returns>A dispatcher that verifies and processes signed events</returns>
    /// <remarks>
    /// This is a placeholder implementation. For production use, integrate with
    /// Microsoft.IdentityModel.JsonWebTokens or similar JWS library.
    /// </remarks>
    public static Action<string> GetJwsEventDispatcher(
        ErrorHandler err,
        Dictionary<string, EventHandler> handlers,
        Func<string, JObject> verifyFunction)
    {
        var baseDispatcher = EventDispatcher.GetEventDispatcher(err, handlers);

        return (string signedEvent) =>
        {
            try
            {
                var verifiedEvent = verifyFunction(signedEvent);
                baseDispatcher(verifiedEvent);
            }
            catch (Exception ex)
            {
                err(new ErrorMessage
                {
                    Error = "VerificationError",
                    Message = $"Failed to verify event signature: {ex.Message}"
                });
            }
        };
    }
}
