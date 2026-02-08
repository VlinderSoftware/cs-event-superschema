using Newtonsoft.Json.Linq;

namespace EventSuperSchema;

/// <summary>
/// Factory for creating JWE (JSON Web Encryption) encrypted send event functions
/// </summary>
public static class JweSendEventFunction
{
    /// <summary>
    /// Get a function to send JWE-encrypted events
    /// </summary>
    /// <param name="send">Function to send encrypted events</param>
    /// <param name="pid">Producer ID</param>
    /// <param name="encryptFunction">Function to encrypt event objects into JWE tokens</param>
    /// <param name="dataPreprocessors">Optional data preprocessors</param>
    /// <returns>A function that formats and encrypts events before sending</returns>
    /// <remarks>
    /// This is a placeholder implementation. For production use, integrate with
    /// Microsoft.IdentityModel.JsonWebTokens or similar JWE library.
    /// </remarks>
    public static Action<string, object?, string?, string?, string?> GetJweSendEventFunction(
        Action<string> send,
        string pid,
        Func<JObject, string> encryptFunction,
        Dictionary<string, DataPreprocessor>? dataPreprocessors = null)
    {
        var formatAndSend = SendEventFunction.GetSendEventFunction(
            (JObject evt) =>
            {
                var encrypted = encryptFunction(evt);
                send(encrypted);
            },
            pid,
            dataPreprocessors
        );

        return formatAndSend;
    }
}

