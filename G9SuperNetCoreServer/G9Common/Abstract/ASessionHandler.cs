using System;
using System.Threading.Tasks;

namespace G9Common.Abstract
{
    public abstract class ASessionHandler
    {
        #region Fields And Properties

        /// <summary>
        ///     Specified ping duration in milliseconds
        /// </summary>
        public ushort PingDurationInMilliseconds;

        #endregion

        #region Methods

        #region Setter Action And Function

        /// <summary>
        ///     ### Execute From Core ###
        ///     Actions used for set ping
        ///     ushort => set ping
        /// </summary>
        public Action<ushort> Core_SetPing;

        /// <summary>
        ///     ### Execute From Core ###
        ///     Actions used for set last command
        ///     string => command name
        /// </summary>
        public Action<string> Core_SetLastCommand;

        /// <summary>
        ///     ### Execute From Core ###
        ///     Actions used for plus session send bytes
        ///     string => command name
        /// </summary>
        public Action<ushort> Core_PlusSessionTotalSendBytes;

        /// <summary>
        ///     ### Execute From Core ###
        ///     Actions used for plus session receive bytes
        ///     string => command name
        /// </summary>
        public Action<ushort> Core_PlusSessionTotalReceiveBytes;

        /// <summary>
        ///     ### Execute From Session ###
        ///     Access to action send command by name
        ///     uint => session id
        ///     string => command name
        ///     object => data for send
        ///     bool => If set true, check command exists
        /// </summary>
        public Action<uint, string, object, bool, bool> Session_SendCommandByName;

        /// <summary>
        ///     ### Execute From Session ###
        ///     Access to action send command by name async
        ///     uint => session id
        ///     string => command name
        ///     object => data for send
        ///     bool => bool => If set true, check command exists
        /// </summary>
        public Action<uint, string, object, bool, bool> Session_SendCommandByNameAsync;

        #endregion

        #endregion
    }
}