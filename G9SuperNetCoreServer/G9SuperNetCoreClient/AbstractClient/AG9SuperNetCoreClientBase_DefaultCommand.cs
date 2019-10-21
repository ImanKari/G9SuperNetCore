using System;
using G9Common.Enums;
using G9Common.LogIdentity;
using G9Common.Resource;
using G9LogManagement.Enums;
using G9SuperNetCoreClient.Abstract;

namespace G9SuperNetCoreClient.AbstractClient
{
    public abstract partial class AG9SuperNetCoreClientBase<TAccount, TSession>
        where TAccount : AClientAccount<TSession>, new()
        where TSession : AClientSession, new()
    {
        #region Default Command Methods

        #region Default Ping Command

        /// <summary>
        ///     Ping Command Handler
        /// </summary>
        private void PingCommandReceiveHandler(string receiveData, TAccount Account,
            Action<string, SendTypeForCommand> sendDataForThisCommand)
        {
            if (DateTime.TryParse(receiveData, out _))
            {
                sendDataForThisCommand(receiveData, SendTypeForCommand.Asynchronous);
            }
            else if (ushort.TryParse(receiveData, out var receivePingResult))
            {
                _mainAccountUtilities.SessionHandler.Core_SetPing(receivePingResult);
                if (_logging.LogIsActive(LogsType.INFO))
                    _logging.LogInformation(Account.Session.GetSessionInfo(), G9LogIdentity.CLIENT_PING,
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
            if (_logging.LogIsActive(LogsType.INFO))
                _logging.LogInformation($"{LogMessage.CommandEcho}\n{LogMessage.ReceiveData}: {receiveData}",
                    G9LogIdentity.ECHO_COMMAND,
                    LogMessage.SuccessfulOperation);
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
            // Send receive data again
            sendDataForThisCommand(receiveData, SendTypeForCommand.Asynchronous);

            // Set log
            if (_logging.LogIsActive(LogsType.INFO))
                _logging.LogInformation(
                    $"{LogMessage.CommanTestSendReceive}\n{LogMessage.ReceiveData}: {receiveData}\n{LogMessage.TestNumber}: {_testCounter++}",
                    G9LogIdentity.TEST_SEND_RECEIVE, LogMessage.SuccessfulOperation);
        }

        #endregion

        #endregion
    }
}