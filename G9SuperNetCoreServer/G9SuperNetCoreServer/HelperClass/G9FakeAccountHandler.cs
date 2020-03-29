using System;
using G9Common.Enums;
using G9SuperNetCoreServer.Abstarct;

namespace G9SuperNetCoreServer.HelperClass
{
    public struct G9FakeAccountHandler<TAccount, TSession> where TAccount : AServerAccount<TSession>, new()
        where TSession : AServerSession, new()
    {
        #region ### Fields And Properties ###

        public readonly TAccount Account;

        /// <summary>
        ///     <para>Action for send ai robot command</para>
        ///     <para>Arg1: TAccount => Access to account</para>
        ///     <para>Arg2: string => commandName</para>
        ///     <para>Arg3: string => commandData</para>
        ///     <para>Arg4: Guid => requestId</para>
        /// </summary>
        // ReSharper disable once NotAccessedField.Local
        private Action<G9FakeAccountHandler<TAccount, TSession>, string, object, Guid> _receiveCommandCallBack;

        /// <summary>
        ///     <para>Action for send ai robot command</para>
        ///     <para>Arg1: string => commandName</para>
        ///     <para>Arg2: object => commandData</para>
        ///     <para>Arg1: Guid? => customRequestId</para>
        ///     <para>Arg1: bool => checkCommandExists</para>
        ///     <para>Arg1: bool => checkCommandSendType</para>
        /// </summary>
        private readonly Action<TAccount, string, object, Guid?, bool, bool, CommandSendType> _sendCommand;

        #endregion ### Fields And Properties ###


        #region ### Methods ###

        /// <summary>
        ///     Constructor => Initialize requirement
        /// </summary>
        /// <param name="account">
        ///     <para>Notice: Automatic fill</para>
        ///     <para>Specified created account</para>
        /// </param>
        /// <param name="receiveCommandCallBack">
        ///     <para>Notice: Automatic fill</para>
        ///     <para>Specified call back for receive command</para>
        /// </param>
        /// <param name="sendCommand">
        ///     <para>Notice: Automatic fill</para>
        ///     <para>Specified action for send</para>
        /// </param>

        #region G9FakeAccountHandler

        public G9FakeAccountHandler(TAccount account,
            Action<G9FakeAccountHandler<TAccount, TSession>, string, object, Guid> receiveCommandCallBack,
            Action<TAccount, string, object, Guid?, bool, bool, CommandSendType> sendCommand)
        {
            Account = account;
            _receiveCommandCallBack = receiveCommandCallBack;
            _sendCommand = sendCommand;
        }

        #endregion

        /// <summary>
        ///     Send command request by name
        /// </summary>
        /// <param name="commandName">Name of command</param>
        /// <param name="commandData">Data for send</param>
        /// <param name="customRequestId">send data by custom request id</param>
        /// <param name="checkCommandExists">
        ///     If set true, check command exists
        ///     If not exists throw exception
        /// </param>
        /// <param name="checkCommandSendType">
        ///     If set true, check command send type
        ///     If func send data type not equal with command send type throw exception
        /// </param>

        #region SendCommandByName

        public void SendCommandByName(string commandName, object commandData, Guid? customRequestId = null,
            bool checkCommandExists = true,
            bool checkCommandSendType = true)
        {
            _sendCommand(Account, commandName, commandData, customRequestId, checkCommandExists, checkCommandSendType,
                CommandSendType.Synchronous);
        }

        #endregion

        /// <summary>
        ///     Send async command request by name
        /// </summary>
        /// <param name="commandName">Name of command</param>
        /// <param name="commandData">Data for send</param>
        /// <param name="customRequestId">send data by custom request id</param>
        /// <param name="checkCommandExists">
        ///     If set true, check command exists
        ///     If not exists throw exception
        /// </param>
        /// <param name="checkCommandSendType">
        ///     If set true, check command send type
        ///     If func send data type not equal with command send type throw exception
        /// </param>

        #region SendCommandByNameAsync

        public void SendCommandByNameAsync(string commandName, object commandData, Guid? customRequestId = null,
            bool checkCommandExists = true, bool checkCommandSendType = true)
        {
            _sendCommand(Account, commandName, commandData, customRequestId, checkCommandExists, checkCommandSendType,
                CommandSendType.Asynchronous);
        }

        #endregion

        /// <summary>
        ///     Send command request by command
        /// </summary>
        /// <param name="commandData">Data for send</param>
        /// <param name="customRequestId">send data by custom request id</param>
        /// <param name="checkCommandExists">
        ///     If set true, check command exists
        ///     If not exists throw exception
        /// </param>
        /// <param name="checkCommandSendType">
        ///     If set true, check command send type
        ///     If func send data type not equal with command send type throw exception
        /// </param>

        #region SendCommand

        public void SendCommand<TCommand, TTypeSend>(TTypeSend commandData, Guid? customRequestId = null,
            bool checkCommandExists = true,
            bool checkCommandSendType = true)
        {
            SendCommandByName(typeof(TCommand).Name, commandData, customRequestId, checkCommandExists,
                checkCommandSendType);
        }

        #endregion

        /// <summary>
        ///     Send async command request by command
        /// </summary>
        /// <param name="commandData">Data for send</param>
        /// <param name="customRequestId">send data by custom request id</param>
        /// <param name="checkCommandExists">
        ///     If set true, check command exists
        ///     If not exists throw exception
        /// </param>
        /// <param name="checkCommandSendType">
        ///     If set true, check command send type
        ///     If func send data type not equal with command send type throw exception
        /// </param>
        /// <returns>Return => Task int specify byte to send. if don't send return 0</returns>

        #region SendCommandAsync

        public void SendCommandAsync<TCommand, TTypeSend>(TTypeSend commandData, Guid? customRequestId = null,
            bool checkCommandExists = true,
            bool checkCommandSendType = true)
        {
            SendCommandByNameAsync(typeof(TCommand).Name, commandData, customRequestId, checkCommandExists,
                checkCommandSendType);
        }

        #endregion

        /// <summary>
        ///     Change listener for receive call back
        /// </summary>
        /// <param name="receiveCommandCallBack">Specified new listener</param>

        #region RemoveOldReceiveListenerAndAddNewListener

        public void RemoveOldReceiveListenerAndAddNewListener(
            Action<G9FakeAccountHandler<TAccount, TSession>, string, object, Guid> receiveCommandCallBack)
        {
            _receiveCommandCallBack = receiveCommandCallBack;
        }

        #endregion

        #endregion ### Methods ###
    }
}