using System;
using G9SuperNetCoreCommon.Abstract;
using G9SuperNetCoreCommon.Enums;
using G9SuperNetCoreServerSampleApp_GameServer.AccountAndSession;
using G9SuperNetCoreServerSampleApp_GameServer.Commands.Struct;
using G9SuperNetCoreServerSampleApp_GameServer.Core;

namespace G9SuperNetCoreServerSampleApp_GameServer.Commands
{
    public class CPlayerMove : AG9Command<G9DtPlayerMove, GameAccount>
    {
        public override void ReceiveCommand(G9DtPlayerMove data, GameAccount account, Guid requestId,
            Action<G9DtPlayerMove, CommandSendType> sendAnswerWithReceiveRequestId)
        {
            account.LastPlayerPosition = data.NewPosition;
            GameCore.MoveGameAccount(account);
        }

        public override void OnError(Exception exceptionError, GameAccount account)
        {
            throw new NotImplementedException();
        }
    }
}