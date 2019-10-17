using System;
using G9Common.Enums;

namespace G9SuperNetCoreClient.ClientDefaultCommand
{
    internal static class G9EchoCommand
    {
        public const string G9CommandName = nameof(G9EchoCommand);

        public static void ErrorHandler(Exception exception, object Account)
        {
            
        }

        public static void ReceiveHandler(string receiveData, object Account, Action<string, SendTypeForCommand, Action<int>> sendDataForThisCommand)
        {
            Console.WriteLine(receiveData);
        }
    }
}
