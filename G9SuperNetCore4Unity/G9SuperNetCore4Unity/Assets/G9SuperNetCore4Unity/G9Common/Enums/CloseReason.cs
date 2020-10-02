namespace G9Common.Enums
{
    public enum CloseReason : byte
    {
        TimeOut,

        DisconnectFromClient,

        DisconnectFromServer,

        RejectFromServer_MaxConnectionLimit,

        RejectFromServer_Unknown
    }
}