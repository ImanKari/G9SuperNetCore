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
            var client2 =
                new G9SuperNetCoreSocketClient<ClientAccountSample, ClientSessionSample>(
                    new G9ClientConfig(IPAddress.Parse("127.0.0.1"), 9639, SocketMode.Tcp), Assembly.GetExecutingAssembly());
            var client3 =
                new G9SuperNetCoreSocketClient<ClientAccountSample, ClientSessionSample>(
                    new G9ClientConfig(IPAddress.Parse("127.0.0.1"), 9639, SocketMode.Tcp), Assembly.GetExecutingAssembly());
            var client4 =
                new G9SuperNetCoreSocketClient<ClientAccountSample, ClientSessionSample>(
                    new G9ClientConfig(IPAddress.Parse("127.0.0.1"), 9639, SocketMode.Tcp), Assembly.GetExecutingAssembly());
            var client5 =
                new G9SuperNetCoreSocketClient<ClientAccountSample, ClientSessionSample>(
                    new G9ClientConfig(IPAddress.Parse("127.0.0.1"), 9639, SocketMode.Tcp), Assembly.GetExecutingAssembly());
            var client6 =
                new G9SuperNetCoreSocketClient<ClientAccountSample, ClientSessionSample>(
                    new G9ClientConfig(IPAddress.Parse("127.0.0.1"), 9639, SocketMode.Tcp), Assembly.GetExecutingAssembly());
            var client7 =
                new G9SuperNetCoreSocketClient<ClientAccountSample, ClientSessionSample>(
                    new G9ClientConfig(IPAddress.Parse("127.0.0.1"), 9639, SocketMode.Tcp), Assembly.GetExecutingAssembly());
            var client8 =
                new G9SuperNetCoreSocketClient<ClientAccountSample, ClientSessionSample>(
                    new G9ClientConfig(IPAddress.Parse("127.0.0.1"), 9639, SocketMode.Tcp), Assembly.GetExecutingAssembly());
            var client9 =
                new G9SuperNetCoreSocketClient<ClientAccountSample, ClientSessionSample>(
                    new G9ClientConfig(IPAddress.Parse("127.0.0.1"), 9639, SocketMode.Tcp), Assembly.GetExecutingAssembly());

            await client1.StartConnection();
            await client2.StartConnection();
            await client3.StartConnection();
            await client4.StartConnection();
            await client5.StartConnection();
            await client6.StartConnection();
            await client7.StartConnection();
            await client8.StartConnection();
            await client9.StartConnection();

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
                    client1.SendCommandByNameAsync(nameof(CounterCommand), 0);
                    client2.SendCommandByNameAsync(nameof(CounterCommand), 0);
                    client3.SendCommandByNameAsync(nameof(CounterCommand), 0);
                    client4.SendCommandByNameAsync(nameof(CounterCommand), 0);
                    client5.SendCommandByNameAsync(nameof(CounterCommand), 0);
                    client6.SendCommandByNameAsync(nameof(CounterCommand), 0);
                    client7.SendCommandByNameAsync(nameof(CounterCommand), 0);
                    client8.SendCommandByNameAsync(nameof(CounterCommand), 0);
                    client9.SendCommandByNameAsync(nameof(CounterCommand), 0);
                }
                else
                    client1.SendCommandByName("G9TestSendReceive", $"Client send a message {counter++}: {message}");
            }

            Console.WriteLine("Press any key to exist.");
            Console.ReadLine();
        }
    }
}
