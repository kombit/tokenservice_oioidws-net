using System.Diagnostics;
using Digst.OioIdws.CommonCore.Logging;
using log4net;
using log4net.Config;

namespace Digst.OioIdws.WscExampleCommon
{
    public class Log4NetLogger : ILogger
    {
        private static readonly ILog Logger = LogManager.GetLogger("OioIdws");

        public void WriteCore(TraceEventType eventType, int eventId, object state, Exception? exception, Func<object, Exception, string> formatter)
        {
            switch (eventType)
            {
                case TraceEventType.Critical:
                    Logger.Fatal(state, exception);
                    break;
                case TraceEventType.Error:
                    Logger.Error(state, exception);
                    break;
                case TraceEventType.Warning:
                    Logger.Warn(state, exception);
                    break;
                case TraceEventType.Verbose:
                    Logger.Debug(state, exception);
                    break;
                default:
                    Logger.Info(state, exception);
                    break;
            }
        }

        public static void Initialize()
        {
            XmlConfigurator.Configure();
        }
    }
}
