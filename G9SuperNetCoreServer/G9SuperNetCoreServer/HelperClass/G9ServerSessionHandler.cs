using System;
using G9Common.Abstract;

namespace G9SuperNetCoreServer.HelperClass
{
    public class G9ServerSessionHandler : ASessionHandler
    {
        #region Fields And Properties

        #endregion

        #region Methods

        #region Setter Action And Function

        /// <summary>
        ///     ### Execute From Core ###
        ///     Enable test mode for session
        ///     string => custom test message
        /// </summary>
        public Action<string> Core_EnableTestMode;

        /// <summary>
        ///     ### Execute From Core ###
        ///     Disable test mode for session
        /// </summary>
        public Action Core_DisableTestMode;

        /// <summary>
        ///     ### Execute From Core ###
        ///     Set max request requirement
        ///     ushort => maximum request per second
        /// </summary>
        public Action<ushort> Core_SetMaxRequestRequirement;

        /// <summary>
        ///     ### Execute From Session ###
        ///     Run in session when Receive Request Over The Limit In Second
        ///     uint => session id
        /// </summary>
        public Action<uint> Session_OnSessionReceiveRequestOverTheLimitInSecond;

        #endregion

        #endregion
    }
}