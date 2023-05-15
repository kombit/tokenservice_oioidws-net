using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Digst.OioIdws.CommonCore.Test")]
namespace Digst.OioIdws.CommonCore.Logging
{
    /// <summary>
    /// A factory that creates logger instances per the configuration in the app.config file
    /// </summary>
    public static class LoggerFactory
    {
        private static ILogger? _customLogger;

        internal static ILogger CreateLogger()
        {
            if (_customLogger != null)
            {
                return _customLogger;
            }

            // Retrieve Configuration
            var config = (LoggingConfigurationSection)System.Configuration.ConfigurationManager.GetSection("oioIdwsLoggingConfiguration");

            if (config != null && !string.IsNullOrEmpty(config.Logger))
            {
                try
                {
                    var t = Type.GetType(config.Logger);
                    if (t != null)
                    {
                        return (ILogger)Activator.CreateInstance(t);
                    }

                    throw new NotSupportedException($"The type {config.Logger} is not available for the logging. Please check the type name and assembly");
                }
                catch (Exception e)
                {
                    new TraceLogger().Fatal("Could not instantiate the configured logger. Message: " + e.Message);
                    throw;
                }
            }

            return new TraceLogger();
        }

        /// <summary>
        /// Set a custom logger. This method must be called *before* the first call to Logger.Instance.
        /// </summary>
        /// <param name="logger">A custom logger. A null value resets the inner _customLogger object.</param>
        public static void SetLogger(ILogger? logger)
        {
            _customLogger = logger;
        }
    }
}
