using System.Threading.Tasks;
using G9Common.Interface;

namespace G9Common.Abstract
{
    public abstract class ASession
    {
        /// <summary>
        ///     Send async command request by name
        /// </summary>
        /// <param name="name">Name of command</param>
        /// <param name="data">Data for send</param>
        /// <returns>Return => Task int specify byte to send. if don't send return 0</returns>
        public abstract Task<int> SendCommandByNameAsync(string name, object data);

        /// <summary>
        ///     Send command request by name
        /// </summary>
        /// <param name="name">Name of command</param>
        /// <param name="data">Data for send</param>
        /// <returns>Return => int number specify byte to send. if don't send return 0</returns>
        public abstract int SendCommandByName(string name, object data);

        /// <summary>
        ///     Send async request by command async
        /// </summary>
        /// <typeparam name="TCommand">Command for send</typeparam>
        /// <typeparam name="TTypeSend">Type of data for send</typeparam>
        /// <param name="data">Data for send</param>
        /// <returns>Return => Task int specify byte to send. if don't send return 0</returns>
        public abstract Task<int> SendCommandAsync<TCommand, TTypeSend>(TTypeSend data)
            where TCommand : IG9CommandWithSend;

        /// <summary>
        ///     Send request by command
        /// </summary>
        /// <typeparam name="TCommand">Command for send</typeparam>
        /// <typeparam name="TTypeSend">Type of data for send</typeparam>
        /// <param name="data">Data for send</param>
        /// <returns>Return => int number specify byte to send. if don't send return 0</returns>
        public abstract int SendCommand<TCommand, TTypeSend>(TTypeSend data)
            where TCommand : IG9CommandWithSend;
    }
}