using G9SuperNetCoreCommon.Resource;

namespace G9SuperNetCoreCommon.Abstract
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