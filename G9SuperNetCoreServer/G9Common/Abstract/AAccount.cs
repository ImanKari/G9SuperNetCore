using G9Common.Enums;

namespace G9Common.Abstract
{
    public abstract class AAccount
    {
        /// <summary>
        ///     Access to send command of session
        /// </summary>
        public abstract ASession SessionSendCommand { get; }

        /// <summary>
        ///     Call when session close
        /// </summary>
        /// <param name="reason">Get reason of close</param>
        public abstract void OnSessionClose(CloseReason reason);
    }
}