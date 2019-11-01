namespace G9Common.Enums
{
    public enum G9ReservedCommandName
    {
        /// <summary>
        ///     Echo command
        ///     Send data from client to server and echo data from server to client
        /// </summary>
        G9EchoCommand,

        /// <summary>
        ///     Test Send Receive Command
        ///     Test infinity data send and receive between server and clients
        /// </summary>
        G9TestSendReceive,

        /// <summary>
        ///     Ping Command
        ///     Automatically used from server when need get ping from client
        /// </summary>
        G9PingCommand,

        /// <summary>
        ///     Authorization command
        ///     Automatically used from server
        ///     Set certificate and requirement for auth
        /// </summary>
        G9Authorization
    }
}