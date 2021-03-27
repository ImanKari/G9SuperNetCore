using System;
using G9SuperNetCoreCommon.Abstract;
using G9SuperNetCoreCommon.Enums;
using G9SuperNetCoreServer.Sample;
using G9SuperNetCoreServerSampleApp_GameServer.AccountAndSession;
using G9SuperNetCoreServerSampleApp_GameServer.Core;

namespace G9SuperNetCoreServerSampleApp_GameServer.Commands
{
    public class CPlayerDisconnected : AG9Command<long, bool, GameAccount>
    {
        public override void ReceiveCommand(bool data, GameAccount account, Guid requestId, Action<long, CommandSendType> sendAnswerWithReceiveRequestId)
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