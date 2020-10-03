using System;
using G9Common.Abstract;
using G9Common.Enums;
using G9SuperNetCoreServerSampleApp_GameServer.AccountAndSession;
using G9SuperNetCoreServerSampleApp_GameServer.Commands.Struct;

namespace G9SuperNetCoreServerSampleApp_GameServer.Commands
{
    public class CGetPlayersPosition : AG9Command<bool, G9DtSingTotalPlayers, GameAccount>
    {
        public override void ReceiveCommand(G9DtSingTotalPlayers data, GameAccount account, Guid requestId,
            Action<bool, CommandSendType> sendAnswerWithReceiveRequestId)
        {
            // Just use command for send
            account.PlayerIdentity = data.YourIdentity;
            account.AccessToGameCore.GenerateTotalPlayer(data);
        }

        public override void OnError(Exception exceptionError, GameAccount account)
        {
            throw new NotImplementedException();
        }
    }
}