using System.Security.Cryptography;

namespace Digst.OioIdws.CommonCore.Constants
{
    /// <summary>
    /// Define some constants of SHA256 algorithms
    /// </summary>
    public static class Sha256SignatureAlgorithms
    {
        // SHA256 URL Identifiers
#pragma warning disable S1075 // URIs should not be hardcoded
        public const string XmlDsigMoreRsaSha256Url = @"http://www.w3.org/2001/04/xmldsig-more#rsa-sha256"; // Signature algoritm
#pragma warning restore S1075 // URIs should not be hardcoded
#pragma warning disable S1075 // URIs should not be hardcoded
        public const string XmlEncSha256Url = @"http://www.w3.org/2001/04/xmlenc#sha256"; // Digest algorithm
#pragma warning restore S1075 // URIs should not be hardcoded
    }
}