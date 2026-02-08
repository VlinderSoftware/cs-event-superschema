using Newtonsoft.Json.Linq;

namespace EventSuperSchema;

/// <summary>
/// Factory for creating JWS (JSON Web Signature) signed send event functions
/// </summary>
public static class JwsSendEventFunction
{
    /// <summary>
    /// Get a function to send JWS-signed events
    /// </summary>
    /// <param name="send">Function to send signed events</param>
    /// <param name="pid">Producer ID</param>
    /// <param name="signFunction">Function to sign event objects into JWS tokens</param>
    /// <param name="dataPreprocessors">Optional data preprocessors</param>
    /// <returns>A function that formats and signs events before sending</returns>
    /// <remarks>
    /// This is a placeholder implementation. For production use, integrate with
    /// Microsoft.IdentityModel.JsonWebTokens or similar JWS library.
    /// </remarks>
    public static Action<string, object?, string?, string?, string?> GetJwsSendEventFunction(
        Action<string> send,
        string pid,
        Func<JObject, string> signFunction,
        Dictionary<string, DataPreprocessor>? dataPreprocessors = null)
    {
        var formatAndSend = SendEventFunction.GetSendEventFunction(
            (JObject evt) =>
            {
                var signed = signFunction(evt);
                send(signed);
            },
            pid,
            dataPreprocessors
        );

        return formatAndSend;
    }
}
