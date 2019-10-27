using System;
using System.Globalization;
using G9Common.Enums;
using G9Common.LogIdentity;
using G9Common.Resource;
using G9LogManagement.Enums;
using G9SuperNetCoreServer.Abstarct;

namespace G9SuperNetCoreServer.AbstractServer
{
    public abstract partial class AG9SuperNetCoreServerBase<TAccount, TSession>
        where TAccount : AServerAccount<TSession>, new()
        where TSession : AServerSession, new()
    {
        #region Default Command Methods

        #region Default Ping Command

        /// <summary>
        ///     Ping Command Handler
        /// </summary>
        private void PingCommandReceiveHandler(string receiveData, TAccount Account,
            Action<string, SendTypeForCommand> sendDataForThisCommand)
        {
            if (DateTime.TryParse(receiveData, out var receiveDateTime))
            {
                var ping = (ushort)(DateTime.Now - receiveDateTime).TotalMilliseconds;
                _core.GetAccountUtilitiesBySessionId(Account.Session.SessionId).SessionHandler
                    .Core_SetPing(ping);
                sendDataForThisCommand(ping.ToString(CultureInfo.InvariantCulture), SendTypeForCommand.Asynchronous);
                if (_core.Logging.CheckLoggingIsActive(LogsType.INFO))
                    _core.Logging.LogInformation(Account.Session.GetSessionInfo(), G9LogIdentity.CLIENT_PING,
                        LogMessage.ClientPing);
            }
        }

        #endregion

        #region Default Echo Command

        /// <summary>
        ///     Echo Command Handler
        /// </summary>
        private void G9EchoCommandPingCommandReceiveHandler(string receiveData, TAccount Account,
            Action<string, SendTypeForCommand> sendDataForThisCommand)
        {
            if (_core.Logging.CheckLoggingIsActive(LogsType.INFO))
                _core.Logging.LogInformation($"{LogMessage.CommandEcho}\n{LogMessage.ReceiveData}: {receiveData}",
                    G9LogIdentity.ECHO_COMMAND,
                    LogMessage.SuccessfulOperation);
            sendDataForThisCommand(receiveData, SendTypeForCommand.Asynchronous);
        }

        #endregion

        #region Default Test Send Receive

        /// <summary>
        ///     Field for save text counter
        /// </summary>
        private int _testCounter;

        /// <summary>
        ///     Test Send Receive Command Handler
        /// </summary>
        private void G9TestSendReceiveCommandReceiveHandler(string receiveData, TAccount Account,
            Action<string, SendTypeForCommand> sendDataForThisCommand)
        {
            // Check if disable test mode return
            if (!Account.Session.EnableTestSendReceiveMode) return;

            // if enable => send receive data
            sendDataForThisCommand(receiveData, SendTypeForCommand.Asynchronous);

            // Set log
            if (_core.Logging.CheckLoggingIsActive(LogsType.INFO))
                _core.Logging.LogInformation(
                    $"{LogMessage.CommanTestSendReceive}\n{LogMessage.ReceiveData}: {receiveData}\n{LogMessage.TestNumber}: {_testCounter++}",
                    G9LogIdentity.TEST_SEND_RECEIVE, LogMessage.SuccessfulOperation);
        }

        #endregion

        #endregion
    }
}