using Digst.OioIdws.CommonCore;
using Digst.OioIdws.SoapCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Reflection;
using System.Runtime.Caching;

namespace Digst.OioIdws.IntegrationTests;

public abstract class IntegrationTestBase
{
    private IStsTokenService _tokenService;

    private ITokenAcquisitionScenario _acquisitionScenario;

    protected StsTokenServiceConfiguration Configuration { get; }

    protected IntegrationTestBase(string soapVersion)
    {
        var exeLocation = Assembly.GetExecutingAssembly().Location;
        var configuration = ConfigurationManager.OpenExeConfiguration(exeLocation);
        var configSection = (OioIdwsWcfConfigurationSection)configuration.GetSection("oioIdwsWcfConfiguration");
        if (soapVersion == "1.2")
        {
            configSection.WspSoapVersion = "1.2";
            configSection.WspEndpointID = configuration.AppSettings.Settings["Wsp12EndpointId"].Value;
            configSection.WspEndpoint = configuration.AppSettings.Settings["Wsp12Endpoint"].Value;
        }

        Configuration = TokenServiceConfigurationFactory.CreateConfiguration(configSection);
        Configuration.SslCertificateAuthentication.CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.None;
        Configuration.WspCertificateAuthentication.CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.None;
        Configuration.StsCertificateAuthentication.CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.None;

        _tokenService = new StsTokenService(Configuration);
        _acquisitionScenario = new SystemUserScenario(_tokenService);
    }

    protected void ConfigureSystemUserScenario()
    {
        _acquisitionScenario = new SystemUserScenario(_tokenService);
    }

    protected void ConfigureSystemUserScenario(IDistributedCache customReplayAttackCache)
    {
        Configuration.ReplayAttackCache = customReplayAttackCache;
        _tokenService = new StsTokenService(Configuration);
        _acquisitionScenario = new SystemUserScenario(_tokenService);
    }

    protected void ConfigureLocalTokenScenario()
    {
        _acquisitionScenario = new LocalTokenScenario(_tokenService);
    }

    protected void ConfigureProxyOboScenario()
    {
        _acquisitionScenario = new ProxyOboScenario(_tokenService, Configuration);
    }

    protected T CreateChannel<T>()
    {
        var token = _acquisitionScenario.AcquireTokenFromSts();
        return FederatedChannelFactoryExtensions.CreateChannelWithIssuedToken<T>(token, Configuration);
    }
}
