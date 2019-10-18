namespace G9SuperNetCoreServer.Enums
{
    public enum ServerErrorReason : byte
    {
        /// <summary>
        ///     Unknown reason
        /// </summary>
        Unknown,

        /// <summary>
        ///     Error when start server fail
        /// </summary>
        ErrorInStartServer,

        /// <summary>
        ///     Error when client try to connected server
        /// </summary>
        ClientConnectedError,

        /// <summary>
        ///     Error when receive data from client
        /// </summary>
        ErrorReceiveDataFromClient,

        /// <summary>
        ///     Error when send data to client
        /// </summary>
        ErrorSendDataToClient,

        /// <summary>
        ///     Error when data preparation for send to client
        /// </summary>
        ErrorReadyToSendDataToClient,

        /// <summary>
        ///     Error when server is stopped and receive request again for stop
        /// </summary>
        ServerIsStoppedAndReceiveRequestForStop,

        /// <summary>
        /// Error when stop server fail
        /// </summary>
        ErrorInStopServer
    }
}