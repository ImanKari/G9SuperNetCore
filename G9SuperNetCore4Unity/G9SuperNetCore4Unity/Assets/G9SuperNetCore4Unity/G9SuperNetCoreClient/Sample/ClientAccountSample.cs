using System;
using G9Common.Enums;
using G9Common.Resource;
using G9SuperNetCoreClient.Abstract;
using G9SuperNetCoreClient.Enums;

namespace G9SuperNetCoreClient.Sample
{
    public class ClientAccountSample : AClientAccount<ClientSessionSample>
    {
        public override void OnSessionClosed(DisconnectReason reason)
        {
            Console.WriteLine($"{LogMessage.OnSessionClose}\n{LogMessage.CloseReason}: {reason.ToString()}");
        }
    }
}