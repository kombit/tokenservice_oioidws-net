using System;
using System.Configuration;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;

namespace Digst.OioIdws.Soap.Bindings
{
    /// <summary>
    /// Class used for making configuration possible of the <see cref="SoapBinding"/>
    /// </summary>
    public class SoapBindingElement : StandardBindingElement
    {
        protected override void OnApplyConfiguration(Binding binding)
        {
            var SoapBinding = (SoapBinding) binding;
            SoapBinding.UseHttps = UseHttps;
            SoapBinding.UseSoap12 = UseSoap12;
            SoapBinding.MaxReceivedMessageSize = MaxReceivedMessageSize;
        }

        protected override Type BindingElementType
        {
            get { return typeof (SoapBinding); }
        }

        /// <summary>
        /// Specifies max size of message received in bytes. If not set, default value on <see cref="TransportBindingElement.MaxReceivedMessageSize"/> are used.
        /// </summary>
        [ConfigurationProperty("maxReceivedMessageSize", IsRequired = false, DefaultValue = null)]
        public int? MaxReceivedMessageSize
        {
            get
            {
                return (int?)this["maxReceivedMessageSize"];
            }
            set
            {
                this["maxReceivedMessageSize"] = value;
            }
        }

        /// <summary>
        /// Specifies whether transport layer security is needed in the http communication.
        /// </summary>
        [ConfigurationProperty("useHttps", IsRequired = false, DefaultValue = true)]
        public bool UseHttps
        {
            get
            {
                return (bool)this["useHttps"];
            }
            set
            {
                this["useHttps"] = value;
            }
        }

        /// <summary>
        /// Specifies binding SOAP version.
        /// </summary>
        [ConfigurationProperty("useSoap12", IsRequired = false, DefaultValue = true)]
        public bool UseSoap12
        {
            get
            {
                return (bool)this["useSoap12"];
            }
            set
            {
                this["useSoap12"] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                var properties = base.Properties;
                properties.Add(new ConfigurationProperty("useHttps", typeof (bool), true));
                properties.Add(new ConfigurationProperty("useSoap12", typeof(bool), true));
                properties.Add(new ConfigurationProperty("maxReceivedMessageSize", typeof (int?), null));
                return properties;
            }
        }

        protected override void InitializeFrom(Binding binding)
        {
            base.InitializeFrom(binding);
            var SoapBinding = (SoapBinding)binding;
            UseHttps = SoapBinding.UseHttps;
            UseSoap12 = SoapBinding.UseSoap12;
            MaxReceivedMessageSize = SoapBinding.MaxReceivedMessageSize;
        }
    }
}