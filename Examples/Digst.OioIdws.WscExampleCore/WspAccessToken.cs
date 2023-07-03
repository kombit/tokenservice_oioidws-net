using System.Text.Json.Serialization;

namespace Digst.OioIdws.WscExampleCore
{
    public class WspAccessToken
    {
        [JsonPropertyName("access_token")]
        /// <summary>
        /// Access token to use in authorization header in service calls.
        /// </summary>
        public string? AccessToken { get; set; }
        [JsonPropertyName("expires_in")]
        /// <summary>
        /// Seconds before this token expires and new one must be created. This is seconds from when token was initially created.
        /// </summary>
        public int ExpiresIn { get; set; }

        [JsonPropertyName("token_type")]
        /// <summary>
        /// Type of token. Either holder-of-key or bearer.
        /// </summary>
        public string? TokenType { get; set; }
    }
}
