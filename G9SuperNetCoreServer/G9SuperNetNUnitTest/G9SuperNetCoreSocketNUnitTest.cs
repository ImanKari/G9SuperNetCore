using System;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using G9Common.Configuration;
using G9Common.Enums;
using G9SuperNetCoreClient.Client.Socket;
using G9SuperNetCoreClient.Config;
using G9SuperNetCoreClient.Sample;
using G9SuperNetCoreServer;
using G9SuperNetCoreServer.Config;
using G9SuperNetCoreServer.Sample;
using NUnit.Framework;

namespace G9SuperNetNUnitTest
{
    public class G9SuperNetCoreSocketNUnitTest
    {

        public G9SuperNetCoreSocketServer<ServerAccountSample, ServerSessionSample> Server;

        public G9SuperNetCoreSocketClient<ClientAccountSample, ClientSessionSample> Client;


        [SetUp]
        public void Setup()
        {
        }

        [Test, Order(1)]
        public void InitializeServer()
        {
            Server = new G9SuperNetCoreSocketServer<ServerAccountSample, ServerSessionSample>(
                new G9ServerConfig("Test Server", IPAddress.Any, 9639, SocketMode.Tcp), Assembly.GetExecutingAssembly());
        }

        [Test, Order(2)]
        public void InitializeClient()
        {
            Client = new G9SuperNetCoreSocketClient<ClientAccountSample, ClientSessionSample>(
                new G9ClientConfig(IPAddress.Parse("127.0.0.1"), 9639, SocketMode.Tcp), Assembly.GetExecutingAssembly());
        }

        [Test, Order(3)]
        public async Task StartServer()
        {
            await Server.Start();
        }

        [Test, Order(4)]
        public async Task StartClient()
        {
            Thread.Sleep(639);
            Assert.True(await Client.StartConnection());
            Thread.Sleep(639);
        }

        [Test, Order(5)]
        public void ServerSend1000TimeTest()
        {
            for (var i = 0; i < 1000; i++)
            {
                Server.SendCommandToAllByName("G9ECHO", $"G9 Net Core Serve - Server Request {i}");
            }
            Thread.Sleep(9369);
        }

        [Test, Order(6)]
        public void ClientSend1000TimeTest()
        {
            for (var i = 0; i < 1000; i++)
            {
                Client.SendCommandByName("G9ECHO", $"G9 Net Core Serve - Client Request {i}");
            }

            Thread.Sleep(9369);
        }
    }
}