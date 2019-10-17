using System;
using System.Collections.Generic;
using System.Reflection;
using G9Common.CommandHandler;
using G9Common.Interface;
using G9Common.JsonHelper;

namespace G9Common.Abstract
{
    // Abstract class for command

    #region Abstract Class

    /// <summary>
    ///     Class used for command with send and receive
    /// </summary>
    /// <typeparam name="TSendData">Specify send data type</typeparam>
    /// <typeparam name="TReceiveData">Specify receive data type</typeparam>
    /// <typeparam name="TAccount">Access to account</typeparam>

    #region AG9Command<TSendData, TReceiveData, TAccount>

    public abstract class AG9Command<TSendData, TReceiveData, TAccount> : IG9CommandWithSend
        where TAccount : AAccount, new()
    {
        #region Fields And Properties

        ///<inheritdoc />
        public Type TypeOfSend { get; }

        /// <summary>
        ///     Specify name of command
        ///     call server and client with this name
        /// </summary>
        public string CommandName { get; }

        /// <summary>
        ///     Access to session
        /// </summary>
        private TAccount _account;

        /// <summary>
        ///     send command
        /// </summary>
        private readonly MethodInfo _sendCommand;

        /// <summary>
        ///     send command async
        /// </summary>
        private readonly MethodInfo _sendCommandAsync;

        #endregion

        #region Methods

        /// <summary>
        ///     Constructor
        ///     Initialize default requirement and set command name
        /// </summary>
        /// <param name="commandName">
        ///     Set command name.
        ///     if command name is null, command Name = class name
        /// </param>

        #region AG9CommandWithSendReceiveData

        protected AG9Command()
        {
            CommandName = GetType().Name;
            TypeOfSend = typeof(TSendData);

            // Set send command
            var method1 = typeof(TAccount).GetMethod("SendCommand");
            _sendCommand = method1.MakeGenericMethod(GetType(), TypeOfSend);

            var method2 = typeof(TAccount).GetMethod("SendCommandAsync");
            _sendCommandAsync = method2.MakeGenericMethod(GetType(), TypeOfSend);
        }

        #endregion

        /// <summary>
        ///     Used automatic in core for initialize
        /// </summary>
        /// <param name="accessToCommandDataType">requirement list from core</param>

        #region InitializeRequirement

        public void InitializeRequirement(object accessToCommandDataType)
        {
            ((SortedDictionary<string, CommandDataType<TAccount>>) accessToCommandDataType)
                .Add(CommandName, new CommandDataType<TAccount>(
                    // Access to method "ReceiveCommand" in command
                    (data, account) =>
                    {
                        try
                        {
                            _account = account;
                            ReceiveCommand(data.ToArray().FromJson<TReceiveData>(), account);
                        }
                        catch (Exception ex)
                        {
                            OnError(ex, account);
                        }
                    },
                    // Access to method "OnError" in command
                    OnError
                ));
        }

        #endregion

        /// <summary>
        ///     Method call when call command
        /// </summary>
        /// <param name="data">Received data</param>
        /// <param name="account">Access to account</param>
        /// <returns>return data</returns>
        public abstract void ReceiveCommand(TReceiveData data, TAccount account);

        /// <summary>
        ///     Send command
        /// </summary>
        /// <param name="data">Data for send</param>
        /// <returns>Return 'true' if send is success</returns>
        public bool SendCommand(TSendData data)
        {
            return (bool) _sendCommand.Invoke(_account, new[] {(object) data});
        }

        /// <summary>
        ///     Send command async
        /// </summary>
        /// <param name="data">Data for send</param>
        public void SendCommandAsync(TSendData data)
        {
            _sendCommandAsync.Invoke(_account, new[] {(object) data});
        }

        /// <summary>
        ///     Method call when throw exception for this command
        /// </summary>
        /// <param name="exceptionError">Specify exception error</param>
        /// <param name="account">Access to account</param>
        public abstract void OnError(Exception exceptionError, TAccount account);

        #endregion
    }

    #endregion


    /// <summary>
    ///     Class used for command with send and receive
    /// </summary>
    /// <typeparam name="TSendAndReceiveData">Specify send and receive data type</typeparam>
    /// <typeparam name="TAccount">Access to account</typeparam>

    #region AG9Command<TSendAndReceiveData, TReceiverAccount>

    public abstract class AG9Command<TSendAndReceiveData, TAccount>
        : AG9Command<TSendAndReceiveData, TSendAndReceiveData, TAccount>
        where TAccount : AAccount, new()
    {
    }

    #endregion

    #endregion
}