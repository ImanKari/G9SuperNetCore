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
        ///     Enable test mode for session
        /// </summary>
        public Action<string> EnableTestMode;

        /// <summary>
        ///     Disable test mode for session
        /// </summary>
        public Action DisableTestMode;

        #endregion

        #endregion
    }
}