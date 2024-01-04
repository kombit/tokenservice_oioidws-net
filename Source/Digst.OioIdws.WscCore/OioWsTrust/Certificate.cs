using System;
using System.Collections.Generic;
using System.Configuration;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Digst.OioIdws.WscCore.OioWsTrust
{
    public class Certificate : ConfigurationElement
    {
        /// <summary>
        /// Only values defined in the <see cref="StoreLocation"/> enum can be specified.
        /// </summary>
        [ConfigurationProperty("storeLocation")]
        public StoreLocation StoreLocation
        {
            get
            {
                return (StoreLocation)this["storeLocation"];
            }
            set
            {
                this["storeLocation"] = value;
            }
        }

        /// <summary>
        /// Only values defined in the <see cref="StoreName"/> enum can be specified.
        /// </summary>
        [ConfigurationProperty("storeName")]
        public StoreName StoreName
        {
            get
            {
                return (StoreName)this["storeName"];
            }
            set
            {
                this["storeName"] = value;
            }
        }

        /// <summary>
        /// Only values defined in the <see cref="X509FindType"/> enum can be specified
        /// </summary>
        [ConfigurationProperty("x509FindType")]
        public X509FindType X509FindType
        {
            get
            {
                return (X509FindType)this["x509FindType"];
            }
            set
            {
                this["x509FindType"] = value;
            }
        }

        /// <summary>
        /// A value representing the type defined in <see cref="X509FindType"/>
        /// </summary>
        [ConfigurationProperty("findValue")]
        public string FindValue
        {
            get
            {
                return (string)this["findValue"];
            }
            set
            {
                this["findValue"] = value;
            }
        }

        /// <summary>
        /// A value representing that the certificate is loaded from the file system instead of Windows store
        /// </summary>
        [ConfigurationProperty("fromFileSystem")]
        public bool FromFileSystem
        {
            get
            {
                return (bool)this["fromFileSystem"];
            }
            set
            {
                this["fromFileSystem"] = value;
            }
        }

        /// <summary>
        /// A value representing the file path of the certificate/>
        /// </summary>
        [ConfigurationProperty("filePath")]
        public string FilePath
        {
            get
            {
                return (string)this["filePath"];
            }
            set
            {
                this["filePath"] = value;
            }
        }

        /// <summary>
        /// A value representing the certificate password/>
        /// </summary>
        [ConfigurationProperty("password")]
        public string Password
        {
            get
            {
                return (string)this["password"];
            }
            set
            {
                this["password"] = value;
            }
        }
    }
}
