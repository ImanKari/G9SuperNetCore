using G9Common.Resource;

namespace G9Common.Abstract
{
#if UNITY_2018_1_OR_NEWER
    public abstract class AAccount : MonoBehaviour
#else
    public abstract class AAccount
#endif
    {
        /// <summary>
        ///     Access to send command of session
        /// </summary>
        public abstract ASession SessionSendCommand { get; }
    }
}