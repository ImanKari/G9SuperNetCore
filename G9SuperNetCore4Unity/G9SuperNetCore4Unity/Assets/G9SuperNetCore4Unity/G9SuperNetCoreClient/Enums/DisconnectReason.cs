namespace G9SuperNetCoreClient.Enums
{
    public enum DisconnectReason : byte
    {
        /// <summary>
        ///     Unknown reason
        /// </summary>
        Unknown,

        /// <summary>
        ///     When disconnected from server
        /// </summary>
        DisconnectedFromServer,

        /// <summary>
        ///     When client disconnected with program (used method disconnect)
        /// </summary>
        DisconnectedByProgram,

        #region Authorization reson - Shared between client and server

        /// <summary>
        ///     When client with ssl connected to server without ssl
        /// </summary>
        AuthorizationFailClientIsSslButServerWithoutSsl = 249,

        /// <summary>
        ///     When client without ssl connected to ssl server
        /// </summary>
        AuthorizationFailServerIsSslButClientWithoutSsl = 250,

        /// <summary>
        ///     When client receive certificate but private key is empty and can't read certificate
        /// </summary>
        AuthorizationFailPrivateKeyIsEmpty = 251,

        /// <summary>
        ///     When client receive certificate but private key is not correct and can't read certificate
        /// </summary>
        AuthorizationFailPrivateKeyNotCorrect = 252,

        /// <summary>
        ///     When client receive certificate but certificate is damage and can't read it
        /// </summary>
        AuthorizationFailCertificateIsDamage = 253,

        /// <summary>
        ///     When client receive certificate but can't use certificate | Reason: unknown
        /// </summary>
        AuthorizationFailUnknownError = 254,

        /// <summary>
        ///     <para>This item is not used for 'DisconnectReason'.</para>
        ///     <para>Only for reservation number 255</para>
        /// </summary>
        AuthorizationIsSuccess = 255

        #endregion
    }
}