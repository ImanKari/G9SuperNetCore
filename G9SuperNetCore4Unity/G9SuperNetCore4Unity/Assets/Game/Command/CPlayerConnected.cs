using System;
using G9Common.Abstract;
using G9Common.Enums;
using G9SuperNetCoreServerSampleApp_GameServer.AccountAndSession;

namespace G9SuperNetCoreServerSampleApp_GameServer.Commands
{
    public class CPlayerConnected : AG9Command<bool, long, GameAccount>
    {
        public override void ReceiveCommand(long data, GameAccount account, Guid requestId,
            Action<bool, CommandSendType> sendAnswerWithReceiveRequestId)
        {
            account.AccessToGameCore.GenerateTotalPlayer(data);
        }

        public override void OnError(Exception exceptionError, GameAccount account)
        {
            throw new NotImplementedException();
        }
    }
}