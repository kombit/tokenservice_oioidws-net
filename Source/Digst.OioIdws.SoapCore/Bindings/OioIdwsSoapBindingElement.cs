﻿using Digst.OioIdws.SoapCore.Tokens;
using System;
using System.Linq;
using System.ServiceModel.Channels;

namespace Digst.OioIdws.SoapCore.Bindings
{
    /// <summary>
    /// This binding element is responsible for building a channel factory of type <see cref="OioIdwsSoapChannelFactory"/>
    /// </summary>
    public class OioIdwsSoapBindingElement : BindingElement
    {
        public FederatedSecurityTokenParameters FederatedSecurityTokenParameters { get; }

        /// <inheritdoc />
        public override BindingElement Clone()
        {
            return new OioIdwsSoapBindingElement(FederatedSecurityTokenParameters);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OioWsTrustBindingElement"/> class.
        /// </summary>
        /// <param name="stsTokenServiceConfiguration">Configuration parameters for the security token service (STS)</param>
        /// <param name="stsAuthenticationCase"></param>
        public OioIdwsSoapBindingElement(FederatedSecurityTokenParameters federatedSecurityTokenParameters)
        {
            FederatedSecurityTokenParameters = federatedSecurityTokenParameters ?? throw new ArgumentNullException(nameof(federatedSecurityTokenParameters));
        }

#nullable disable
        /// <inheritdoc />
        public override T GetProperty<T>(BindingContext context)
        {
            // GetProperty<T>(BindingContext context) of SignatureCaseBindingElement must match GetProperty<T>() of SignatureCaseChannelFactory. 
            // In runtime, Wcf verifies that the security capabilities the factory claims it can support are also supported by the actual channel.
            // SignatureCaseChannelFactory uses the default implementation of GetProperty<T>() which returns null and this method returns an instance of type ISecurityCapabilities (created by HttpsTransportBindingElement). This makes the WCF throw an exception.
            // Either side could be changed to match the other but setting null here has been chosen because SecurityCapabilities is internal to the .Net Framework. The alternative would have been to copy/paste SecurityCapabilities and create my own implementation of ISecurityCapabilities and override GetProperty<T>() in SignatureCaseChannelFactory.
            // Downside of this is that above protocol channels are not able to read the ISecurityCapabilities.
            if (typeof(T) == typeof(ISecurityCapabilities))
                return null;

            return context.GetInnerProperty<T>();
        }
#nullable enable
        /// <summary>
        /// Returns a value that indicates whether the binding element can build a channel factory for a specific type of channel.
        /// </summary>
        /// <typeparam name="TChannel">The type of channel the channel factory produces.</typeparam>
        /// <param name="context">The <see cref="T:System.ServiceModel.Channels.BindingContext" /> that provides context for the binding element.</param>
        /// <returns>
        ///   <see langword="true" /> if the <see cref="T:System.ServiceModel.Channels.IChannelFactory`1" /> of type <paramref name="TChannel" /> can be built by the binding element; otherwise, <see langword="false" />.
        /// </returns>
        public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            // Return true if it is a request channel and the rest of the call stack supports this type of channel.
            return typeof(TChannel) == typeof(IRequestChannel) && context.CanBuildInnerChannelFactory<TChannel>();
        }

        /// <summary>
        /// Initializes a channel factory for producing channels of a specified type from the binding context.
        /// </summary>
        /// <typeparam name="TChannel">The type of channel the factory builds.</typeparam>
        /// <param name="context">The <see cref="T:System.ServiceModel.Channels.BindingContext" /> that provides context for the binding element.</param>
        /// <returns>
        /// The <see cref="T:System.ServiceModel.Channels.IChannelFactory`1" /> of type <paramref name="TChannel" /> initialized from the <paramref name="context" />.
        /// </returns>
        /// <exception cref="InvalidOperationException">No Client certificate was configured.</exception>
        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            var clientCredentials = context.BindingParameters.OfType<FederatedChannelClientCredentials>().SingleOrDefault();
            if (clientCredentials?.ClientCertificate?.Certificate == null)
                throw new InvalidOperationException("No Client certificate was configured.");

            var innerFactory = context.BuildInnerChannelFactory<IRequestChannel>();
            var factory = new OioIdwsSoapChannelFactory(innerFactory, FederatedSecurityTokenParameters);
            return (IChannelFactory<TChannel>)factory;
        }
    }
}
