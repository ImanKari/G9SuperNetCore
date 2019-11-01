using System;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using G9Common.Enums;
using G9Common.HelperClass;
using G9SuperNetCoreClientSampleApp.Commands;
using G9SuperNetCoreServer;
using G9SuperNetCoreServer.Config;
using G9SuperNetCoreServer.Sample;

namespace G9SuperNetCoreServerSampleApp
{
    internal class Program
    {
        private static async Task Main()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(
                $@"
        __________ _____                      _____            __        __  _   __     __  _____              
       / ____/ __ / ___/__  ______  ___  ____/ ___/____  _____/ /_____  / /_/ | / ___  / /_/ ___/____  ________ 
      / / __/ /_/ \__ \/ / / / __ \/ _ \/ ___\__ \/ __ \/ ___/ //_/ _ \/ __/  |/ / _ \/ __/ /   / __ \/ ___/ _ \
     / /_/ /\__, ___/ / /_/ / /_/ /  __/ /  ___/ / /_/ / /__/ ,< /  __/ /_/ /|  /  __/ /_/ /___/ /_/ / /  /  __/
     \____//____/____/\__,_/ .___/\___/_/  /____/\____/\___/_/|_|\___/\__/_/ |_/\___/\__/\____/\____/_/   \___/ 
                 G9STUDIO /_/ SERVER - V: {Assembly.GetExecutingAssembly().GetName().Version}
");

            Console.WriteLine("Hello World!");
            Console.ResetColor();

            const string privateKey =
                "9ZdBx9VQ6D97XZwFlTjqR6QtL1hXZhkCIQCFTw1vlf9QO5ZdxnuqjfSeXj2A4hibPQdEiMu/mEgp2lIX5Tbvvskmz7ue7F1MYEWybe8kdq9ByLTQPBEuEMoiJxQr7Nqj";

            var sslCertificate = new G9SslCertificate(privateKey, 9, "IR");

            var server = new G9SuperNetCoreSocketServer<ServerAccountSample, ServerSessionSample>(
                new G9ServerConfig("Test Server", IPAddress.Any, 9639, SocketMode.Tcp), Assembly.GetExecutingAssembly(),
                null, sslCertificate);

#pragma warning disable 4014
            server.Start();
#pragma warning restore 4014

            var counter = 0;
            string message;
            while ((message = Console.ReadLine()?.ToUpper()) != "Q")
                if (message == "G9TEST")
                {
                    if (server.EnableCommandTestSendReceiveAllClients)
                        server.DisableCommandTestSendAndReceiveForAllClients();
                    else
                        server.EnableCommandTestSendAndReceiveForAllClients();
                }
                else if (message == "STOP")
                {
                    await server.Stop();
                }
                else if (message == "COUNTER")
                {
                    server.SendCommandToAllByName(nameof(CounterCommand), 0);
                }
                else if (message == "INFO")
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine(server.GetServerInfo());
                    Console.ResetColor();
                }
                else
                {
                    server.SendCommandToAllByName("G9EchoCommand", $"Server send a message {counter++}: {message}");
                }

            Console.WriteLine("Press any key to exist.");
            Console.ReadLine();
        }
    }
}