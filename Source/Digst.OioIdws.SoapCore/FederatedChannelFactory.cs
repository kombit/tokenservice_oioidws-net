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

        public FederatedChannelFactory(Binding binding, EndpointAddress remoteAddress) : base(binding, remoteAddress)
        {
        }

        protected FederatedChannelFactory(Type channelType) : base(channelType)
        {
        }

        protected override ServiceEndpoint CreateDescription()
        {
            ServiceEndpoint endpoint = base.CreateDescription();

            if (endpoint.EndpointBehaviors.Count == 0)
            {
                endpoint.EndpointBehaviors.Add(new FederatedChannelClientCredentials());
            }
            endpoint.EndpointBehaviors.Add(new SoapClientBehavior());

            return endpoint;
        }
    }

}
