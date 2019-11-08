namespace G9Common.HelperClass
{
    public struct G9ServerCommandMessage
    {
        /// <summary>
        /// Server reject connection
        /// Reason limit max connection
        /// </summary>
        public const string REJECT_MAX_CONNECTION = "REJECT_MAX_CONNECTION";

        /// <summary>
        /// Server reject connection
        /// Reason unknown
        /// </summary>
        public const string REJECT_UNKNOWN = "REJECT_UNKNOWN";

        /// <summary>
        /// Server reject connection
        /// Reason server error
        /// </summary>
        public const string REJECT_SERVER_ERROR = "REJECT_SERVER_ERROR";
    }
}