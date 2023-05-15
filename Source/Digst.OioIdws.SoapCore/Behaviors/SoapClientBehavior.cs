using System;
using Digst.OioIdws.CommonCore.Logging;
using Digst.OioIdws.SoapCore.MessageInspectors;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using Digst.OioIdws.SoapCore.Bindings;

namespace Digst.OioIdws.SoapCore.Behaviors
{
    /// <summary>
    /// This custom behavior class is used to add the liberty framework header to the SOAP message and ensure that it is included in the signature.
    /// </summary>
    public class SoapClientBehavior : IEndpointBehavior
    {
        public void AddBindingParameters(ServiceEndpoint endpoint,
            BindingParameterCollection bindingParameters)
        {
            // This is done in order to have the liberty framework header included in the SOAP signaure. This is required by [OIO IDWS SOAP 1.1].
            Logger.Instance.Trace("Specifying that the liberty framework header must be signed in the request to WSP.");
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            Logger.Instance.Trace("Adding message inspector on WSC.");
            var binding = endpoint.Binding as OioIdwsSoapBinding;
            bool includeLibertyHeader = false;
            if (binding != null)
            {
                includeLibertyHeader = binding.FederatedSecurityTokenParameters.IncludeLibertyHeader;
            }
            var inspector = new SoapMessageInspector(includeLibertyHeader);
            clientRuntime.ClientMessageInspectors.Add(inspector);
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            // No Dispatch Behavior needed
        }

        public void Validate(ServiceEndpoint endpoint)
        {
            // No Validation needed
        }
    }
}
