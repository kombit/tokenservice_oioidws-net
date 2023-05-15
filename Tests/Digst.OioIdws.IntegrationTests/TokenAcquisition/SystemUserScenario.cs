namespace Digst.OioIdws.IntegrationTests.TokenAcquisition;

public class SystemUserScenario : ITokenAcquisitionScenario
{
    private readonly IStsTokenService _tokenService;

    public SystemUserScenario(IStsTokenService tokenService)
    {
        _tokenService = tokenService;
    }
    
    public GenericXmlSecurityToken AcquireTokenFromSts()
    {
        return (GenericXmlSecurityToken)_tokenService.GetToken();
    }
}