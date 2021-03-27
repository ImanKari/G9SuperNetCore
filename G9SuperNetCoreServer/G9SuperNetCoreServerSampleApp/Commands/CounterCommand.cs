using System;
using G9SuperNetCoreCommon.Abstract;
using G9SuperNetCoreCommon.Enums;
using G9SuperNetCoreServer.Sample;

namespace G9SuperNetCoreClientSampleApp.Commands
{
    public class CounterCommand : AG9Command<int, ServerAccountSample>
    {
        public override void ReceiveCommand(int data, ServerAccountSample account, Guid requestId,
            Action<int, CommandSendType> sendAnswerWithReceiveRequestId)
        {
            Console.WriteLine($"Receive Counter: {data}");
            sendAnswerWithReceiveRequestId(data + 1, CommandSendType.Asynchronous);
        }

        public override void OnError(Exception exceptionError, ServerAccountSample account)
        {
            throw new NotImplementedException();
        }
    }
}