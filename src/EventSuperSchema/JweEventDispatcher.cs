using Newtonsoft.Json.Linq;

namespace EventSuperSchema;

/// <summary>
/// Factory for creating JWE (JSON Web Encryption) encrypted event dispatchers
/// </summary>
public static class JweEventDispatcher
{
    /// <summary>
    /// Get a JWE-encrypted event dispatcher
    /// </summary>
    /// <param name="err">Error handler</param>
    /// <param name="handlers">Event handlers</param>
    /// <param name="decryptFunction">Function to decrypt JWE tokens</param>
    /// <returns>A dispatcher that decrypts and processes encrypted events</returns>
    /// <remarks>
    /// This is a placeholder implementation. For production use, integrate with
    /// Microsoft.IdentityModel.JsonWebTokens or similar JWE library.
    /// </remarks>
    public static Action<string> GetJweEventDispatcher(
        ErrorHandler err,
        Dictionary<string, EventHandler> handlers,
        Func<string, JObject> decryptFunction)
    {
        var baseDispatcher = EventDispatcher.GetEventDispatcher(err, handlers);

        return (string encryptedEvent) =>
        {
            try
            {
                var decryptedEvent = decryptFunction(encryptedEvent);
                baseDispatcher(decryptedEvent);
            }
            catch (Exception ex)
            {
                err(new ErrorMessage
                {
                    Error = "DecryptionError",
                    Message = $"Failed to decrypt event: {ex.Message}"
                });
            }
        };
    }
}
