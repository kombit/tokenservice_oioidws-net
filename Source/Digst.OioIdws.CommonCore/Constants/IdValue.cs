namespace Digst.OioIdws.CommonCore.Constants
{
    /// <summary>
    /// Defines constants that hold ids of all mandatory SOAP headers and elements
    /// </summary>
    public static class IdValue
    {
        public const string ActionIdValue = "action";
        public const string MessageIdIdValue = "msgid";
        public const string RelatesToIdValue = "rtid";
        public const string ToIdValue = "to";
        public const string TimeStampIdValue = "sec-ts";
        public const string BinarySecurityTokenIdValue = "sec-binsectoken";
        public const string BodyIdValue = "body";
        public const string ReplyToIdValue = "replyto";
        public const string SecurityTokenReferenceElementIdValue = "sec-str";
        public const string LibertyFrameworkIdValue = "lib";
        // Always set to encryptedassertion by KOMBIT STS. If this value becomes dynamic then the KeyIdentfier value must be compared to the id of the encrypted assertion.
        public const string EncryptedAssertionId = "encryptedassertion";
    }
}
