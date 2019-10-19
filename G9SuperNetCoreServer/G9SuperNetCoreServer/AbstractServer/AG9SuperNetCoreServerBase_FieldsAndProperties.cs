using System;
using System.Net.Sockets;
using System.Threading;
using G9Common.PacketManagement;
using G9SuperNetCoreServer.Abstarct;
using G9SuperNetCoreServer.Core;

namespace G9SuperNetCoreServer.AbstractServer
{
    public abstract partial class AG9SuperNetCoreServerBase<TAccount, TSession>
        where TAccount : AServerAccount<TSession>, new()
        where TSession : AServerSession, new()
    {
        /// <summary>
        ///     Access to core
        /// </summary>
        private readonly G9Core<TAccount, TSession> _core;

        /// <summary>
        ///     Use thread signal.
        /// </summary>
        private readonly ManualResetEvent _listenerAcceptManualResetEvent = new ManualResetEvent(false);

        /// <summary>
        ///     Access to packet management
        /// </summary>
        private readonly G9PacketManagement _packetManagement;

        /// <summary>
        ///     Specify main socket listener for server
        /// </summary>
        private Socket _mainSocketListener;

        /// <summary>
        ///     Specified server start date time
        /// </summary>
        public DateTime ServerStartDateTime;

        /// <summary>
        ///     Specified Number Of Session Connect From Start Server to current time
        /// </summary>
        public uint NumberOfSessionFromStartServer { private set; get; }

        /// <summary>
        ///     Specified Number Of Current Session Connected to server
        /// </summary>
        public uint NumberOfCurrentSession { private set; get; }

        /// <summary>
        ///     Specify enable command test send and receive for all clients
        /// </summary>
        public bool EnableCommandTestSendReceiveAllClients { private set; get; }

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
    }
}