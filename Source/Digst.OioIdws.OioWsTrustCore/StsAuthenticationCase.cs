namespace Digst.OioIdws.OioWsTrustCore
{

    /// <summary>
    /// The scenarios supported by KOMBIT STS
    /// </summary>
    public enum StsAuthenticationCase
    {
        /// <summary>
        /// The signature case (also called the normal STS case): The request for security token (RST) must be signed
        /// using the employee FOCES certificate. The RST contains no "OnBehalfOf as" token.
        /// </summary>
        SignatureCase = 0,

        /// <summary>
        /// The OBO case: The request for security token (RST) must
        /// contain a local token from a local STS. The local STS must have
        /// been registered in advance with KOMBIT STS using the local STS
        /// entity ID and signing certificate.
        /// </summary>
        OnBehalfOfCase = 1,

        /// <summary>
        /// The Proxy OBO case: The request for security token (RST) must
        /// contain a certificate of a system that the caller wants to act on behalf of.
        /// </summary>
        ProxyOnBehalfOfCase = 2,
    }
}