using System;
using G9Common.Abstract;
using G9SuperNetCoreClient.Sample;

namespace G9SuperNetCoreClientSampleApp.Commands
{
    public class CounterCommand : AG9Command<int, ClientAccountSample>
    {
        public override void ReceiveCommand(int data, ClientAccountSample account)
        {
            Console.WriteLine($"Receive Counter: {data}");
            account.Session.SendCommandAsync<CounterCommand, int>(data + 1);
        }

        public override void OnError(Exception exceptionError, ClientAccountSample account)
        {
            throw new NotImplementedException();
        }
    }
}