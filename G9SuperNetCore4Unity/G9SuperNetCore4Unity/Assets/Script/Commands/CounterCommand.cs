using System;
using System.Diagnostics;
using G9Common.Abstract;
using G9Common.Enums;
using G9SuperNetCoreClient.Sample;

namespace G9SuperNetCoreClientSampleApp.Commands
{
    public class CounterCommand : AG9Command<int, ClientAccountSample>
    {

        public static bool EnableCounterScript = false;

        public override void ReceiveCommand(int data, ClientAccountSample account, Guid requestId,
            Action<int, CommandSendType> sendAnswerWithReceiveRequestId)
        {
            if (!EnableCounterScript) return;

            Debug.WriteLine($"Send : CounterCommand | UniqueId: {requestId} | {DateTime.Now:HH:mm:ss.fff}");
            sendAnswerWithReceiveRequestId(data + 1, CommandSendType.Asynchronous);
        }

        public override void OnError(Exception exceptionError, ClientAccountSample account)
        {
            throw new NotImplementedException();
        }
    }
}