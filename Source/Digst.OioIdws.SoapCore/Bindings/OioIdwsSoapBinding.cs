using Digst.OioIdws.SoapCore.Tokens;
using System;
using System.IdentityModel.Tokens;
using System.ServiceModel.Channels;

namespace Digst.OioIdws.SoapCore.Bindings
{
    /// <summary>
    /// A custom binding that is responsible for creating a BindingElementCollection with correct elements
    /// that is used to call a WSP
    /// This class was named "SoapBinding" in the .NET Framework implementation
    /// </summary>
    public class OioIdwsSoapBinding : CustomBinding
    {
        public FederatedSecurityTokenParameters FederatedSecurityTokenParameters { get; }

        /// <summary>
        /// Initializes an instance of type <see cref="OioIdwsSoapBinding"/>
        /// </summary>
        /// <param name="federatedSecurityTokenParameters"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public OioIdwsSoapBinding(FederatedSecurityTokenParameters federatedSecurityTokenParameters)
        {
            FederatedSecurityTokenParameters = federatedSecurityTokenParameters ?? throw new ArgumentNullException(nameof(federatedSecurityTokenParameters));
        }

        public override BindingElementCollection CreateBindingElements()
        {
            var transport = new HttpsTransportBindingElement()
            {
                ManualAddressing = true,
            };

            // Otherwise, transport's MaxReceivedMessageSize is 65536
            if (FederatedSecurityTokenParameters.MaxReceivedMessageSize.HasValue)
            {
                transport.MaxReceivedMessageSize = FederatedSecurityTokenParameters.MaxReceivedMessageSize.Value;
            }

            transport.RequireClientCertificate = true;

            var encoding = new TextMessageEncodingBindingElement();
            // [OIO IDWS SOAP 1.1] requires SOAP 1.2 and WS-Addressing 1.0, but some services still use SOAP 1.1
            encoding.MessageVersion = FederatedSecurityTokenParameters.MessageVersion;

            var transportSecurityBindingElement = CreateMessageSecurity();

            var elements = new BindingElementCollection();
            elements.Add(transportSecurityBindingElement);
            elements.Add(new OioIdwsSoapBindingElement(FederatedSecurityTokenParameters));  // replace the AsymmetricSecurityBindingElement to sign stuff correctly
            elements.Add(encoding);
            elements.Add(transport);

            return elements;
        }

        /// <summary>
        /// Create a TransportSecurityBindingElement with a corresponding FederatedSecurityTokenParameters
        /// </summary>
        /// <returns></returns>
        private SecurityBindingElement CreateMessageSecurity()
        {
            TransportSecurityBindingElement result = new TransportSecurityBindingElement();
            result.IncludeTimestamp = true;
            result.LocalClientSettings.DetectReplays = false;
            result.MessageSecurityVersion = FederatedSecurityTokenParameters.MessageSecurityVersion;

            if (FederatedSecurityTokenParameters.KeyType == SecurityKeyType.BearerKey)
            {
                result.EndpointSupportingTokenParameters.Signed.Add(FederatedSecurityTokenParameters);
            }
            else
            {
                result.EndpointSupportingTokenParameters.Endorsing.Add(FederatedSecurityTokenParameters);
            }

            return result;
        }
    }
}