using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Titanium.Web.Proxy;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Models;

namespace Digst.OioIdws.IntegrationTests
{
    internal static class ProxyServerHelper
    {
        public static void ExecuteWithProxy(Action action)
        {
            var proxyServer = new ProxyServer();
            try
            {
                var explicitEndPoint = new ExplicitProxyEndPoint(IPAddress.Any, 8000);
                proxyServer.AddEndPoint(explicitEndPoint);
                proxyServer.Start();
                proxyServer.SetAsSystemHttpsProxy(explicitEndPoint);
                action();
            }
            finally
            {
                proxyServer.Stop();
                proxyServer.Dispose();
            }
        }

        public static void ExecuteWithBeforeResponse(Action action, AsyncEventHandler<SessionEventArgs> onBeforeAction)
        {
            var proxyServer = new ProxyServer();
            try
            {
                var explicitEndPoint = new ExplicitProxyEndPoint(IPAddress.Any, 8000);
                proxyServer.AddEndPoint(explicitEndPoint);

                proxyServer.BeforeResponse += onBeforeAction;

                proxyServer.Start();
                proxyServer.SetAsSystemHttpsProxy(explicitEndPoint);
                action();
            }
            finally
            {
                proxyServer.BeforeResponse -= onBeforeAction;
                proxyServer.Stop();
                proxyServer.Dispose();
            }
        }

        public static void ExecuteWithBeforeRequest(Action action, AsyncEventHandler<SessionEventArgs> onBeforeAction)
        {
            var proxyServer = new ProxyServer();
            try
            {
                var explicitEndPoint = new ExplicitProxyEndPoint(IPAddress.Any, 8000);
                proxyServer.AddEndPoint(explicitEndPoint);

                proxyServer.BeforeRequest += onBeforeAction;

                proxyServer.Start();
                proxyServer.SetAsSystemHttpsProxy(explicitEndPoint);
                action();
            }
            finally
            {
                proxyServer.BeforeRequest -= onBeforeAction;
                proxyServer.Stop();
                proxyServer.Dispose();
            }
        }
    }
}
