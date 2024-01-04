using Digst.OioIdws.Wsp.Wsdl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace Digst.OioIdws.WspSvcExample
{
    [ServiceContract]
    //[WsdlExportExtension(Token = TokenType.HolderOfKey)] 
    //[WsdlExportExtension(Token = TokenType.Bearer)]
    [WsdlExportExtension] // Default value -> (Token = TokenType.HolderOfKey)
    public interface IHelloWorld
    {
        [OperationContract(ProtectionLevel = ProtectionLevel.Sign)]
        string HelloSign(string name);

        /// <summary>
        /// Soap faults are not encrypted when predefined as a <see cref="FaultContractAttribute"/>
        /// </summary>
        [OperationContract(ProtectionLevel = ProtectionLevel.Sign)]
        [FaultContract(typeof(string))]
        string HelloSignError(string name);
    }
}
