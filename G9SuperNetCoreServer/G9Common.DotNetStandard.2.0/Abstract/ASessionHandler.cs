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
        ///     <para>### Execute From Core ###</para>
        ///     <para>Actions used for set ping</para>
        ///     <para>ushort => set ping</para>
        /// </summary>
        public Action<ushort> Core_SetPing;

        /// <summary>
        ///     <para>### Execute From Core ###</para>
        ///     <para>Actions used for set last command</para>
        ///     <para>string => command name</para>
        /// </summary>
        public Action<string> Core_SetLastCommand;

        /// <summary>
        ///     <para>### Execute From Core ###</para>
        ///     <para>Actions used for plus session send bytes</para>
        ///     <para>string => command name</para>
        /// </summary>
        public Action<ushort> Core_PlusSessionTotalSendBytes;

        /// <summary>
        ///     <para>### Execute From Core ###</para>
        ///     <para>Actions used for plus session receive bytes</para>
        ///     <para>string => command name</para>
        /// </summary>
        public Action<ushort> Core_PlusSessionTotalReceiveBytes;

        /// <summary>
        ///     <para>### Execute From Core ###</para>
        ///     <para>Actions used for enable is authorization</para>
        /// </summary>
        public Action Core_AuthorizationClient;

        /// <summary>
        ///     <para>### Execute From Session ###</para>
        ///     <para>Access to action send command by name</para>
        ///     <para>uint => session id</para>
        ///     <para>string => command name</para>
        ///     <para>object => data for send</para>
        ///     <para>Guid? => set custom request id</para>
        ///     <para>bool => If set true, check command exists</para>
        ///     <para>bool => If set true, check command send type</para>
        /// </summary>
        public Action<uint, string, object, Guid?, bool, bool> Session_SendCommandByName;

        /// <summary>
        ///     <para>### Execute From Session ###</para>
        ///     <para>Access to action send command by name async</para>
        ///     <para>uint => session id</para>
        ///     <para>string => command name</para>
        ///     <para>object => data for send</para>
        ///     <para>Guid? => set custom request id</para>
        ///     <para>bool => If set true, check command exists</para>
        ///     <para>bool => If set true, check command send type</para>
        /// </summary>
        public Action<uint, string, object, Guid?, bool, bool> Session_SendCommandByNameAsync;

        #endregion

        #endregion
    }
}