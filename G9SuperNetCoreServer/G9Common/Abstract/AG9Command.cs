﻿using System;
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
    /// <typeparam name="TSendType">Specify send data type</typeparam>
    /// <typeparam name="TReceiveType">Specify receive data type</typeparam>
    /// <typeparam name="TAccount">Access to account</typeparam>

    #region AG9Command<TSendData, TReceiveData, TAccount>

    public abstract class AG9Command<TSendType, TReceiveType, TAccount> : IG9CommandWithSend
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
        ///     Specified command initialized
        /// </summary>
        private bool _initializeCommand;

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
            TypeOfSend = typeof(TSendType);
        }

        #endregion

        /// <summary>
        ///     Used automatically in core for initialize
        /// </summary>
        /// <param name="accessToCommandDataType">requirement for initialize command</param>

        #region InitializeRequirement

        public void InitializeRequirement(object accessToCommandDataType)
        {
            if (!_initializeCommand)
            {
                ((Action<string, CommandDataType<TAccount>>) accessToCommandDataType)?.Invoke(
                    CommandName, new CommandDataType<TAccount>(
                        // Access to method "ReceiveCommand" in command
                        (data, account) =>
                        {
                            try
                            {
                                ReceiveCommand(data.ToArray().FromJson<TReceiveType>(), account);
                            }
                            catch (Exception ex)
                            {
                                OnError(ex, account);
                            }
                        },
                        // Access to method "OnError" in command
                        OnError,
                        // Specified receive type
                        typeof(TReceiveType),
                        // Specified send type
                        typeof(TSendType)
                    ));
                _initializeCommand = true;
            }
        }

        #endregion

        /// <summary>
        ///     Method call when call command
        /// </summary>
        /// <param name="data">Received data</param>
        /// <param name="account">Access to account</param>
        /// <returns>return data</returns>
        public abstract void ReceiveCommand(TReceiveType data, TAccount account);

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
    /// <typeparam name="TSendAndReceiveType">Specify send and receive data type</typeparam>
    /// <typeparam name="TAccount">Access to account</typeparam>

    #region AG9Command<TSendAndReceiveData, TReceiverAccount>

    public abstract class AG9Command<TSendAndReceiveType, TAccount>
        : AG9Command<TSendAndReceiveType, TSendAndReceiveType, TAccount>
        where TAccount : AAccount, new()
    {
    }

    #endregion

    #endregion
}