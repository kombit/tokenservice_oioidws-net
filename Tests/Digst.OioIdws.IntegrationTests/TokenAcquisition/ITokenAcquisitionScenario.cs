
namespace Digst.OioIdws.IntegrationTests.TokenAcquisition;

public interface ITokenAcquisitionScenario
{
    GenericXmlSecurityToken AcquireTokenFromSts();
}