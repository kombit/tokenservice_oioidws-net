using X509SecurityToken = Digst.OioIdws.OioWsTrustCore.X509SecurityToken;

namespace Digst.OioIdws.IntegrationTests.TokenAcquisition;

public class ProxyOboScenario : ITokenAcquisitionScenario
{
    private readonly IStsTokenService _tokenService;
    private readonly StsTokenServiceConfiguration _stsConfiguration;

    public ProxyOboScenario(IStsTokenService tokenService, StsTokenServiceConfiguration stsConfiguration)
    {
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        _stsConfiguration = stsConfiguration ?? throw new ArgumentNullException(nameof(stsConfiguration));
    }

    public GenericXmlSecurityToken AcquireTokenFromSts()
    {
        if(_stsConfiguration.StsConfiguration.OboCertificate == null)
        {
            throw new ConfigurationErrorsException("Obo Certificate");
        }
        var oboToken = new X509SecurityToken(_stsConfiguration.StsConfiguration.OboCertificate);
        return (GenericXmlSecurityToken)_tokenService.GetTokenWithProxyOnBehalfOf(oboToken);
    }
}