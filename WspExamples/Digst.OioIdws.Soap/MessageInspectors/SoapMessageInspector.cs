using System;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using Digst.OioIdws.Common;
using Digst.OioIdws.Common.Constants;
using Digst.OioIdws.Common.Logging;

namespace Digst.OioIdws.Soap.MessageInspectors
{
    /// <summary>
    /// This message inspector adds the liberty framework header to the the SOAP message.
    /// </summary>
    public class SoapMessageInspector : IClientMessageInspector, IDispatchMessageInspector
    {
        #region Message Inspector of the Service

        /// <summary>
        /// This method is called on the server when a request is received from the client.
        /// </summary>
        public object AfterReceiveRequest(ref Message request,
               IClientChannel channel, InstanceContext instanceContext)
        {
            Logger.Instance.Trace("Validating WS-Addressing headers on request from WSC.");
            ValidateWsAddressingHeadersCommon(request);
            Logger.Instance.Trace("WS-Addressing headers validated fine on request from WSC.");

            // Validate and remove the liberty "Framework" to pass the SOAP validation
            var libertyFrameworkHeaderIndex = request.Headers.FindHeader("Framework", "urn:liberty:sb:2006-08");
            if(libertyFrameworkHeaderIndex >= 0)
            {
                request.Headers.RemoveAt(libertyFrameworkHeaderIndex);
            }
            else
            {
                var errorMessage = "Liberty Framework header was not present";
                Logger.Instance.Error(errorMessage);
                throw new FaultException(SoapFaults.CreateClientSoapFault(errorMessage));
            }
            
            return null;
        }

        /// <summary>
        /// This method is called after processing a method on the server side and just
        /// before sending the response to the client.
        /// </summary>
        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            // WCF does not automatically add a MessageID on responses.
            Logger.Instance.Trace("Adding MessageID header on response from WSP.");
            var header = new MessageHeader<string>("urn:uuid:" + Guid.NewGuid());
            reply.Headers.Add(header.GetUntypedHeader(WsAdressing.WsAdressingMessageId, WsAdressing.WsAdressing10NameSpace));
            Logger.Instance.Trace("Added MessageID header on response from WSP.");
        }

        #endregion

        #region Message Inspector of the Consumer

        /// <summary>
        /// This method will be called from the client side just before any method is called.
        /// </summary>
        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            return null;
        }

        /// <summary>
        /// This method will be called after completion of a request to the server.
        /// </summary>
        /// <param name="reply"></param>
        /// <param name="correlationState"></param>
        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
            Logger.Instance.Trace("Validating WS-Addressing headers on response from WSP.");
            ValidateWsAddressingHeadersReceivedFromWsp(reply);
            Logger.Instance.Trace("WS-Addressing headers validated fine on response from WSP.");
        }

        #endregion

        /// <summary>
        /// WS-Addressing headers are not required in responses according to the WS-Addressing specification. E.g. does WCF not automatically include a MessageId in responses.
        /// Therefore this extra validation has been made on top of WCF in order to always ensure that the WS-Addressing headers specified by [OIO IDWS SOAP 1.1] are present.
        /// </summary>
        /// <param name="message">The message in which the headers must be.</param>
        private static void ValidateWsAddressingHeadersCommon(Message message)
        {
            var messageIdHeader =
                message.Headers.SingleOrDefault(
                    x =>
                        WsAdressing.WsAdressingMessageId == x.Name &&
                        WsAdressing.WsAdressing10NameSpace == x.Namespace);

            if (messageIdHeader == null)
            {
                const string errorMessage = "WS-Addressing MessageID header was not present";
                Logger.Instance.Error(errorMessage);
                SoapFaults.CreateClientSoapFault(errorMessage);
            }
        }

        private static void ValidateWsAddressingHeadersReceivedFromWsp(Message message)
        {
            // First do the client validation which is common for both server and client.
            ValidateWsAddressingHeadersCommon(message);

            var relatesToHeader =
                message.Headers.SingleOrDefault(
                    x =>
                        WsAdressing.WsAdressingRelatesTo == x.Name &&
                        WsAdressing.WsAdressing10NameSpace == x.Namespace);

            if (relatesToHeader == null)
            {
                var errorMessage = "WS-Addressing RelatesTo header was not present";
                Logger.Instance.Error(errorMessage);
                throw new FaultException(SoapFaults.CreateClientSoapFault(errorMessage));
            }
        }
    }
}
