﻿using G9Common.Resource;

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