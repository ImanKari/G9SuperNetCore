using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using G9Common.Abstract;
using G9Common.Enums;
using G9Common.HelperClass;
using G9Common.Interface;
using G9Common.JsonHelper;
using G9Common.LogIdentity;
using G9Common.Packet;
using G9Common.Resource;
using G9LogManagement.Enums;

namespace G9Common.CommandHandler
{
    /// <summary>
    ///     Class command handler
    ///     Handle received command
    ///     Handle custom command
    /// </summary>
    /// <typeparam name="TAccount">Specified account type</typeparam>
    public class G9CommandHandler<TAccount>
        where TAccount : AAccount, new()
    {
        #region Events And Delegates

        /// <summary>
        ///     Delegate for Unhandled commands
        /// </summary>
        /// <param name="callServiceDataType">Receive data</param>
        /// <param name="account">account send command</param>
        public delegate string Unhandled(G9SendAndReceivePacket callServiceDataType, TAccount account);

        /// <summary>
        ///     Event used for unhandled command
        ///     Call when command not exists
        /// </summary>
        public event Unhandled UnhandledCommand;

        #endregion

        #region Fields And Properties

        /// <summary>
        ///     Field save assembly of commands
        /// </summary>
        private readonly Assembly _commandAssembly;

        /// <summary>
        ///     Access to logging
        /// </summary>
        private readonly IG9Logging _logging;

        /// <summary>
        ///     Save response command data type of commands
        /// </summary>
        private readonly SortedDictionary<string, CommandDataType<TAccount>> _accessToCommandDataTypeCollection;

        /// <summary>
        ///     Save instance of commands
        /// </summary>
        private readonly SortedDictionary<string, object> _instanceCommandCollection;

        /// <summary>
        ///     Specified command size
        /// </summary>
        public readonly int _commandSize;

        #endregion

        #region Methods

        /// <summary>
        ///     Constructor
        ///     Initialize requirement
        /// </summary>
        /// <param name="commandAssembly">Specified command assembly (find command in specified assembly)</param>
        /// <param name="logging">Specified custom logging system</param>
        /// <param name="oCommandSize">
        ///     Specify max command size
        ///     Example: if set "n" length is "n*16" => if set 1 length is 16 then maximum command name length is 16 byte or
        ///     character
        /// </param>

        #region G9CommandHandler

        public G9CommandHandler(Assembly commandAssembly, IG9Logging logging, int oCommandSize)
        {
            // Set assembly of commands
            _commandAssembly = commandAssembly ?? throw new ArgumentNullException(nameof(commandAssembly));
            // Set command size
            _commandSize = oCommandSize * 16;
            // Set logging
            _logging = logging;
            // Initialize collection
            _accessToCommandDataTypeCollection = new SortedDictionary<string, CommandDataType<TAccount>>();
            _instanceCommandCollection = new SortedDictionary<string, object>();
            // Initialize all command
            InitializeAllCommand();
        }

        #endregion

        /// <summary>
        ///     Handle request from command
        /// </summary>
        /// <param name="request">Received request</param>
        /// <param name="account">Access to account</param>
        /// <returns>Response for client</returns>

        #region G9CallHandler

        public void G9CallHandler(G9SendAndReceivePacket request, TAccount account)
        {
            Task.Run(() =>
            {
                CommandDataType<TAccount> command = null;
                try
                {
                    // Check exist command

                    #region Unhandled Command

                    if (!_accessToCommandDataTypeCollection.ContainsKey(request.Command))
                    {
                        UnhandledCommand?.Invoke(request, account);
                        return;
                    }

                    #endregion

                    // Get normal command
                    command = _accessToCommandDataTypeCollection[request.Command];

                    // Set log
                    if (_logging.LogIsActive(LogsType.EVENT))
                        _logging.LogEvent(
                            $"{LogMessage.RunningCommand}: {request.Command}", G9LogIdentity.SERVER_RUNNING_COMMAND,
                            LogMessage.SuccessfulOperation);

                    // Execute command with information
                    command.AccessToMethodReceiveCommand(request.Body, account);
                }
                catch (Exception ex)
                {
                    // Add to log exception
                    if (_logging.LogIsActive(LogsType.EXCEPTION))
                        _logging.LogException(ex, LogMessage.ErrorInRunningCommand,
                            G9LogIdentity.SERVER_RUNNING_COMMAND, LogMessage.FailedOperation);

                    // If not null call OnError in this command
                    command?.AccessToMethodOnErrorInCommand?.Invoke(ex, account);
                }
            });
        }

        #endregion

        /// <summary>
        ///     Initialize all command class
        /// </summary>

        #region InitializeAllCommand

        private void InitializeAllCommand()
        {
            // Initialize normal commands

            #region AG9Command<,>

            var derivedTypes = VType.GetDerivedTypes(typeof(AG9Command<,,>),
                _commandAssembly).Where(s => s.IsAbstract == false).ToList();

            derivedTypes.ForEach(oType =>
            {
                var instance = Activator.CreateInstance(oType);
                _instanceCommandCollection.Add(oType.Name.PadLeft(_commandSize, '9'), instance);
                var method = oType.GetMethod("InitializeRequirement");
                method.Invoke(instance, new object[1] {_accessToCommandDataTypeCollection});
            });

            #endregion
        }

        #endregion

        // Handler for add custom command

        #region AddCustomCommand

        /// <summary>
        ///     Add custom command for send and receive
        /// </summary>
        /// <typeparam name="TSendAndReceive">Type of send and receive data</typeparam>
        /// <param name="commandName">Custom command name</param>
        /// <param name="receiveHandler">
        ///     Create handler for receive data
        ///     <para>
        ///         Action parameter
        ///     </para>
        ///     <para>
        ///         <paramref name="receiveHandler.TReceiveType" />: Access to receive data
        ///     </para>
        ///     <para>
        ///         <paramref name="receiveHandler.G9Session&lt;TAccount&gt;" /> :Access to sender session
        ///     </para>
        ///     <para>
        ///         <paramref name="receiveHandler.Action&lt;TSendType, SendTypeForCommand, Action&lt;bool&gt;&gt;&gt;" />:Access
        ///         to action for send data for sender
        ///     </para>
        ///     <para>
        ///         <paramref
        ///             name="receiveHandler.Action&lt;TSendType, SendTypeForCommand, Action&lt;bool&gt;&gt;&gt;.Action&lt;bool&gt;" />
        ///         : Action run when send success
        ///     </para>
        /// </param>
        /// <param name="errorHandler">
        ///     Create handler for runtime error
        ///     <para>
        ///         Action parameter
        ///     </para>
        ///     <para>
        ///         <paramref name="errorHandler.Exception" />: Access to exception error
        ///     </para>
        ///     <para>
        ///         <paramref name="errorHandler.G9Session&lt;TAccount&gt;" />: Access to sender session
        ///     </para>
        /// </param>

        #region AddCustomCommand<TSendAndReceive>

        public void AddCustomCommand<TSendAndReceive>(
            string commandName,
            Action<TSendAndReceive, TAccount, Action<TSendAndReceive, SendTypeForCommand, Action<int>>>
                receiveHandler,
            Action<Exception, TAccount> errorHandler)
        {
            AddCustomCommand<TSendAndReceive, TSendAndReceive>(commandName, receiveHandler, errorHandler);
        }

        #endregion

        /// <summary>
        ///     Add custom command for send and receive
        /// </summary>
        /// <typeparam name="TReceiveType">Type of receive data</typeparam>
        /// <typeparam name="TSendType">Type of send data</typeparam>
        /// <param name="commandName">Custom command name</param>
        /// <param name="receiveHandler">
        ///     Create handler for receive data
        ///     <para>
        ///         Action parameter
        ///     </para>
        ///     <para>
        ///         <paramref name="receiveHandler.TReceiveType" />: Access to receive data
        ///     </para>
        ///     <para>
        ///         <paramref name="receiveHandler.G9Session&lt;TAccount&gt;" /> :Access to sender session
        ///     </para>
        ///     <para>
        ///         <paramref name="receiveHandler.Action&lt;TSendType, SendTypeForCommand, Action&lt;int&gt;&gt;&gt;" />:Access
        ///         to action for send data for sender
        ///     </para>
        ///     <para>
        ///         <paramref
        ///             name="receiveHandler.Action&lt;TSendType, SendTypeForCommand, Action&lt;int&gt;&gt;&gt;.Action&lt;int&gt;" />
        ///         : Action run when send success | int number specify bytes to send. if don't send receive 0
        ///     </para>
        /// </param>
        /// <param name="errorHandler">
        ///     Create handler for runtime error
        ///     <para>
        ///         Action parameter
        ///     </para>
        ///     <para>
        ///         <paramref name="errorHandler.Exception" />: Access to exception error
        ///     </para>
        ///     <para>
        ///         <paramref name="errorHandler.G9Session&lt;TAccount&gt;" />: Access to sender session
        ///     </para>
        /// </param>

        #region AddCustomCommand<TReceiveType, TSendType>

        public void AddCustomCommand<TReceiveType, TSendType>(
            string commandName,
            Action<TReceiveType, TAccount, Action<TSendType, SendTypeForCommand, Action<int>>>
                receiveHandler,
            Action<Exception, TAccount> errorHandler)
        {
            _accessToCommandDataTypeCollection.Add(
                commandName.PadLeft(_commandSize, '9'),
                new CommandDataType<TAccount>(
                    // Access to method "ResponseService" in command
                    (data, account) =>
                    {
                        try
                        {
                            receiveHandler(data.ToArray().FromJson<TReceiveType>(), account,
                                async (type, command, onSendFinish) =>
                                {
                                    try
                                    {
                                        if (command == SendTypeForCommand.Asynchronous)
                                            onSendFinish?.Invoke(
                                                await account.SessionSendCommand.SendCommandByNameAsync(commandName,
                                                    type));
                                        else
                                            onSendFinish?.Invoke(
                                                account.SessionSendCommand.SendCommandByName(commandName, type));
                                    }
                                    catch (Exception ex)
                                    {
                                        onSendFinish?.Invoke(0);
                                        errorHandler?.Invoke(ex, account);
                                    }
                                });
                        }
                        catch (Exception ex)
                        {
                            errorHandler?.Invoke(ex, account);
                        }
                    },
                    // Access to method "OnError" in command
                    errorHandler
                )
            );
        }

        #endregion

        #endregion

        #endregion
    }
}