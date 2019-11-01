using System;
using G9Common.Enums;

namespace G9Common.Abstract
{
    public abstract class AAccount
    {
        /// <summary>
        ///     Access to send command of session
        /// </summary>
        public abstract ASession SessionSendCommand { get; }

        
    }
}