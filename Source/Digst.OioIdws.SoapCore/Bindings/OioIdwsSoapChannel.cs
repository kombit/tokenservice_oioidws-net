using Digst.OioIdws.CommonCore;
using Digst.OioIdws.CommonCore.Constants;
using Digst.OioIdws.CommonCore.Logging;
using Digst.OioIdws.SoapCore.Tokens;
using Digst.OioIdws.SoapCore.Utils;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;
using System.Xml;
using System.Xml.Linq;

namespace Digst.OioIdws.SoapCore.Bindings
{
    /// <summary>
    /// This custom request channel is responsible for the actual communication between client and a WSP:
    /// 1. Modify a request to meet requirements of an OioIdws WSP
    /// 2. Send the modified request to the WSP and receive a response
    /// 3. Validate the response
    /// 4. Modify the security header of the response so that .NET Core's default processing code will not complain about elements that it does not understand
    /// </summary>
    public class OioIdwsSoapChannel : ChannelBase, IRequestChannel
    {
        private readonly IRequestChannel _innerChannel;
        private readonly FederatedSecurityTokenParameters _federatedSecurityTokenParameters;
        private readonly IDistributedCache _distributedCache;

        public OioIdwsSoapChannel(OioIdwsSoapChannelFactory channelManager, IRequestChannel innerChannel, FederatedSecurityTokenParameters federatedSecurityTokenParameters)
            : base(channelManager)
        {
            _innerChannel = innerChannel ?? throw new ArgumentNullException("innerChannel");
            _federatedSecurityTokenParameters = federatedSecurityTokenParameters ?? throw new ArgumentNullException(nameof(federatedSecurityTokenParameters));
            _distributedCache = _federatedSecurityTokenParameters.StsTokenServiceConfiguration.ReplayAttackCache;
        }

        public Message Request(Message message)
        {
            return Request(message, DefaultSendTimeout);
        }

        public Message Request(Message message, TimeSpan timeout)
        {
            // Synchorous case: Wsc calls Wsp with sync way
            var signatureCaseMessageTransformer = new OioIdwsSoapMessageTransformer(_federatedSecurityTokenParameters);
            var modifiedMessage = signatureCaseMessageTransformer.ModifyMessageAccordingToWspNeeds(message);
            var response = _innerChannel.Request(modifiedMessage, timeout);

            // manually validate signature and other constraints because .NET Core does not have support for Asymmetric and message security
            response = Validate(response, modifiedMessage.Headers.MessageId);

            // after verifying that the response's security header is correct, modify the message
            var transformer = new OioIdwsSoapMessageTransformer(_federatedSecurityTokenParameters);
            var manipulatedResponse = transformer.ModifyMessageToSkipDefautValidation(response);

            manipulatedResponse.Properties.Add("originalMessage", response);
            return manipulatedResponse;
        }

        #region Members which simply delegate to the inner channel
        public IAsyncResult BeginRequest(Message message, AsyncCallback callback, object state)
        {
            return BeginRequest(message, DefaultSendTimeout, callback, state);
        }

        public IAsyncResult BeginRequest(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            var signatureCaseMessageTransformer = new OioIdwsSoapMessageTransformer(_federatedSecurityTokenParameters);
            var modifiedMessage = signatureCaseMessageTransformer.ModifyMessageAccordingToWspNeeds(message);
            state = modifiedMessage.Headers.MessageId;
            return _innerChannel.BeginRequest(modifiedMessage, timeout, callback, state);
        }

        public Message EndRequest(IAsyncResult result)
        {
            Message originalResponse = _innerChannel.EndRequest(result);

            // manually validate signature and other constraints because .NET Core does not have support for Asymmetric and message security
            UniqueId originalMessageId = (UniqueId)result.AsyncState;
            originalResponse = Validate(originalResponse, originalMessageId);

            // after verifying that the response's security header is correct, modify the message
            var transformer = new OioIdwsSoapMessageTransformer(_federatedSecurityTokenParameters);
            var manipulatedResponse = transformer.ModifyMessageToSkipDefautValidation(originalResponse);

            manipulatedResponse.Properties.Add("originalMessage", originalResponse);
            return manipulatedResponse;
        }

        private Message Validate(Message response, UniqueId messageId)
        {
            if (!response.IsFault)
            {
                // Validate signature
                response = ValidateSignature(response);

                //[OIO-IDWS-SOAP]
                //The header block MUST be included exactly once in responses to prior-received 
                //request messages. If the RelationshipType attribute is included it MUST be set
                //to the value http://www.w3.org/2005/03/addressing/reply.
                //In response messages, the value of this header block MUST be set to the value of
                //the < wsa:MessageID > header block of the prior-received message
                if (response.Headers.RelatesTo == null || messageId != response.Headers.RelatesTo)
                {
                    Logger.Instance.Error($"RelatesTo header block is required and its value ({response.Headers.RelatesTo}) MUST be set to the value of the <wsa:MessageID> header block ({messageId}) of the prior-received message.");
                    throw new MessageSecurityException("RelatesTo header block is required and its value MUST be set to the value of the <wsa:MessageID> header block of the prior-received message.");
                }

                //[OIO-IDWS-SOAP] <wsa:MessageID> is required
                if (response.Headers.MessageId == null)
                {
                    Logger.Instance.Error($"The <wsa:MessageID> header block MUST be included in the SOAP header.");
                    throw new MessageSecurityException("The <wsa:MessageID> header block MUST be included in the SOAP header.");
                }

                PerformReplayDetection(response, messageId);
            }

            return response;
        }

        private void PerformReplayDetection(Message response, UniqueId messageId)
        {
            var replayAttackCacheKey = response.Headers.MessageId.ToString();
            if (_distributedCache.DoesKeyExist(replayAttackCacheKey))
            {
                throw new MessageSecurityException("Replay attack detected. Response of message with id: " + messageId);
            }
            else
            {
                _distributedCache.Set(replayAttackCacheKey, DateTimeOffset.Now.AddMinutes(3));
            }
        }

        private Message ValidateSignature(Message response)
        {
            var doc = response.ToXmlDocument();

            Logger.Instance.Trace("Response from WSP being validated:\n" + doc.OuterXml);

            if (!XmlSignatureUtils.VerifySignature(doc, _federatedSecurityTokenParameters.ServerCertificate, 0, response.Version.Envelope))
            {
                Logger.Instance.Error($"Failed to validate (using the certificate '{_federatedSecurityTokenParameters.ServerCertificate.Thumbprint}') the SOAP signature received from WSP!");
                throw new MessageSecurityException("SOAP signature received from WSP does not validate!");
            }

            return doc.ToMessage(response);
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
