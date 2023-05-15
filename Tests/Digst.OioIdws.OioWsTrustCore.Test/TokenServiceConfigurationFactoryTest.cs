using Digst.OioIdws.WscCore.OioWsTrust;
using Digst.OioIdws.Common.TestUtils;
using System.Runtime.Caching;
using System.ServiceModel;
using Microsoft.Extensions.Caching.Distributed;

namespace Digst.OioIdws.OioWsTrustCore.Test;

[TestClass]
public class TokenServiceConfigurationFactoryTest
{
    [TestMethod]
    [TestCategory(Constants.UnitTest)]
    public void ConfigMissingInConfigurationTest()
    {
        // Setup
        OioIdwsWcfConfigurationSection wscConfiguration = new OioIdwsWcfConfigurationSection()
        {
            StsEndpointAddress = "https://stsendpoint.addr",
            StsEntityIdentifier = "http://stsentity.id",
            Cvr = "123456789",
            WspEndpoint = "https://endpoint.wsp",
            WspEndpointID = "http://service.id",
            WspSoapVersion = "1.2",
            TokenLifeTimeInMinutes = 60,
            MaxReceivedMessageSize = 1024 * 256,
            ClientCertificate = new Certificate
            {
                FromFileSystem = true,
                FilePath = "Data\\Test.p12",
                Password = "D0mm3dag"
            },
            StsCertificate = new Certificate
            {
                FromFileSystem = true,
                FilePath = "Data\\Test.p12",
                Password = "D0mm3dag"
            },
            ServiceCertificate = new Certificate
            {
                FromFileSystem = true,
                FilePath = "Data\\Test.p12",
                Password = "D0mm3dag",
                FindValue = "Any",
                StoreLocation = System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine,
                StoreName = System.Security.Cryptography.X509Certificates.StoreName.My,
                X509FindType = System.Security.Cryptography.X509Certificates.X509FindType.FindByThumbprint
            },
            OboCertificate = new Certificate
            {
                FromFileSystem = true,
                FilePath = "Data\\Test.p12",
                Password = "D0mm3dag"
            },
            IncludeLibertyHeader = true,
            DebugMode = false,
            CacheClockSkewInSeconds = 5
        };

        // Act
        var sut = TokenServiceConfigurationFactory.CreateConfiguration(wscConfiguration);

        // Assert

        Assert.IsNotNull(sut);
        Assert.AreEqual(wscConfiguration.StsEndpointAddress, sut.StsConfiguration.EndpointAddress);
        Assert.AreEqual(wscConfiguration.StsEntityIdentifier, sut.StsConfiguration.EntityIdentifier);
        Assert.AreEqual(wscConfiguration.Cvr, sut.StsConfiguration.Cvr);
        Assert.IsNotNull(sut.StsConfiguration.Certificate);
        Assert.IsNotNull(sut.StsConfiguration.OboCertificate);

        Assert.AreEqual(wscConfiguration.WspEndpoint, sut.WspConfiguration.EndpointAddress);
        Assert.AreEqual(wscConfiguration.WspEndpointID, sut.WspConfiguration.EndpointId);
        Assert.AreEqual(EnvelopeVersion.Soap12, sut.WspConfiguration.SoapVersion);
        Assert.IsNotNull(sut.WspConfiguration.ServiceCertificate);

        Assert.AreEqual(wscConfiguration.TokenLifeTimeInMinutes, sut.TokenLifeTimeInMinutes);
        Assert.AreEqual(wscConfiguration.MaxReceivedMessageSize, sut.MaxReceivedMessageSize);
        Assert.IsNotNull(sut.ClientCertificate);

        Assert.IsNotNull(sut.ReplayAttackCache);
        Assert.IsInstanceOfType<MemoryDistributedCache>(sut.ReplayAttackCache);
        Assert.IsTrue(sut.IncludeLibertyHeader);
        Assert.AreEqual(5, sut.CacheClockSkew.TotalSeconds);
    }

    [TestMethod]
    public void CreateConfigurationWithNullWscConfigurationWillThrowException()
    {
        // No configuration from config file
        Assert.ThrowsException<ArgumentNullException>(() => { TokenServiceConfigurationFactory.CreateConfiguration(); });
    }
}
