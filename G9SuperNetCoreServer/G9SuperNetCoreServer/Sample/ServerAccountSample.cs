﻿using System;
using G9Common.Enums;
using G9Common.Resource;
using G9SuperNetCoreServer.Abstarct;
using G9SuperNetCoreServer.Enums;

namespace G9SuperNetCoreServer.Sample
{
    public class ServerAccountSample : AServerAccount<ServerSessionSample>
    {
        public override void OnSessionClosed(DisconnectReason reason)
        {
            Console.WriteLine($"{LogMessage.OnSessionClose}\n{LogMessage.CloseReason}: {reason.ToString()}");
        }
    }
}