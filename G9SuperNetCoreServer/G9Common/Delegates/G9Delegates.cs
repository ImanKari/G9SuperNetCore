using System;
using G9Common.Abstract;
using G9Common.Packet;

namespace G9Common.Delegates
{
    /// <summary>
    ///     Delegate management
    /// </summary>
    public class G9Delegates<TAccount, TStopReason, TErrorReason, TDisconnectReason>
        where TAccount : AAccount, new()
        where TStopReason : struct, IConvertible
        where TErrorReason : struct, IConvertible
        where TDisconnectReason : struct, IConvertible
    {
        /// <summary>
        ///     Delegate for error
        /// </summary>
        /// <param name="exceptionError">Exception of error</param>
        /// <param name="stopReason">Reason of error</param>
        public delegate void Error(Exception exceptionError, TErrorReason stopReason);

        /// <summary>
        ///     Delegate for start
        /// </summary>
        public delegate void Start();

        /// <summary>
        ///     Delegate for stop
        /// </summary>
        /// <param name="stopReason">Reason of stop</param>
        public delegate void Stop(TStopReason stopReason);

        /// <summary>
        ///     Delegate for Unhandled commands
        /// </summary>
        /// <param name="packet">Received packet data</param>
        /// <param name="account">account send command</param>
        public delegate void Unhandled(G9SendAndReceivePacket packet, TAccount account);

        /// <summary>
        ///     Delegate for connected
        /// </summary>
        /// <param name="account">Connected account</param>
        public delegate void Connected(TAccount account);

        /// <summary>
        ///     Delegate for disconnected
        /// </summary>
        /// <param name="stopReason">Reason of disconnected</param>
        public delegate void Disconnected(TAccount account, TDisconnectReason stopReason);

        /// <summary>
        ///     Delegate for Request Over The Limit
        /// </summary>
        /// <param name="account"></param>
        public delegate void RequestOverTheLimit(TAccount account);
    }
}