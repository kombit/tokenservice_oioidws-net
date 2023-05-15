using System;
using System.Collections.Generic;
using System.Text;

namespace Digst.OioIdws.CommonCore.Constants
{
    /// <summary>
    /// Defines constants of keys of key-value pair requirements that are passed around
    /// </summary>
    public static class SecurityTokenRequirementConstants
    {
        public const string Namespace = "http://schemas.microsoft.com/ws/2006/05/servicemodel/securitytokenrequirement";
        public const string IssuedSecurityTokenParametersProperty = Namespace + "/IssuedSecurityTokenParameters";
        public const string SecurityAlgorithmSuiteProperty = Namespace + "/SecurityAlgorithmSuite";
        public const string SecurityBindingElementProperty = Namespace + "/SecurityBindingElement";
        public const string TargetAddressProperty = Namespace + "/TargetAddress";
        public const string CvrProperty = Namespace + "/Cvr";
        public const string LifetimeProperty = Namespace + "/Lifetime";
        public const string OnBehalfOfProperty = Namespace + "/OnBehalfOf";
        public const string ProxyOnBehalfOfProperty = Namespace + "/ProxyOnBehalfOf";
        public const string IssuerProperty = Namespace + "/IssuerEntityId";
    }
}
