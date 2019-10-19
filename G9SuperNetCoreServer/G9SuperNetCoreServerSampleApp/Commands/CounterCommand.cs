using System;
using G9Common.Abstract;
using G9SuperNetCoreServer.Sample;

namespace G9SuperNetCoreClientSampleApp.Commands
{
    public class CounterCommand : AG9Command<int, ServerAccountSample>
    {
        public override void ReceiveCommand(int data, ServerAccountSample account)
        {
            Console.WriteLine($"Receive Counter: {data}");
            account.Session.SendCommandByNameAsync(nameof(CounterCommand), data+1);
        }

        public override void OnError(Exception exceptionError, ServerAccountSample account)
        {
            throw new NotImplementedException();
        }
    }
}