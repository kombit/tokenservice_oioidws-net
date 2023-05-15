using System;
using System.Diagnostics;

namespace Digst.OioIdws.CommonCore.Logging
{
    /// <summary>
    /// A built-in logger that writes data to TraceSource
    /// </summary>
    internal class TraceLogger : ILogger
    {
        /// <summary>
        /// The source to use for logging
        /// </summary>
        private static readonly TraceSource Source = new TraceSource("Digst.OioIdws");

        public void WriteCore(TraceEventType eventType, int eventId, object state, Exception? exception, Func<object, Exception?, string> formatter)
        {
            if (Source.Switch.ShouldTrace(eventType))
            {
                Source.TraceEvent(eventType, eventId, formatter(state, exception));
            }
        }
    }
}
