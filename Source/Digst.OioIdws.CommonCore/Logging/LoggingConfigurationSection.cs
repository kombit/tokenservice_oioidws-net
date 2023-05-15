using System;
using System.Configuration;

namespace Digst.OioIdws.CommonCore.Logging
{
    /// <summary>
    /// A configuration section that specifies what logger to use. Ported almost as-is from https://github.com/digst/OIOIDWS.Net
    /// </summary>
    public class LoggingConfigurationSection : ConfigurationSection
    {
        /// <summary>
        /// Specifies a <see cref="Type.AssemblyQualifiedName"/> of a custom implementation of the <see cref="ILogger"/> interface. <see cref="TraceLogger"/> is used if no custom logger has been specified.
        /// </summary>
        [System.Configuration.ConfigurationProperty("logger", IsRequired = false)]
        public string Logger
        {
            get { return (string) this["logger"]; }
            set { this["logger"] = value; }
        }
    }
}
