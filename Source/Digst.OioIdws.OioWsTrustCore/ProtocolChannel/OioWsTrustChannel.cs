using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Digst.OioIdws.OioWsTrustCore.ProtocolChannel
{
    /// <summary>
    /// This custom channel allows us to manipulate a request before sending it to an STS as well as to verify and manipulate a response after receiving it from the said STS
    /// </summary>
    public class OioWsTrustChannel : ChannelBase, IRequestChannel
    {
        private readonly OioWsTrustChannelFactory _channelManager;
        private readonly IRequestChannel _innerChannel;

        public OioWsTrustChannel(OioWsTrustChannelFactory channelManager, IRequestChannel innerChannel)
            : base(channelManager)
        {
            if (channelManager == null) throw new ArgumentNullException("channelManager");
            if (innerChannel == null) throw new ArgumentNullException("innerChannel");
            _channelManager = channelManager;
            _innerChannel = innerChannel;
        }

        /// <summary>
        /// Request a token from an STS in a synchronous manner
        /// </summary>
        /// <param name="message">The RST message</param>
        /// <returns>A RSTR message or a fault message</returns>
        public Message Request(Message message)
        {
            return Request(message, DefaultSendTimeout);
        }

        /// <summary>
        /// Request a token from an STS in a synchronous manner
        /// </summary>
        /// <param name="message">The RST message</param>
        /// <param name="timeout"></param>
        /// <returns>A RSTR message or a fault message</returns>
        public Message Request(Message message, TimeSpan timeout)
        {
            var signatureCaseMessageTransformer = new OioWsTrustMessageTransformer(_channelManager.StsTokenServiceConfiguration);
            var modifiedMessage = signatureCaseMessageTransformer.ModifyMessageAccordingToStsNeeds(message);
            var response = _innerChannel.Request(modifiedMessage, timeout);
            
            return signatureCaseMessageTransformer.ValidateAndModifyMessageAccordingToWsTrust(response, modifiedMessage.Headers.MessageId);
        }

        #region Members which simply delegate to the inner channel
        public IAsyncResult BeginRequest(Message message, AsyncCallback callback, object state)
        {
            return _innerChannel.BeginRequest(message, callback, state);
        }

        public IAsyncResult BeginRequest(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return _innerChannel.BeginRequest(message, timeout, callback, state);
        }

        public Message EndRequest(IAsyncResult result)
        {
            return _innerChannel.EndRequest(result);
        }
        protected override void OnAbort()
        {
            _innerChannel.Abort();
        }

        protected override void OnClose(TimeSpan timeout)
        {
            _innerChannel.Close(timeout);
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            _innerChannel.EndClose(result);
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return _innerChannel.BeginClose(timeout, callback, state);
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            _innerChannel.Open(timeout);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return _innerChannel.BeginOpen(timeout, callback, state);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            _innerChannel.EndOpen(result);
        }

        public EndpointAddress RemoteAddress
        {
            get { return _innerChannel.RemoteAddress; } 
        }

        public Uri Via
        {
            get { return _innerChannel.Via; }
        }
        #endregion
    }
}
