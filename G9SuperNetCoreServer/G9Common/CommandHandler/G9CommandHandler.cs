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
        #region Fields And Properties

        /// <summary>
        ///     Field save assemblies of commands
        /// </summary>
        private readonly Assembly[] _commandAssembly;

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
        // ReSharper disable once CollectionNeverQueried.Local
        private readonly SortedDictionary<string, object> _instanceCommandCollection;

        /// <summary>
        ///     Specified command size
        /// </summary>
        public readonly int CommandSize;

        /// <summary>
        ///     Save event on unhandled command
        /// </summary>
        private readonly Action<G9SendAndReceivePacket, TAccount> _onUnhandledCommand;

        #endregion

        #region Methods

        /// <summary>
        ///     Constructor
        ///     Initialize requirement
        /// </summary>
        /// <param name="commandAssemblies">Specified command assemblies (find command in specified assembly)</param>
        /// <param name="logging">Specified custom logging system</param>
        /// <param name="oCommandSize">
        ///     Specify max command size
        ///     Example: if set "n" length is "n*16" => if set 1 length is 16 then maximum command name length is 16 byte or
        ///     character
        /// </param>
        /// <param name="onUnhandledCommand">Specified event on unhandled command</param>

        #region G9CommandHandler

        public G9CommandHandler(Assembly[] commandAssemblies, IG9Logging logging, int oCommandSize,
            Action<G9SendAndReceivePacket, TAccount> onUnhandledCommand)
        {
            // Set assembly of commands
            _commandAssembly = commandAssemblies ?? throw new ArgumentNullException(nameof(commandAssemblies));

            // Set command size
            CommandSize = oCommandSize * 16;

            // Set logging
            _logging = logging;

            // Set on unhandled command
            _onUnhandledCommand = onUnhandledCommand;

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
        /// <param name="waitForFinish">If true, wait task to finish progress</param>
        /// <returns>Response for client</returns>

        #region G9CallHandler

        public void G9CallHandler(G9SendAndReceivePacket request, TAccount account, bool waitForFinish = false)
        {
            Action callCommand = () =>
            {
                CommandDataType<TAccount> command = null;
                try
                {
                    // Check exist command

                    #region Unhandled Command

                    if (!_accessToCommandDataTypeCollection.ContainsKey(request.Command))
                    {
                        _onUnhandledCommand?.Invoke(request, account);
                        return;
                    }

                    #endregion

                    // Get normal command
                    command = _accessToCommandDataTypeCollection[request.Command];

                    // Set log
                    if (_logging.CheckLoggingIsActive(LogsType.EVENT))
                        _logging.LogEvent(
                            $"{LogMessage.RunningCommand}: {request.Command}", G9LogIdentity.RUNNING_COMMAND,
                            LogMessage.SuccessfulOperation);

                    // Execute command with information
                    command.AccessToMethodReceiveCommand(request.Body, account, request.RequestId,
                        command.ExecuteRegisterCallBack);
                }
                catch (Exception ex)
                {
                    // Add to log exception
                    if (_logging.CheckLoggingIsActive(LogsType.EXCEPTION))
                        _logging.LogException(ex, LogMessage.ErrorInRunningCommand,
                            G9LogIdentity.RUNNING_COMMAND, LogMessage.FailedOperation);

                    // If not null call OnError in this command
                    command?.AccessToMethodOnErrorInCommand?.Invoke(ex, account);
                }
            };

            if (waitForFinish)
                callCommand.Invoke();
            else
                Task.Run(callCommand);
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

            // Instance action for add command data type
            void AddCommandDataType(string commandName, CommandDataType<TAccount> commandDataType)
            {
                _accessToCommandDataTypeCollection.Add(commandName.GenerateStandardCommandName(CommandSize),
                    commandDataType);
            }

            derivedTypes.ForEach(oType =>
            {
                // Check for account type is equal with server account type
                // Get generic type from base type
                if (oType.BaseType == null) return;
                var genericAccountArgument = oType.BaseType.GetGenericArguments().Last();
                if (genericAccountArgument == typeof(TAccount))
                {
                    // Create instance and get method initialize
                    var instance = Activator.CreateInstance(oType);
                    _instanceCommandCollection.Add(oType.Name.GenerateStandardCommandName(CommandSize),
                        instance);
                    var method = oType.GetMethod("InitializeRequirement");

                    // Initialize command and add to server
                    method?.Invoke(instance,
                        new object[] {(Action<string, CommandDataType<TAccount>>) AddCommandDataType});

                    if (_logging.CheckLoggingIsActive(LogsType.INFO))
                        _logging.LogInformation(
                            $"{LogMessage.AddCommandSuccessfully}\n{LogMessage.CommandName}: {oType.Name}\nPath: {oType.FullName}",
                            G9LogIdentity.ADD_COMMAND, LogMessage.SuccessfulOperation);
                }
                else
                {
                    if (_logging.CheckLoggingIsActive(LogsType.WARN))
                        _logging.LogWarning(
                            $"{LogMessage.FailAddCommandForGenericAccountType}\n{LogMessage.CommandName}: {oType.Name}\nPath: {oType.FullName}",
                            G9LogIdentity.ADD_COMMAND, LogMessage.FailedOperation);
                }
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
        ///         <paramref name="receiveHandler.G9Session&lt;Guid&gt;" /> :Access to sender request id
        ///     </para>
        ///     <para>
        ///         <paramref name="receiveHandler.Action&lt;TSendType, SendTypeForCommand, Action&lt;bool&gt;&gt;&gt;" />:Access
        ///         to action for send data for sender with receive request id
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
            Action<TSendAndReceive, TAccount, Guid, Action<TSendAndReceive, CommandSendType>>
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
        ///         <paramref name="receiveHandler.G9Session&lt;Guid&gt;" /> :Access to sender request id
        ///     </para>
        ///     <para>
        ///         <paramref name="receiveHandler.Action&lt;TSendType, SendTypeForCommand, Action&lt;int&gt;&gt;&gt;" />:Access
        ///         to action for send data for sender with receive request id
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
            Action<TReceiveType, TAccount, Guid, Action<TSendType, CommandSendType>>
                receiveHandler,
            Action<Exception, TAccount> errorHandler)
        {
            _accessToCommandDataTypeCollection.Add(
                commandName.GenerateStandardCommandName(CommandSize),
                new CommandDataType<TAccount>(
                    // Access to method "ResponseService" in command
                    (data, account, requestId, callBack) =>
                    {
                        try
                        {
                            // Ready data
                            var receiveData = data.FromJson<TReceiveType>(account.SessionSendCommand.SessionEncoding);

                            // Func for send
                            void SendCommandBack(TSendType answerData, CommandSendType sendType)
                            {
                                try
                                {
                                    if (sendType == CommandSendType.Asynchronous)
                                        account.SessionSendCommand.SendCommandByNameAsync(commandName, answerData,
                                            requestId);
                                    else
                                        account.SessionSendCommand.SendCommandByName(commandName, answerData,
                                            requestId);
                                }
                                catch (Exception ex)
                                {
                                    errorHandler?.Invoke(ex, account);
                                }
                            }

                            receiveHandler(receiveData, account, requestId, SendCommandBack);

                            callBack?.Invoke(receiveData, account, requestId, (Action<object, CommandSendType>)
                                (object) (Action<TSendType, CommandSendType>) SendCommandBack);
                        }
                        catch (Exception ex)
                        {
                            errorHandler?.Invoke(ex, account);
                        }
                    },
                    // Access to method "OnError" in command
                    errorHandler,
                    // Specified receive type
                    typeof(TReceiveType),
                    // Specified send type
                    typeof(TSendType)
                )
            );
        }

        #endregion

        #endregion

        /// <summary>
        ///     Check command exists
        /// </summary>
        /// <param name="commandName">Specified command name</param>
        /// <returns>return 'true' if exists</returns>

        #region CheckCommandExist

        public bool CheckCommandExist(string commandName)
        {
            return _accessToCommandDataTypeCollection.ContainsKey(
                commandName.GenerateStandardCommandName(CommandSize));
        }

        #endregion

        /// <summary>
        ///     Get command send type
        /// </summary>
        /// <param name="commandName">Specified command name</param>
        /// <returns>return type of send for command</returns>

        #region GetCommandSendType

        public Type GetCommandSendType(string commandName)
        {
            return _accessToCommandDataTypeCollection[
                commandName.GenerateStandardCommandName(CommandSize)]?.CommandSendType;
        }

        #endregion

        /// <summary>
        ///     Add extra call back for command
        /// </summary>
        /// <typeparam name="TReceive">Specified receive item</typeparam>
        /// <typeparam name="TSendType">Specified send item</typeparam>
        /// <param name="commandName">Specified game name</param>
        /// <param name="actionCallBack">Specified action call back</param>
        /// <param name="callBackExecutePeriod">Specified type of period for call back</param>

        #region AddCallBackForCommand

        public void AddCallBackForCommand<TReceive, TSendType>(string commandName, Action<TReceive, TAccount, Guid,
            Action<TSendType, CommandSendType>> actionCallBack, EnumCallBackExecutePeriod callBackExecutePeriod)
        {
            // If command not exist return
            if (!CheckCommandExist(commandName))
                throw new Exception("Command not found!");

            // Func for call back
            void ActionCallBackForCommand(object data, TAccount account, Guid id,
                Action<object, CommandSendType> sendAnswerWithReceiveRequestId)
            {
                var command = _accessToCommandDataTypeCollection[commandName.GenerateStandardCommandName(CommandSize)];

                if (callBackExecutePeriod == EnumCallBackExecutePeriod.JustOnce)
                    command.RemoveRegisterCallback(ActionCallBackForCommand);

                actionCallBack?.Invoke((TReceive) data, account, id, (answerData, sendType) =>
                {
                    if (sendType == CommandSendType.Asynchronous)
                        account.SessionSendCommand.SendCommandByNameAsync(commandName, answerData, id);
                    else
                        account.SessionSendCommand.SendCommandByName(commandName, answerData, id);
                });
            }

            // Add call back
            _accessToCommandDataTypeCollection[commandName.GenerateStandardCommandName(CommandSize)]
                .AddRegisterCallback(ActionCallBackForCommand);
        }

        #endregion

        #endregion
    }
}