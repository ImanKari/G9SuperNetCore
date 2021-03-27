using System;
using System.IO;
using G9SuperNetCoreCommon.Abstract;
using G9SuperNetCoreCommon.Enums;
using G9SuperNetCoreServer.Sample;

namespace G9SuperNetCoreClientSampleApp.Commands
{
    public class FileTransfer : AG9Command<byte[], ServerAccountSample>
    {
        public override void ReceiveCommand(byte[] data, ServerAccountSample account, Guid requestId,
            Action<byte[], CommandSendType> sendAnswerWithReceiveRequestId)
        {
            Console.WriteLine($"Receive File Transfer Length: {data.Length}");
            using var file = File.Create("test.png");
            file.Write(data);
        }

        public override void OnError(Exception exceptionError, ServerAccountSample account)
        {
            throw new NotImplementedException();
        }
    }
}