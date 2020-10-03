using System;
using System.Collections.Generic;
using System.Text;
using G9Common.Abstract;
using G9Common.Enums;
using G9SuperNetCoreServerSampleApp_GameServer.AccountAndSession;
using G9SuperNetCoreServerSampleApp_GameServer.Commands.Struct;
using G9SuperNetCoreServerSampleApp_GameServer.Core;

namespace G9SuperNetCoreServerSampleApp_GameServer.Commands
{
    public class CVoice : AG9Command<float[], GameAccount>
    {
        public override void ReceiveCommand(float[] data, GameAccount account, Guid requestId, Action<float[], CommandSendType> sendAnswerWithReceiveRequestId)
        {
            GameCore.VoiceRecord(account, data);
        }

        public override void OnError(Exception exceptionError, GameAccount account)
        {
            throw new NotImplementedException();
        }
    }
}
