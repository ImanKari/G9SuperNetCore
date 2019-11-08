namespace G9Common.Resource
{
    public struct LogMessage
    {
        public const string ArgumentXNotCorrect = "ArgumentXNotCorrect";
        public const string AuthorizationFail = "The problem in the authorization process";
        public const string AuthorizationSuccess = "Authorization approved correctly";
        public const string Body = "Body";
        public const string CantStopStoppedServer = "CantStopStoppedServer";
        public const string CertificateDoseNotHavePrivateKey = "The certificate does not have a private key";
        public const string CertificateIsNotExportable =
            "The certificate is not exportable. Use 'X509KeyStorageFlags.Exportable' when creating 'X509Certificate2'.";
        public const string ChangeBodyMultiPacketNotFill = "Envelopes are not a complete request for body modification";
        public const string ClientPing = "ClientPing";
        public const string ClientPingDateTime = "ClientPingDateTime";
        public const string ClientSessionIdentity = "ClientSessionIdentity";
        public const string ClientWithOutCertificate =
            "The server is implemented as SSL. But the client is connected to it without a certificate.";
        public const string CloseReason = "CloseReason";
        public const string Command = "Command";
        public const string CommandEcho = "CommandEcho";
        public const string CommandLengthError = "CommandLengthError";
        public const string CommandName = "CommandName";
        public const string CommandSendType = "CommandSendType";
        public const string CommandSendTypeNotCorrect = "CommandSendTypeNotCorrect";
        public const string CommanTestSendReceive = "CommanTestSendReceive";
        public const string CreateAndInitializeClient = "CreateAndInitializeClient";
        public const string DataLength = "DataLength";
        public const string ErrorInRunningCommand = "ErrorInRunningCommand";
        public const string ExtraPassIsNot32Char = "The hashed password must be 32 characters";
        public const string FailClientReceive = "FailClientReceive";
        public const string FailClinetConnection = "FailClinetConnection";
        public const string FailedOperation = "FailedOperation";
        public const string FailGeneratePacket = "FailGeneratePacket";
        public const string FailRequestSendData = "FailRequestSendData";
        public const string FailSendComandByName = "FailSendComandByName";
        public const string FailSendComandByNameAsync = "FailSendComandByNameAsync";
        public const string IpAddress = "IpAddress";
        public const string LastCommandUsed = "LastCommandUsed";
        public const string LastCommandUsedDateTime = "LastCommandUsedDateTime";
        public const string LengthEntered = "LengthEntered";
        public const string OnSessionClose = "OnSessionClose";
        public const string PacketNumberIsGreater = "Packet number is greater than total packets!";
        public const string PacketRequestId = "PacketRequestId";
        public const string PacketType = "PacketType";
        public const string Port = "Port";
        public const string Reason = "Reason";
        public const string ReceiveClientFromServer = "ReceiveClientFromServer";
        public const string ReceiveData = "ReceiveData";
        public const string ReceivedUnhandledCommand = "ReceivedUnhandledCommand";
        public const string RequestSendData = "RequestSendData";
        public const string RunningCommand = "RunningCommand";
        public const string SendTypeWithFunction = "SendTypeWithFunction";
        public const string ServerWithOutCertificate =
            "The client is implemented as SSL. But the server without a certificate.";
        public const string StandardLength = "StandardLength";
        public const string StartClientConnection = "StartClientConnection";
        public const string StopServer = "StopServer";
        public const string SuccessClientConnection = "SuccessClientConnection";
        public const string SuccessfulOperation = "SuccessfulOperation";
        public const string SuccessRequestSendData = "SuccessRequestSendData";
        public const string SuccessUnpackingReceiveData = "SuccessUnpackingReceiveData";
        public const string TestNumber = "TestNumber";
        public const string UnhandledCommand = "UnhandledCommand";
    }
}