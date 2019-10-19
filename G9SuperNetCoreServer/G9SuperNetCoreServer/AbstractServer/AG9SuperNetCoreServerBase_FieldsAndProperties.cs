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
        ///     Save total send bytes
        /// </summary>
        private ulong _totalSendBytes;

        /// <summary>
        ///     Access to total send bytes
        /// </summary>
        // ReSharper disable once ConvertToAutoProperty
        public ulong TotalSendBytes => _totalSendBytes;

        /// <summary>
        ///     Save total receive bytes
        /// </summary>
        private ulong _totalReceiveBytes;

        /// <summary>
        ///     Access to total receive bytes
        /// </summary>
        // ReSharper disable once ConvertToAutoProperty
        public ulong TotalReceiveBytes => _totalReceiveBytes;

        #endregion
    }
}