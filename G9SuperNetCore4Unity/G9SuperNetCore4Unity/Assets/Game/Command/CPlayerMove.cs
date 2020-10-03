using System;
using G9Common.Abstract;
using G9Common.Enums;
using G9SuperNetCoreServerSampleApp_GameServer.AccountAndSession;
using G9SuperNetCoreServerSampleApp_GameServer.Commands.Struct;

namespace G9SuperNetCoreServerSampleApp_GameServer.Commands
{
    public class CPlayerMove : AG9Command<G9DtPlayerMove, GameAccount>
    {
        public override void ReceiveCommand(G9DtPlayerMove data, GameAccount account, Guid requestId,
            Action<G9DtPlayerMove, CommandSendType> sendAnswerWithReceiveRequestId)
        {
            account.AccessToGameCore.MovePlayer(data);
        }

        public override void OnError(Exception exceptionError, GameAccount account)
        {
            throw new NotImplementedException();
        }
    }
}