using System;
using System.Net;
using G9Common.Configuration;
using G9Common.Enums;
using G9Common.HelperClass;

namespace G9SuperNetCoreServer.Config
{
    public class G9ServerConfig : AG9BaseConfig
    {
        #region Methods

        /// <summary>
        ///     Constructor
        ///     Initialize requirement config for server
        /// </summary>
        /// <param name="oServerName">Specify server name</param>
        /// <param name="oIpAddress">Specify ip address</param>
        /// <param name="oPortNumber">Specify port number</param>
        /// <param name="oMode">Specify mode TCP or UDP</param>
        /// <param name="oGetPingTimeOut">
        ///     Specify time out for get ping from client
        ///     If set 'TimeSpan.Zero' or 'Timeout.InfiniteTimeSpan' => infinity (Disable get ping)
        ///     If set null adjusted default value => 3963 millisecond
        /// </param>
        /// <param name="oClearIdleSessionTimeOut">
        ///     Specify remove session time out in second
        ///     If set null adjusted default value => 'TimeSpan.Zero'  => infinity (Disable clear idle session)
        /// </param>
        /// <param name="oCommandSize">
        ///     Specify command size
        ///     Example: if set "n" length is "n*16" => if set 1 length is 16 then maximum command name length is 16 byte or
        ///     character
        /// </param>
        /// <param name="oBodySize">
        ///     Specify body size
        ///     Example: if set "n" length is "n*16" => if set 1 length is 16 then maximum command name length is 16 byte or
        ///     character
        /// </param>
        /// <param name="oMaxConnectionNumber">
        ///     Specify max of connection for server
        ///     The maximum length of the pending connections queue.
        ///     Set 0 => infinity
        /// </param>
        /// <param name="oMaxRequestPerSecond">
        ///     Specify maximum request from client per second
        ///     Set 0 => infinity
        /// </param>
        /// <param name="oEnableAutoKickClientForMaxRequest">
        ///     Specify auto kick client If the number of requests exceeded the limit
        ///     'MaxRequestPerSecond'
        /// </param>
        /// <param name="oEncodingAndDecoding">Specify type of encoding and decoding</param>

        #region G9SuperSocketServerConfig

        public G9ServerConfig(string oServerName, IPAddress oIpAddress, ushort oPortNumber, SocketMode oMode,
            TimeSpan? oGetPingTimeOut = null, TimeSpan? oClearIdleSessionTimeOut = null, byte oCommandSize = 1,
            byte oBodySize = 8, ushort oMaxConnectionNumber = 0, ushort oMaxRequestPerSecond = 0,
            bool oEnableAutoKickClientForMaxRequest = false, G9Encoding oEncodingAndDecoding = null
        )
            : base(oIpAddress, oPortNumber, oMode, oCommandSize, oBodySize, oEncodingAndDecoding)
        {
            // Set name
            ServerName = string.IsNullOrEmpty(oServerName)
                ? throw new ArgumentException($"Argument {nameof(oServerName)} not correct!", nameof(oServerName))
                : oServerName;
            // Set max connection number
            MaxConnectionNumber = oMaxConnectionNumber == 0
                ? ushort.MaxValue
                : oMaxConnectionNumber;
            // Set max request per second
            MaxRequestPerSecond = oMaxRequestPerSecond;
            // Set enable auto kick client for max request
            EnableAutoKickClientForMaxRequest = oEnableAutoKickClientForMaxRequest;
            // Set clear idle session time out
            ClearIdleSessionTimeOut = oClearIdleSessionTimeOut ?? TimeSpan.Zero;
            // Set get ping time out
            GetPingTimeOut = oGetPingTimeOut ?? TimeSpan.FromMilliseconds(3963);
        }

        #endregion

        #endregion

        #region Fields And Properties

        /// <summary>
        ///     Specify server name
        /// </summary>
        public string ServerName { set; get; }

        /// <summary>
        ///     Specify max of connection for server
        ///     Set 0 => infinity
        /// </summary>
        public ushort MaxConnectionNumber { set; get; }

        /// <summary>
        ///     Specify maximum request from client per second
        ///     Set 0 => infinity
        /// </summary>
        public ushort MaxRequestPerSecond { set; get; }

        /// <summary>
        ///     Specify auto kick client If the number of requests exceeded the limit 'MaxRequestPerSecond'
        /// </summary>
        public bool EnableAutoKickClientForMaxRequest { set; get; }

        /// <summary>
        ///     Specify remove session time out in second
        ///     Set 'TimeSpan.Zero' or 'Timeout.InfiniteTimeSpan' => infinity (Disable clear idle session)
        /// </summary>
        public TimeSpan ClearIdleSessionTimeOut { set; get; }

        /// <summary>
        ///     Timeout for get ping
        ///     If set 'TimeSpan.Zero' or 'Timeout.InfiniteTimeSpan' => infinity (Disable get ping)
        ///     Default value is 3963 millisecond
        /// </summary>
        public TimeSpan GetPingTimeOut { set; get; }

        #endregion
    }
}