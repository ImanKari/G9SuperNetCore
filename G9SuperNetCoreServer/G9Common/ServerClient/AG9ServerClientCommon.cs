using System;
using G9SuperNetCoreCommon.Abstract;
using G9SuperNetCoreCommon.CommandHandler;
using G9SuperNetCoreCommon.Enums;
using G9SuperNetCoreCommon.Interface;

namespace G9SuperNetCoreCommon.ServerClient
{
    // ReSharper disable once InconsistentNaming
    public class AG9ServerClientCommon<TAccount>
        where TAccount : AAccount, new()
    {
        #region ### Fields And Properties ###

        /// <summary>
        ///     <para>Access to command handler</para>
        ///     <para>Fill automatic with server</para>
        /// </summary>
        protected G9CommandHandler<TAccount> CommandHandlerCallback;

        #endregion ### Fields And Properties ###

        #region ### Methods ###

        /// <summary>
        ///     Register Extra call back for command
        /// </summary>
        /// <typeparam name="TCommand">Specified type of command</typeparam>
        /// <typeparam name="TSendReceiveType">Specified type of send and receive item</typeparam>
        /// <param name="actionCallBack">Extra call back for command</param>
        /// <param name="callBackExecutePeriod">Specified type of execute</param>

        #region RegisterExtraCallBackForCommand

        public void RegisterExtraCallBackForCommand<TCommand, TSendReceiveType>(Action<TSendReceiveType, TAccount, Guid,
            Action<TSendReceiveType, CommandSendType>> actionCallBack, EnumCallBackExecutePeriod callBackExecutePeriod)
            where TCommand : IG9CommandWithSend
        {
            CommandHandlerCallback.AddCallBackForCommand(typeof(TCommand).Name, actionCallBack, callBackExecutePeriod);
        }

        #endregion

        /// <summary>
        ///     Register Extra call back for command
        /// </summary>
        /// <typeparam name="TCommand">Specified type of command</typeparam>
        /// <typeparam name="TReceiveType">Specified type of receive item</typeparam>
        /// <typeparam name="TSendType">Specified type of send item</typeparam>
        /// <param name="actionCallBack">Extra call back for command</param>
        /// <param name="callBackExecutePeriod">Specified type of execute</param>

        #region RegisterExtraCallBackForCommand

        public void RegisterExtraCallBackForCommand<TCommand, TReceiveType, TSendType>(
            Action<TReceiveType, TAccount, Guid,
                Action<TSendType, CommandSendType>> actionCallBack, EnumCallBackExecutePeriod callBackExecutePeriod)
            where TCommand : IG9CommandWithSend
        {
            CommandHandlerCallback.AddCallBackForCommand(typeof(TCommand).Name, actionCallBack, callBackExecutePeriod);
        }

        #endregion

        /// <summary>
        ///     Register Extra call back for command
        /// </summary>
        /// <typeparam name="TSendReceiveType">Specified type of send and receive item</typeparam>
        /// <param name="commandName">Specified command name</param>
        /// <param name="actionCallBack">Extra call back for command</param>
        /// <param name="callBackExecutePeriod">Specified type of execute</param>

        #region RegisterExtraCallBackForCommand

        public void RegisterExtraCallBackForCommand<TSendReceiveType>(string commandName,
            Action<TSendReceiveType, TAccount, Guid,
                Action<TSendReceiveType, CommandSendType>> actionCallBack,
            EnumCallBackExecutePeriod callBackExecutePeriod)
        {
            CommandHandlerCallback.AddCallBackForCommand(commandName, actionCallBack, callBackExecutePeriod);
        }

        #endregion

        /// <summary>
        ///     Register Extra call back for command
        /// </summary>
        /// <param name="commandName">Specified command name</param>
        /// <param name="actionCallBack">Extra call back for command</param>
        /// <param name="callBackExecutePeriod">Specified type of execute</param>

        #region RegisterExtraCallBackForCommand

        public void RegisterExtraCallBackForCommand(string commandName, Action<object, TAccount, Guid,
            Action<object, CommandSendType>> actionCallBack, EnumCallBackExecutePeriod callBackExecutePeriod)
        {
            CommandHandlerCallback.AddCallBackForCommand(commandName, actionCallBack, callBackExecutePeriod);
        }

        #endregion

        #endregion ### Methods ###
    }
}