using System;
using System.IO;
using G9Common.Abstract;
using G9Common.Enums;
using G9SuperNetCoreClient.Sample;

namespace G9SuperNetCoreClientSampleApp.Commands
{
    public class FileTransfer : AG9Command<byte[], ClientAccountSample>
    {
        public override void ReceiveCommand(byte[] data, ClientAccountSample account, Guid requestId,
            Action<byte[], CommandSendType> sendAnswerWithReceiveRequestId)
        {
            Console.WriteLine($"Receive File Transfer Length: {data.Length}");
            using var file = File.Create("test.jpg");
            file.Write(data);
        }

        public override void OnError(Exception exceptionError, ClientAccountSample account)
        {
            throw new NotImplementedException();
        }
    }
}