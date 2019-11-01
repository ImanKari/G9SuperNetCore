namespace G9Common.Enums
{
    /// <summary>
    ///     Specified packet data type
    /// </summary>
    public enum G9PacketDataType : byte
    {
        /// <summary>
        ///     Specified packet is standard command
        /// </summary>
        StandardCommand,

        /// <summary>
        ///     Specified Packet is exception or error
        /// </summary>
        ClientExceptionOrError,

        /// <summary>
        ///     Specified packet is authorization
        /// </summary>
        Authorization
    }
}