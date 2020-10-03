using System;
using G9Common.Abstract;
using G9Common.Enums;
using G9SuperNetCoreServerSampleApp_GameServer.AccountAndSession;

namespace G9SuperNetCoreServerSampleApp_GameServer.Commands
{
    public class CVoice : AG9Command<float[], GameAccount>
    {
        public override void ReceiveCommand(float[] data, GameAccount account, Guid requestId, Action<float[], CommandSendType> sendAnswerWithReceiveRequestId)
        {
            account.AccessToGameCore.PlayVoice(data);
        }

        public override void OnError(Exception exceptionError, GameAccount account)
        {
            throw new NotImplementedException();
        }
    }
}
