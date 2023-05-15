using Digst.OioIdws.SoapCore.Behaviors;
using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;

namespace Digst.OioIdws.SoapCore
{
    /// <summary>
    /// This factory adds necessary custom endpoint behaviors
    /// </summary>
    /// <typeparam name="TChannel"></typeparam>
    public class FederatedChannelFactory<TChannel> : ChannelFactory<TChannel>
    {
        public FederatedChannelFactory(ServiceEndpoint endpoint) : base(endpoint)
        {
        }

        public FederatedChannelFactory(string endpointConfigurationName) : base(endpointConfigurationName)
        {
        }

        public FederatedChannelFactory(Binding binding, EndpointAddress remoteAddress) : base(binding, remoteAddress)
        {
        }

        public FederatedChannelFactory(string endpointConfigurationName, EndpointAddress remoteAddress) : base(endpointConfigurationName, remoteAddress)
        {
        }

        protected FederatedChannelFactory(Type channelType) : base(channelType)
        {
        }

        protected override void ApplyConfiguration(string configurationName)
        {
            if (Endpoint.EndpointBehaviors.Count == 0)
            {
                Endpoint.EndpointBehaviors.Add(new FederatedChannelClientCredentials());
            }
            Endpoint.EndpointBehaviors.Add(new SoapClientBehavior());
        }
    }

}
