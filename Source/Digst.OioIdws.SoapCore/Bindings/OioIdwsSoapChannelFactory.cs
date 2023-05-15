using Digst.OioIdws.SoapCore.Tokens;
using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Digst.OioIdws.SoapCore.Bindings
{
    /// <summary>
    /// This factory is responsible for building a channel of type <see cref="OioIdwsSoapChannel"/>
    /// </summary>
    public class OioIdwsSoapChannelFactory : ChannelFactoryBase<IRequestChannel>
    {
        private readonly IChannelFactory<IRequestChannel> _innerFactory;
        private readonly FederatedSecurityTokenParameters _federatedSecurityTokenParameters;

        /// <summary>
        /// Initializes a new instance of the <see cref="OioWsTrustChannelFactory"/> class.
        /// </summary>
        /// <param name="innerFactory">The inner factory.</param>
        /// <param name="stsTokenServiceConfiguration">The STS token service configuration.</param>
        /// <param name="stsAuthenticationCase">Specifies the STS authentication case</param>
        public OioIdwsSoapChannelFactory(IChannelFactory<IRequestChannel> innerFactory, FederatedSecurityTokenParameters federatedSecurityTokenParameters)
        {
            _innerFactory = innerFactory ?? throw new ArgumentNullException(nameof(innerFactory));
            _federatedSecurityTokenParameters = federatedSecurityTokenParameters ?? throw new ArgumentNullException(nameof(federatedSecurityTokenParameters));
        }

        #region Members which simply delegate to the inner factory
        protected override void OnOpen(TimeSpan timeout)
        {
            _innerFactory.Open(timeout);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return _innerFactory.BeginOpen(timeout, callback, state);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            _innerFactory.EndOpen(result);
        }
        #endregion

        protected override IRequestChannel OnCreateChannel(EndpointAddress address, Uri via)
        {
            return new OioIdwsSoapChannel(this, _innerFactory.CreateChannel(address, via), _federatedSecurityTokenParameters);
        }
    }
}
