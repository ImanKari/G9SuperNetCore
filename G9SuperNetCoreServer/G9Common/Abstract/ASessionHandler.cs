using System;
using System.Threading.Tasks;

namespace G9Common.Abstract
{
    public abstract class ASessionHandler
    {
        /// <summary>
        ///     Access to action send command by name
        ///     long => session id
        ///     string => command name
        ///     object => data for send
        ///     Return => int number specify byte to send. if don't send return 0
        /// </summary>
        public Func<long, string, object, int> SendCommandByName;

        /// <summary>
        ///     Access to action send command by name async
        ///     long => session id
        ///     string => command name
        ///     object => data for send
        ///     Return => Task int number specify byte to send. if don't send return 0
        /// </summary>
        public Func<long, string, object, Task<int>> SendCommandByNameAsync;
    }
}