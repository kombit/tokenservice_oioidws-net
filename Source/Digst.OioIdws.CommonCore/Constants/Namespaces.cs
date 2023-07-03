namespace Digst.OioIdws.CommonCore.Constants
{
    /// <summary>
    /// Defines constants of all namespaces used by WsTrust and OioIdws messages
    /// </summary>
    public static class Namespaces
    {
        // Namespace prefixes
        public const string S11Prefix = "S11";
        public const string S12Prefix = "s";
        public const string WsaPrefix = "wsa";
        public const string WsuPrefix = "wsu";
        public const string WssePrefix = "wsse";
        public const string WspPrefix = "wsp";
        public const string Wst13Prefix = "wst";

        public const string SignatureNamespace = "http://www.w3.org/2000/09/xmldsig#";
        public const string WsaNamespace = "http://www.w3.org/2005/08/addressing";
        public const string S11Namespace = "http://schemas.xmlsoap.org/soap/envelope/";
        public const string S12Namespace = "http://www.w3.org/2003/05/soap-envelope";
        //public const string S12Namespace = "http://schemas.xmlsoap.org/soap/envelope/";
        public const string Wsp12Namespace = "http://schemas.xmlsoap.org/ws/2004/09/policy"; // Corresponds to WS-Policy 1.2.
        public const string WspNamespace = "http://schemas.xmlsoap.org/ws/2002/12/policy"; // Corresponds to WS-SecurityPolicy
        public const string Wst13Namespace = "http://docs.oasis-open.org/ws-sx/ws-trust/200512";
        public const string Wst14Namespace = "http://docs.oasis-open.org/ws-sx/ws-trust/200802";
        public const string WsuNamespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd";
        public const string Wsse10Namespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd";
        public const string Wsse11Namespace = "http://docs.oasis-open.org/wss/oasis-wss-wssecurity-secext-1.1.xsd";
        public const string VsDebuggerNamespace = "http://schemas.microsoft.com/vstudio/diagnostics/servicemodelsink";
        public const string WcfDiagnosticsNamespace = "http://schemas.microsoft.com/2004/09/ServiceModel/Diagnostics";
    }
}

