using System;
using System.Net;
using G9Common.HelperClass;
using G9Common.Interface;
using G9Common.Resource;

namespace G9Common.Abstract
{
    /// <summary>
    ///     Class for management session
    /// </summary>
    public abstract class ASession
    {
        #region Methods

        /// <summary>
        ///     Send async command request by name
        /// </summary>
        /// <param name="commandName">Name of command</param>
        /// <param name="commandData">Data for send</param>
        /// <param name="customRequestId">send data by custom request id</param>
        /// <param name="checkCommandExists">
        ///     If set true, check command exists
        ///     If not exists throw exception
        /// </param>
        /// <param name="checkCommandSendType">
        ///     If set true, check command send type
        ///     If func send data type not equal with command send type throw exception
        /// </param>
        public abstract void SendCommandByNameAsync(string commandName, object commandData,
            Guid? customRequestId = null, bool checkCommandExists = true, bool checkCommandSendType = true);

        /// <summary>
        ///     Send command request by name
        /// </summary>
        /// <param name="commandName">Name of command</param>
        /// <param name="commandData">Data for send</param>
        /// <param name="customRequestId">send data by custom request id</param>
        /// <param name="checkCommandExists">
        ///     If set true, check command exists
        ///     If not exists throw exception
        /// </param>
        /// <param name="checkCommandSendType">
        ///     If set true, check command send type
        ///     If func send data type not equal with command send type throw exception
        /// </param>
        public abstract void SendCommandByName(string commandName, object commandData, Guid? customRequestId = null,
            bool checkCommandExists = true, bool checkCommandSendType = true);

        /// <summary>
        ///     Send async request by command async
        /// </summary>
        /// <typeparam name="TCommand">Command for send</typeparam>
        /// <typeparam name="TTypeSend">Type of data for send</typeparam>
        /// <param name="customRequestId">send data by custom request id</param>
        /// <param name="commandData">Data for send</param>
        /// <param name="checkCommandExists">
        ///     If set true, check command exists
        ///     If not exists throw exception
        /// </param>
        /// <param name="checkCommandSendType">
        ///     If set true, check command send type
        ///     If func send data type not equal with command send type throw exception
        /// </param>
        public abstract void SendCommandAsync<TCommand, TTypeSend>(TTypeSend commandData, Guid? customRequestId = null,
            bool checkCommandExists = true, bool checkCommandSendType = true) where TCommand : IG9CommandWithSend;

        /// <summary>
        ///     Send request by command
        /// </summary>
        /// <typeparam name="TCommand">Command for send</typeparam>
        /// <typeparam name="TTypeSend">Type of data for send</typeparam>
        /// <param name="customRequestId">send data by custom request id</param>
        /// <param name="commandData">Data for send</param>
        /// <param name="checkCommandExists">
        ///     If set true, check command exists
        ///     If not exists throw exception
        /// </param>
        /// <param name="checkCommandSendType">
        ///     If set true, check command send type
        ///     If func send data type not equal with command send type throw exception
        /// </param>
        public abstract void SendCommand<TCommand, TTypeSend>(TTypeSend commandData, Guid? customRequestId = null,
            bool checkCommandExists = true, bool checkCommandSendType = true) where TCommand : IG9CommandWithSend;

        /// <summary>
        ///     Get session information
        /// </summary>
        /// <returns>String session information</returns>
        public string GetSessionInfo()
        {
            return
                $"{LogMessage.ClientSessionIdentity}: {SessionId}\n{LogMessage.IpAddress}: {SessionIpAddress}\n{LogMessage.ClientPing}: {Ping}\n{LogMessage.ClientPingDateTime}: {LastPingDateTime:yyyy/MM/dd HH:mm:ss.fff}\n{LogMessage.LastCommandUsed}: {LastCommand}\n{LogMessage.LastCommandUsedDateTime}: {LastCommandDateTime:yyyy/MM/dd HH:mm:ss.fff}";
        }

        #endregion

        #region Fields And Properties

        /// <summary>
        /// Access to game account
        /// </summary>
        public AAccount AccessToAccount { protected set; get; }

        /// <summary>
        ///     Get unique Identity from session
        /// </summary>
        public uint SessionId { protected set; get; }

        /// <summary>
        ///     Specified session start date time
        /// </summary>
        public DateTime SessionStartDateTime { get; } = DateTime.Now;

        /// <summary>
        ///     Get ip address of session
        /// </summary>
        public IPAddress SessionIpAddress { protected set; get; }

        /// <summary>
        ///     Specified client authorization
        /// </summary>
        public bool IsAuthorization { protected set; get; }

        /// <summary>
        ///     Specified type of encoding
        /// </summary>
        public G9Encoding SessionEncoding { protected set; get; }

        /// <summary>
        ///     <para>Check validation for send command</para>
        ///     <para>Optional for check validation</para>
        ///     <para>If use => don't use base of method because base of method force return true</para>
        /// </summary>
        /// <param name="account">Access to account</param>
        /// <param name="commandName">Name of command</param>
        /// <param name="commandData">Data for send</param>
        /// <param name="customRequestId">Specify custom request id</param>
        /// <param name="checkCommandExists">
        ///     If true, check command exists
        /// </param>
        /// <param name="checkCommandSendType">
        ///     If set true, check command send type
        /// </param>
        /// <returns>
        ///     <para>If return true, send command is valid and sending is done</para>
        ///     <para>if return false, send command is not valid and do not send</para>
        /// </returns>
        public virtual bool CheckValidationForSendCommand(AAccount account, string commandName, object commandData,
            Guid? customRequestId, bool checkCommandExists, bool checkCommandSendType)
        {
            // Ignore valid if don't override
            return true;
        }

        #region Ping Utilities

        /// <summary>
        ///     Field for save ping
        /// </summary>
        private ushort _ping;

        /// <summary>
        ///     Specified session ping
        /// </summary>
        public ushort Ping
        {
            protected set
            {
                LastPingDateTime = DateTime.Now;
                _ping = value;
            }
            get => _ping;
        }

        /// <summary>
        ///     Specified date time of ping
        /// </summary>
        public DateTime LastPingDateTime { protected set; get; }

        /// <summary>
        ///     Specified ping duration in milliseconds
        /// </summary>
        public ushort PingDurationInMilliseconds { protected set; get; }

        #endregion

        #region LastCommand Utilities

        /// <summary>
        ///     Specified last command use
        /// </summary>
        public abstract string LastCommand { protected set; get; }

        /// <summary>
        ///     Specify date time of used last command
        /// </summary>
        public DateTime LastCommandDateTime { protected set; get; }

        #endregion

        #region Session Send And Receive Utilities

        /// <summary>
        ///     Specified total send in bytes for current session
        /// </summary>
        public uint SessionTotalSendBytes { protected set; get; }

        /// <summary>
        ///     Specified number of total send packet for current session
        /// </summary>
        public uint SessionTotalSendPacket { protected set; get; }

        /// <summary>
        ///     Specified total receive in bytes for current session
        /// </summary>
        public uint SessionTotalReceiveBytes { protected set; get; }

        /// <summary>
        ///     Specified number of total receive packet for current session
        /// </summary>
        public uint SessionTotalReceivePacket { protected set; get; }

        #endregion

        #endregion
    }
}