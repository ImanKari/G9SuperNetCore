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
        DisconnectedByProgram
    }
}