using Digst.OioIdws.CommonCore.Logging;
using Digst.OioIdws.OioWsTrustCore.ProtocolChannel;
using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;

namespace Digst.OioIdws.OioWsTrustCore.Bindings
{
    /// <summary>
    /// This custom binding initializes all necessary binding elements needed to make a successful WSTrust request
    /// </summary>
    public class OioWsTrustBinding : CustomBinding
    {
        private readonly StsAuthenticationCase _stsAuthenticationCase;
        private readonly StsTokenServiceConfiguration _stsTokenServiceConfiguration;
        public new MessageVersion MessageVersion { get; } = MessageVersion.CreateVersion(EnvelopeVersion.Soap11, AddressingVersion.WSAddressing10);

        public OioWsTrustBinding(StsAuthenticationCase stsAuthenticationCase, StsTokenServiceConfiguration stsTokenServiceConfiguration)
        {
            _stsAuthenticationCase = stsAuthenticationCase;
            _stsTokenServiceConfiguration = stsTokenServiceConfiguration ?? throw new ArgumentNullException(nameof(stsTokenServiceConfiguration));
        }

        public override BindingElementCollection CreateBindingElements()
        {
            var elements = new BindingElementCollection();

            if (_stsTokenServiceConfiguration.SendTimeout.HasValue)
            {
                Logger.Instance.Warning($"RequestToken send timeout set to {_stsTokenServiceConfiguration.SendTimeout.Value}");
                SendTimeout = _stsTokenServiceConfiguration.SendTimeout.Value;
            }

            elements.Add(new OioWsTrustBindingElement(_stsTokenServiceConfiguration, _stsAuthenticationCase));
            // Assuming that all STS uses SOAP 1.1
            elements.Add(new TextMessageEncodingBindingElement(MessageVersion.CreateVersion(EnvelopeVersion.Soap11, AddressingVersion.WSAddressing10),
                Encoding.UTF8));
            // ManualAddressing must be true in order to make sure that wsa header elements are not altered in the HttpsTransportChannel which happens after xml elements have been digitally signed.
            elements.Add(new HttpsTransportBindingElement() { ManualAddressing = true });

            return elements;
        }
    }
}
