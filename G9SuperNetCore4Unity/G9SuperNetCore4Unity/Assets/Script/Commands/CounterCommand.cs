using System;
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

            Console.WriteLine($"Receive Counter: {data}");
            sendAnswerWithReceiveRequestId(data + 1, CommandSendType.Asynchronous);
        }

        public override void OnError(Exception exceptionError, ClientAccountSample account)
        {
            throw new NotImplementedException();
        }
    }
}