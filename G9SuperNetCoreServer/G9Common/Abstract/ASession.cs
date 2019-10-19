using System;
using System.Net;
using System.Threading.Tasks;
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
        /// <param name="name">Name of command</param>
        /// <param name="data">Data for send</param>
        /// <returns>Return => Task int specify byte to send. if don't send return 0</returns>
        public abstract Task<int> SendCommandByNameAsync(string name, object data);

        /// <summary>
        ///     Send command request by name
        /// </summary>
        /// <param name="name">Name of command</param>
        /// <param name="data">Data for send</param>
        /// <returns>Return => int number specify byte to send. if don't send return 0</returns>
        public abstract int SendCommandByName(string name, object data);

        /// <summary>
        ///     Send async request by command async
        /// </summary>
        /// <typeparam name="TCommand">Command for send</typeparam>
        /// <typeparam name="TTypeSend">Type of data for send</typeparam>
        /// <param name="data">Data for send</param>
        /// <returns>Return => Task int specify byte to send. if don't send return 0</returns>
        public abstract Task<int> SendCommandAsync<TCommand, TTypeSend>(TTypeSend data)
            where TCommand : IG9CommandWithSend;

        /// <summary>
        ///     Send request by command
        /// </summary>
        /// <typeparam name="TCommand">Command for send</typeparam>
        /// <typeparam name="TTypeSend">Type of data for send</typeparam>
        /// <param name="data">Data for send</param>
        /// <returns>Return => int number specify byte to send. if don't send return 0</returns>
        public abstract int SendCommand<TCommand, TTypeSend>(TTypeSend data)
            where TCommand : IG9CommandWithSend;

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
        ///     Get unique Identity from session
        /// </summary>
        public uint SessionId { protected set; get; }

        /// <summary>
        ///     Get ip address of session
        /// </summary>
        public IPAddress SessionIpAddress { protected set; get; }

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

        #endregion
    }
}