using System;
using G9Common.Abstract;
using G9Common.Enums;
using G9SuperNetCoreServerSampleApp_GameServer.AccountAndSession;
using G9SuperNetCoreServerSampleApp_GameServer.Core;

namespace G9SuperNetCoreServerSampleApp_GameServer.Commands
{
    public class CPlayerConnected : AG9Command<long, bool, GameAccount>
    {
        public override void ReceiveCommand(bool data, GameAccount account, Guid requestId,
            Action<long, CommandSendType> sendAnswerWithReceiveRequestId)
        {
            GameCore.AddNewGameAccount(account);
        }

        public override void OnError(Exception exceptionError, GameAccount account)
        {
            throw new NotImplementedException();
        }
    }
}