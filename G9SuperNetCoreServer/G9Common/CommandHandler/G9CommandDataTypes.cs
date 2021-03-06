using System;
using System.Collections.Generic;
using G9SuperNetCoreCommon.Abstract;
using G9SuperNetCoreCommon.Enums;

namespace G9SuperNetCoreCommon.CommandHandler
{
    /// <summary>
    ///     Class Command data type -> used for save command class information
    /// </summary>

    #region CommandDataType

    public class CommandDataType<TAccount>
        where TAccount : AAccount, new()
    {
        #region Methods

        /// <summary>
        ///     Structure
        ///     Initialize requirement
        /// </summary>
        /// <param name="accessToMethodReceiveCommand">Specify method "ReceiveCommand" in command</param>
        /// <param name="accessToMethodOnErrorInCommand">Specify method "OnError" in command</param>
        /// <param name="commandReceiveType">Specified receive type for command</param>
        /// <param name="commandSendType">Specified send type for command</param>

        #region CommandDataType

        public CommandDataType(
            Action<
#if NETSTANDARD2_1 || NETCOREAPP3_0 || NETCOREAPP3_1
            ReadOnlyMemory<byte>,
#else
                    byte[],
#endif
                    TAccount, Guid, Action<object, TAccount, Guid, Action<object, CommandSendType>>>
                accessToMethodReceiveCommand,
            Action<Exception, TAccount> accessToMethodOnErrorInCommand,
            Type commandReceiveType,
            Type commandSendType)
        {
            AccessToMethodReceiveCommand = accessToMethodReceiveCommand;
            AccessToMethodOnErrorInCommand = accessToMethodOnErrorInCommand;
            CommandReceiveType = commandReceiveType;
            CommandSendType = commandSendType;
        }

        #endregion

        /// <summary>
        ///     Add extra call back for command
        /// </summary>
        /// <param name="callBack">Action call back</param>

        #region AddRegisterCallback

        public void AddRegisterCallback(
            Func<object, TAccount, Guid, Action<object, CommandSendType>, EnumCallBackExecutePeriod> callBack)
        {
            if (_registerCallbackCollection == null)
                _registerCallbackCollection =
                    new List<Func<object, TAccount, Guid, Action<object, CommandSendType>, EnumCallBackExecutePeriod
                    >>();

            _registerCallbackCollection.Add(callBack);
        }

        #endregion

        /// <summary>
        ///     Execute all register call back
        /// </summary>
        /// <param name="data">Receive data</param>
        /// <param name="account">Specified account</param>
        /// <param name="id">Specified packet id</param>
        /// <param name="sendAction">Specified send action</param>

        #region ExecuteRegisterCallBack

        public void ExecuteRegisterCallBack(object data, TAccount account, Guid id, Action<object, CommandSendType>
            sendAction)
        {
            _registerCallbackCollection?.ForEach(s => s?.Invoke(data, account, id, sendAction));
        }

        #endregion

        #endregion

        #region Fields And Properties

        /// <summary>
        ///     Collection for save register call back for command
        /// </summary>
        private List<Func<object, TAccount, Guid, Action<object, CommandSendType>, EnumCallBackExecutePeriod>>
            _registerCallbackCollection;

        /// <summary>
        ///     Access to method "ResponseService" in command
        /// </summary>
        public readonly Action<
#if NETSTANDARD2_1 || NETCOREAPP3_0 || NETCOREAPP3_1
            ReadOnlyMemory<byte>,
#else
                byte[],
#endif
                TAccount, Guid, Action<object, TAccount, Guid, Action<object, CommandSendType>>>
            AccessToMethodReceiveCommand;

        /// <summary>
        ///     Access to method "OnError" in command
        /// </summary>
        public readonly Action<Exception, TAccount> AccessToMethodOnErrorInCommand;

        /// <summary>
        ///     Access to receive type for command
        /// </summary>
        public readonly Type CommandReceiveType;

        /// <summary>
        ///     Access to send type for command
        /// </summary>
        public readonly Type CommandSendType;

        #endregion
    }

    #endregion
}