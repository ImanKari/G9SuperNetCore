using System;
using G9Common.Enums;

namespace G9Common.DefaultCommonCommand
{
    public static class G9TestSendReceive
    {
        public const string G9CommandName = nameof(G9TestSendReceive);

        private static int _testCounter = 0;

        public static void ErrorHandler(Exception exception, object Account)
        {
        }

        public static void ReceiveHandler(string receiveData, object Account,
            Action<string, SendTypeForCommand, Action<int>> sendDataForThisCommand)
        {
            Console.WriteLine($"Test{_testCounter++} Receive: {receiveData}");
            sendDataForThisCommand(receiveData, SendTypeForCommand.Asynchronous, null);
        }
    }
}