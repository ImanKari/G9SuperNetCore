namespace G9SuperNetCoreServer.Enums
{
    public enum DisconnectReason : byte
    {
        /// <summary>
        ///     Unknown reason
        /// </summary>
        Unknown,

        /// <summary>
        ///     When disconnected connection from client
        ///     Close connection
        /// </summary>
        DisconnectedFromClient,


        /// <summary>
        ///     When server stopped and close all session and account
        /// </summary>
        ServerStopped,


        /// <summary>
        ///     If Session Receive Request Over The Limit In Second
        ///     Limit specified in server configuration
        /// </summary>
        ReceiveRequestOverTheLimit
    }
}