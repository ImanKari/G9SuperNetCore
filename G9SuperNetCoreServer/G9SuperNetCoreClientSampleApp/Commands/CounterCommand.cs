using System;
using G9SuperNetCoreCommon.Abstract;
using G9SuperNetCoreCommon.Enums;
using G9SuperNetCoreClient.Sample;

namespace G9SuperNetCoreClientSampleApp.Commands
{
    public class CounterCommand : AG9Command<int, ClientAccountSample>
    {
        public override void ReceiveCommand(int data, ClientAccountSample account, Guid requestId,
            Action<int, CommandSendType> sendAnswerWithReceiveRequestId)
        {
            Console.WriteLine($"Receive Counter: {data}");
            sendAnswerWithReceiveRequestId(data + 1, CommandSendType.Asynchronous);
        }

        public override void OnError(Exception exceptionError, ClientAccountSample account)
        {
            throw new NotImplementedException();
        }
    }
}