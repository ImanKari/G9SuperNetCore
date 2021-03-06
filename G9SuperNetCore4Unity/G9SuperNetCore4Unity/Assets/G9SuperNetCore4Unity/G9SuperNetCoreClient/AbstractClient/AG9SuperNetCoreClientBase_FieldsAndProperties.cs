using System;
using System.Net.Sockets;
using System.Threading;
using G9Common.CommandHandler;
using G9Common.HelperClass;
using G9Common.Interface;
using G9Common.PacketManagement;
using G9Common.ServerClient;
using G9SuperNetCoreClient.Abstract;
using G9SuperNetCoreClient.Config;
using G9SuperNetCoreClient.Helper;

namespace G9SuperNetCoreClient.AbstractClient
{
    public abstract partial class AG9SuperNetCoreClientBase<TAccount, TSession> : AG9ServerClientCommon<TAccount>
        where TAccount : AClientAccount<TSession>, new()
        where TSession : AClientSession, new()
    {
        /// <summary>
        ///     Access to command handler
        /// </summary>
        private readonly G9CommandHandler<TAccount> _commandHandler;

        /// <summary>
        ///     ManualResetEvent instances signal completion.
        /// </summary>
        private readonly ManualResetEvent _connectDone = new ManualResetEvent(false);

        /// <summary>
        ///     Specified logging system
        /// </summary>
        private readonly IG9Logging _logging;

        /// <summary>
        ///     ManualResetEvent instances signal completion.
        /// </summary>
        private readonly ManualResetEvent _sendDone = new ManualResetEvent(false);

        /// <summary>
        ///     Access to client configuration
        /// </summary>
        public readonly G9ClientConfig Configuration;

        /// <summary>
        ///     Socket for client connection
        /// </summary>
        private Socket _clientSocket;

        /// <summary>
        ///     Field used for save and access to account utilities
        /// </summary>
        private G9AccountUtilities<TAccount, G9ClientAccountHandler, G9ClientSessionHandler>
            _mainAccountUtilities;

        /// <summary>
        ///     Access to packet management
        /// </summary>
        private G9PacketManagement _packetManagement;

        /// <summary>
        ///     Specified packet size
        ///     Diff between ssl mode and normal mode
        /// </summary>
        private ushort _packetSize;

        /// <summary>
        ///     <para>Specified reconnect is enable or no</para>
        ///     <para>If is enable reject other reconnect request</para>
        /// </summary>
        private bool _reconnectModeEnable;

        /// <summary>
        ///     Specify count for try reconnect
        /// </summary>
        private sbyte _reconnectTryCount;

        /// <summary>
        ///     State object handle client task
        /// </summary>
        private G9SuperNetCoreStateObjectClient _stateObject;

        /// <summary>
        ///     <para>If is enable reject other request for call OnUnableToConnectHandler</para>
        /// </summary>
        private bool _unableToConnectFlag;

        /// <summary>
        ///     Specified client connected date time
        /// </summary>
        public DateTime ClientConnectedDateTime;

        /// <summary>
        ///     Access to main account
        /// </summary>
        public TAccount MainAccount => _mainAccountUtilities.Account;

        /// <summary>
        ///     Specified client is Authorization
        /// </summary>
        public bool IsAuthorization => _mainAccountUtilities?.Account?.Session?.IsAuthorization ?? false;

        #region Send And Receive Bytes

        /// <summary>
        ///     Access to total send bytes
        /// </summary>
        public ulong TotalSendBytes { private set; get; }

        /// <summary>
        ///     Access to total receive bytes
        /// </summary>
        public ulong TotalReceiveBytes { private set; get; }

        /// <summary>
        ///     Access to total send packet count
        /// </summary>
        public uint TotalSendPacket { private set; get; }

        /// <summary>
        ///     Access to total receive packet count
        /// </summary>
        public uint TotalReceivePacket { private set; get; }

        #endregion

        #region Ssl Utilities

        /// <summary>
        ///     Access to encrypt decrypt object
        /// </summary>
        private G9EncryptAndDecryptDataWithCertificate _encryptAndDecryptDataWithCertificate;

        /// <summary>
        ///     Specified enable ssl (Secure) connection for client socket
        ///     Automatic set by server
        /// </summary>
        public bool EnableSslConnection { private set; get; }

        /// <summary>
        ///     Field for save private key ssl connection certificate
        /// </summary>
        private readonly string _privateKey;

        /// <summary>
        ///     Field for save client unique identity string
        /// </summary>
        private readonly string _clientIdentity;

        #endregion
    }
}