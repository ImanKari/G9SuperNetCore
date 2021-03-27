using System;
using G9SuperNetCoreCommon.Abstract;
using G9SuperNetCoreCommon.Enums;
using G9SuperNetCoreServerSampleApp_GameServer.AccountAndSession;
using G9SuperNetCoreServerSampleApp_GameServer.Commands.Struct;

namespace G9SuperNetCoreServerSampleApp_GameServer.Commands
{
    public class CGetPlayersPosition : AG9Command<G9DtSingTotalPlayers, bool, GameAccount>
    {
        public override void ReceiveCommand(bool data, GameAccount account, Guid requestId,
            Action<G9DtSingTotalPlayers, CommandSendType> sendAnswerWithReceiveRequestId)
        {
            // Just use command for send
            throw new NotImplementedException();
        }

        public override void OnError(Exception exceptionError, GameAccount account)
        {
            throw new NotImplementedException();
        }
    }
}