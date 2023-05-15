using Digst.OioIdws.CommonCore.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace Digst.OioIdws.CommonCore.Test
{
    [TestClass]
    public class LoggerAndLoggerFactoryTest
    {
        [TestMethod]
        public void GetInstance_CanReturnTestLoggerConfigured()
        {
            // Arrange
            var result = Logger.Instance;
            string message = Guid.NewGuid().ToString();

            // Act
            result.Trace(message);

            // Assert
            Assert.AreEqual(message, StubLogger.Message);
        }

        [TestMethod ]
        public void LoggerFactory_CanUseCustomLogger()
        {
            // Arrange
            StubLogger expectedLogger = new StubLogger();

            // Act and Assert
            LoggerFactory.SetLogger(expectedLogger);
            ILogger logger = LoggerFactory.CreateLogger();

            Assert.AreSame(expectedLogger, logger);

            LoggerFactory.SetLogger(null);
            logger = LoggerFactory.CreateLogger();

            Assert.AreNotSame(expectedLogger, logger);
        }
    }

    public class StubLogger : ILogger
    {
        public static string Message { get; private set; } = string.Empty;

        public void WriteCore(TraceEventType eventType, int eventId, object state, Exception? exception, Func<object, Exception?, string> formatter)
        {
            Message = state.ToString()!;
        }
    }
}