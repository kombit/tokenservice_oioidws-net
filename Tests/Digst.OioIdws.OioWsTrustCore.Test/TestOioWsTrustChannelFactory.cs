using Digst.OioIdws.OioWsTrustCore;
using Digst.OioIdws.OioWsTrustCore.ProtocolChannel;
using Digst.OioIdws.CommonCore;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Digst.OioIdws.OioWsTrustCore.Test
{
    internal class TestOioWsTrustChannelFactory : OioWsTrustChannelFactory
    {
        public TestOioWsTrustChannelFactory(IChannelFactory<IRequestChannel> innerFactory, StsTokenServiceConfiguration stsTokenServiceConfiguration, StsAuthenticationCase stsAuthenticationCase) 
            : base(innerFactory, stsTokenServiceConfiguration, stsAuthenticationCase)
        {
        }

        public IRequestChannel TestCreateChannel(EndpointAddress address, Uri via)
        {
            return OnCreateChannel(address, via);
        }
    }

    internal class TestChannelFactory : IChannelFactory<IRequestChannel>
    {
        public CommunicationState State => throw new NotImplementedException();

        public event EventHandler Closed;
        public event EventHandler Closing;
        public event EventHandler Faulted;
        public event EventHandler Opened;
        public event EventHandler Opening;

        public void Abort()
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginClose(AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginOpen(AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            throw new NotImplementedException();
        }

        public void Close(TimeSpan timeout)
        {
            throw new NotImplementedException();
        }

        public IRequestChannel CreateChannel(EndpointAddress to)
        {
            throw new NotImplementedException();
        }

        public IRequestChannel CreateChannel(EndpointAddress to, Uri via)
        {
            return new TestRequestChannel();
        }

        public void EndClose(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        public void EndOpen(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        public T GetProperty<T>() where T : class
        {
            throw new NotImplementedException();
        }

        public void Open()
        {
            throw new NotImplementedException();
        }

        public void Open(TimeSpan timeout)
        {
            throw new NotImplementedException();
        }
    }

    public class TestRequestChannel : IRequestChannel
    {
        public EndpointAddress RemoteAddress => throw new NotImplementedException();

        public Uri Via => throw new NotImplementedException();

        public CommunicationState State => throw new NotImplementedException();

        public event EventHandler Closed;
        public event EventHandler Closing;
        public event EventHandler Faulted;
        public event EventHandler Opened;
        public event EventHandler Opening;

        public void Abort()
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginClose(AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginOpen(AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginRequest(Message message, AsyncCallback callback, object state)
        {
            return Task<Message>.FromResult(message);
        }

        public IAsyncResult BeginRequest(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return Task<Message>.FromResult(message);
        }

        public void Close()
        {
            throw new NotImplementedException();
        }

        public void Close(TimeSpan timeout)
        {
            throw new NotImplementedException();
        }

        public void EndClose(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        public void EndOpen(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        public Message EndRequest(IAsyncResult result)
        {
            var test = result as Task<Message>;
            return test!.Result;
        }

        public T GetProperty<T>() where T : class
        {
            throw new NotImplementedException();
        }

        public void Open()
        {
            throw new NotImplementedException();
        }

        public void Open(TimeSpan timeout)
        {
            throw new NotImplementedException();
        }

        public Message Request(Message message)
        {
            throw new NotImplementedException();
        }

        // An example/test response from Data\Response.xml file
        public Message Request(Message message, TimeSpan timeout)
        {
            var messageXml = XDocument.Load(@"Data\Response.xml");
            var response = Message.CreateMessage(MessageVersion.Soap12WSAddressingAugust2004, "Test");
            response = messageXml.ToMessage(response);
            response.Headers.MessageId = message.Headers.MessageId;
            response.Headers.RelatesTo = new System.Xml.UniqueId(Guid.Parse("0cbeecb3-8fe4-40c2-837d-db77ffce3801"));
            return response;
        }
    }
}
