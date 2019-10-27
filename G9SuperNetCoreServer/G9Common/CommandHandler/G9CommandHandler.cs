﻿using System;
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
        /// <param name="commandAssembly">Specified command assembly (find command in specified assembly)</param>
        /// <param name="logging">Specified custom logging system</param>
        /// <param name="oCommandSize">
        ///     Specify max command size
        ///     Example: if set "n" length is "n*16" => if set 1 length is 16 then maximum command name length is 16 byte or
        ///     character
        /// </param>
        /// <param name="onUnhandledCommand">Specified event on unhandled command</param>

        #region G9CommandHandler

        public G9CommandHandler(Assembly commandAssembly, IG9Logging logging, int oCommandSize,
            Action<G9SendAndReceivePacket, TAccount> onUnhandledCommand)
        {
            // Set assembly of commands
            _commandAssembly = commandAssembly ?? throw new ArgumentNullException(nameof(commandAssembly));

            // Set command size
            _commandSize = oCommandSize * 16;

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
                    command.AccessToMethodReceiveCommand(request.Body, account);
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

            // Instance action for add command data type
            Action<string, CommandDataType<TAccount>> addCommandDataType = (commandName, commandDataType) =>
            {
                _accessToCommandDataTypeCollection.Add(commandName.GenerateStandardCommandName(_commandSize),
                    commandDataType);
            };

            derivedTypes.ForEach(oType =>
            {
                var instance = Activator.CreateInstance(oType);
                _instanceCommandCollection.Add(oType.Name.GenerateStandardCommandName(_commandSize),
                    instance);
                var method = oType.GetMethod("InitializeRequirement");
                method.Invoke(instance, new object[1] {addCommandDataType});
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
            Action<TSendAndReceive, TAccount, Action<TSendAndReceive, SendTypeForCommand>>
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
            Action<TReceiveType, TAccount, Action<TSendType, SendTypeForCommand>>
                receiveHandler,
            Action<Exception, TAccount> errorHandler)
        {
            _accessToCommandDataTypeCollection.Add(
                commandName.GenerateStandardCommandName(_commandSize),
                new CommandDataType<TAccount>(
                    // Access to method "ResponseService" in command
                    (data, account) =>
                    {
                        try
                        {
                            receiveHandler(data.ToArray().FromJson<TReceiveType>(), account,
                                (type, command) =>
                                {
                                    try
                                    {
                                        if (command == SendTypeForCommand.Asynchronous)
                                            account.SessionSendCommand.SendCommandByNameAsync(commandName, type);
                                        else
                                            account.SessionSendCommand.SendCommandByName(commandName, type);
                                    }
                                    catch (Exception ex)
                                    {
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
                commandName.GenerateStandardCommandName(_commandSize));
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
                commandName.GenerateStandardCommandName(_commandSize)]?.CommandSendType;
        }

        #endregion

        #endregion
    }
}