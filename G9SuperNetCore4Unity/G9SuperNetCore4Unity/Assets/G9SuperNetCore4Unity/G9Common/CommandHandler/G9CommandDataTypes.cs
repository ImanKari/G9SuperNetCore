using System;
using G9Common.Abstract;
using G9Common.HelperClass;

namespace G9Common.CommandHandler
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
#if NETSTANDARD2_1
            ReadOnlyMemory<byte>,
#else
                byte[],
#endif
                TAccount, Guid> accessToMethodReceiveCommand,
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

        #endregion

        #region Fields And Properties

        /// <summary>
        ///     Access to method "ResponseService" in command
        /// </summary>
        public readonly Action<
#if NETSTANDARD2_1
            ReadOnlyMemory<byte>,
#else
            byte[],
#endif
            TAccount, Guid> AccessToMethodReceiveCommand;

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