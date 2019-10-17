using System;
using G9Common.Abstract;

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

        #region CommandDataType

        public CommandDataType(
            Action<ReadOnlyMemory<byte>, TAccount> accessToMethodReceiveCommand,
            Action<Exception, TAccount> accessToMethodOnErrorInCommand)
        {
            AccessToMethodReceiveCommand = accessToMethodReceiveCommand;
            AccessToMethodOnErrorInCommand = accessToMethodOnErrorInCommand;
        }

        #endregion

        #endregion

        #region Fields And Properties

        /// <summary>
        ///     Access to method "ResponseService" in command
        /// </summary>
        public Action<ReadOnlyMemory<byte>, TAccount> AccessToMethodReceiveCommand { get; }

        /// <summary>
        ///     Access to method "OnError" in command
        /// </summary>
        public Action<Exception, TAccount> AccessToMethodOnErrorInCommand { get; }

        #endregion
    }

    #endregion
}