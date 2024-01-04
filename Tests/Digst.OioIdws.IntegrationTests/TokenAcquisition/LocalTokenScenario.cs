
using Microsoft.IdentityModel.Tokens.Saml2;

namespace Digst.OioIdws.IntegrationTests.TokenAcquisition;

/// <summary>
/// Local token scenario. Simulate a local security token by request STS to get a token, Requests a token from the STS with the simulated local security token
/// </summary>
public class LocalTokenScenario : ITokenAcquisitionScenario
{
    private readonly IStsTokenService _tokenService;

    /// <summary>
    /// Instantiate the Local Token Scenario
    /// </summary>
    /// <param name="tokenService">The STS to acquire the WSP-required token from.</param>
    public LocalTokenScenario(IStsTokenService tokenService)
    {
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
    }

    public GenericXmlSecurityToken AcquireTokenFromSts()
    {
        var localToken = AcquireTokenFromLocalSts();
        return (GenericXmlSecurityToken)_tokenService.GetTokenWithOnBehalfOf(localToken);
    }

    /// <summary>
    /// Acquire a token from STS to use as a simulated local security token.
    /// </summary>
    private Microsoft.IdentityModel.Tokens.SecurityToken AcquireTokenFromLocalSts()
    {
        GenericXmlSecurityToken securityToken = (GenericXmlSecurityToken)_tokenService.GetToken();

        var xmlReader = new XmlNodeReader(securityToken.TokenXml);
        var saml2Serializer = new Saml2Serializer();
        var saml2Assertion = saml2Serializer.ReadAssertion(xmlReader);
        return new Saml2SecurityToken(saml2Assertion);
    }
}