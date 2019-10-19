using System;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using G9Common.Enums;
using G9SuperNetCoreClient.Client.Socket;
using G9SuperNetCoreClient.Config;
using G9SuperNetCoreClient.Sample;
using G9SuperNetCoreClientSampleApp.Commands;

namespace G9SuperNetCoreClientSampleApp
{
    class Program
    {
        static async Task Main()
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

            await Task.Delay(963);
            Console.WriteLine("Hello World!");
            var client1 =
                new G9SuperNetCoreSocketClient<ClientAccountSample, ClientSessionSample>(
                    new G9ClientConfig(IPAddress.Parse("127.0.0.1"), 9639, SocketMode.Tcp), Assembly.GetExecutingAssembly());
            G9SuperNetCoreSocketClient<ClientAccountSample, ClientSessionSample>[] clients = new G9SuperNetCoreSocketClient<ClientAccountSample, ClientSessionSample>[999];

            await client1.StartConnection();

            for (var i = 0; i < clients.Length; i++)
            {
                clients[i] = new G9SuperNetCoreSocketClient<ClientAccountSample, ClientSessionSample>(
                    new G9ClientConfig(IPAddress.Parse("127.0.0.1"), 9639, SocketMode.Tcp), Assembly.GetExecutingAssembly());
                await clients[i].StartConnection();
            }

            Console.WriteLine("Connected all clients...");

            int counter = 0;
            string message;
            while ((message = Console.ReadLine()?.ToUpper()) != "Q")
            {
                if (message == "G9TEST")
                {
                    for (var i = 0; i < 100; i++)
                    {
                        client1.SendCommandByName("G9EchoCommand", $"Client send a message {counter++}: {i}");
                    }
                }
                else if (message == "STOP")
                {
                    await client1.Disconnect();
                }
                else if (message == "COUNTER")
                {
                    client1.SendCommandAsync<CounterCommand, int>(0);
                    for (var i = 0; i < clients.Length; i++)
                    {
                        clients[i].SendCommandAsync<CounterCommand, int>(0);
                    }
                    Console.WriteLine("Active counter for all clients...");
                }
                else
                    client1.SendCommandByName("G9TestSendReceive", $"Client send a message {counter++}: {message}");
            }

            Console.WriteLine("Press any key to exist.");
            Console.ReadLine();
        }
    }
}
