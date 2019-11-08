using System;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using G9Common.Enums;
using G9SuperNetCoreClient.Client.Socket;
using G9SuperNetCoreClient.Config;
using G9SuperNetCoreClient.Sample;
using G9SuperNetCoreClientSampleApp.Commands;

namespace G9SuperNetCoreClientSampleApp
{
    internal class Program
    {
        private static async Task Main()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(
                $@"
        __________ _____                      _____            __        __  _   __     __  _____              
       / ____/ __ / ___/__  ______  ___  ____/ ___/____  _____/ /_____  / /_/ | / ___  / /_/ ___/____  ________ 
      / / __/ /_/ \__ \/ / / / __ \/ _ \/ ___\__ \/ __ \/ ___/ //_/ _ \/ __/  |/ / _ \/ __/ /   / __ \/ ___/ _ \
     / /_/ /\__, ___/ / /_/ / /_/ /  __/ /  ___/ / /_/ / /__/ ,< /  __/ /_/ /|  /  __/ /_/ /___/ /_/ / /  /  __/
     \____//____/____/\__,_/ .___/\___/_/  /____/\____/\___/_/|_|\___/\__/_/ |_/\___/\__/\____/\____/_/   \___/ 
                 G9STUDIO /_/ CLIENT - V: {Assembly.GetExecutingAssembly().GetName().Version}
");
            Console.ResetColor();

            Console.WriteLine("Hello World!");
            Console.WriteLine("Enter Ip Address Like '127.0.0.1' Or '192.168.1.103':");
            var ipAddress = Console.ReadLine();
            Console.WriteLine("Enter Port Like '9639':");
            var port = Console.ReadLine();

            if (string.IsNullOrEmpty(ipAddress))
                ipAddress = "127.0.0.1";

            ushort intPort;

            if (string.IsNullOrEmpty(port))
                intPort = 9639;
            else
                intPort = ushort.Parse(port);

            await Task.Delay(963);

            const string privateKey =
                "9ZdBx9VQ6D97XZwFlTjqR6QtL1hXZhkCIQCFTw1vlf9QO5ZdxnuqjfSeXj2A4hibPQdEiMu/mEgp2lIX5Tbvvskmz7ue7F1MYEWybe8kdq9ByLTQPBEuEMoiJxQr7Nqj";

            var client1 =
                new G9SuperNetCoreSocketClient<ClientAccountSample, ClientSessionSample>(
                    new G9ClientConfig(IPAddress.Parse(ipAddress), intPort, SocketMode.Tcp),
                    Assembly.GetExecutingAssembly(), privateKey, Guid.NewGuid().ToString("P"));

            //G9SuperNetCoreSocketClient<ClientAccountSample, ClientSessionSample>[] clients = new G9SuperNetCoreSocketClient<ClientAccountSample, ClientSessionSample>[9999];

            await client1.StartConnection();

//            for (var i = 0; i < clients.Length - 1; i++)
//            {
//                clients[i] = new G9SuperNetCoreSocketClient<ClientAccountSample, ClientSessionSample>(
//                    new G9ClientConfig(IPAddress.Parse("127.0.0.1"), 9639, SocketMode.Tcp),
//                    Assembly.GetExecutingAssembly(), null, certificate);
//#pragma warning disable 4014
//                clients[i].StartConnection();
//#pragma warning restore 4014
//            }
//            clients[^1] = new G9SuperNetCoreSocketClient<ClientAccountSample, ClientSessionSample>(
//                    new G9ClientConfig(IPAddress.Parse("127.0.0.1"), 9639, SocketMode.Tcp), Assembly.GetExecutingAssembly(), null, certificate);
//            await clients[^1].StartConnection();

            Console.WriteLine("Connected all clients...");

            var counter = 0;
            string message;
            while ((message = Console.ReadLine()?.ToUpper()) != "Q")
                if (message == "STOP")
                {
                    await client1.Disconnect();
                }
                else if (message == "COUNTER")
                {
                    client1.SendCommandAsync<CounterCommand, int>(0);
                    //for (var i = 0; i < clients.Length; i++)
                    //{
                    //    clients[i].SendCommandAsync<CounterCommand, int>(0);
                    //}
                    Console.WriteLine("Active counter for all clients...");
                }
                else
                {
                    client1.SendCommandByName("G9EchoCommand", $"Client send a message {counter++}: {message}");
                }

            Console.WriteLine("Press any key to exist.");
            Console.ReadLine();
        }

    }

}