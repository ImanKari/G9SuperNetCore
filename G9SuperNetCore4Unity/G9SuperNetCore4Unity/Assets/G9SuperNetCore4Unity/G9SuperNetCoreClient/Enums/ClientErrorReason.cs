namespace G9SuperNetCoreClient.Enums
{
    public enum ClientErrorReason : byte
    {
        /// <summary>
        ///     Unknown reason
        /// </summary>
        Unknown,

        /// <summary>
        ///     Error when client try to connected server
        /// </summary>
        ClientConnectedError,

        /// <summary>
        ///     Error when client receive data
        /// </summary>
        ErrorInReceiveData,

        /// <summary>
        ///     Error when send data to server
        /// </summary>
        ErrorSendDataToServer,

        /// <summary>
        ///     Error when data preparation for send to server
        /// </summary>
        ErrorReadyToSendDataToServer,

        /// <summary>
        ///     Error when cline is disconnect and receive request again for disconnect
        /// </summary>
        ClientDisconnectedAndReceiveRequestForDisconnect,

        /// <summary>
        ///     Error when client has problem in auth
        /// </summary>
        ErrorInAuthorization
    }
}