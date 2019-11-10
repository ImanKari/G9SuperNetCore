using System;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using G9Common.Enums;
using G9SuperNetCoreClient.AbstractClient;
using G9SuperNetCoreClient.Client.Socket;
using G9SuperNetCoreClient.Config;
using G9SuperNetCoreClient.Sample;
using G9SuperNetCoreClientSampleApp.Commands;
using UnityEngine;

public class G9SuperNetCoreClient4Unity : G9SuperNetCoreClient4UnityHelper
{
    private const string privateKey =
        "9ZdBx9VQ6D97XZwFlTjqR6QtL1hXZhkCIQCFTw1vlf9QO5ZdxnuqjfSeXj2A4hibPQdEiMu/mEgp2lIX5Tbvvskmz7ue7F1MYEWybe8kdq9ByLTQPBEuEMoiJxQr7Nqj";

    private G9SuperNetCoreSocketClient<ClientAccountSample, ClientSessionSample> _client;

    private int _counter;

    // Start is called before the first frame update
    private async Task Start()
    {
        var ipAddress = "192.168.1.103";
        ushort port = 9639;
        _client =
            new G9SuperNetCoreSocketClient<ClientAccountSample, ClientSessionSample>(
                new G9ClientConfig(IPAddress.Parse(ipAddress), port, SocketMode.Tcp),
                Assembly.GetExecutingAssembly(), privateKey, Guid.NewGuid().ToString("P"));
        await _client.StartConnection();
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            if (CounterCommand.EnableCounterScript)
                CounterCommand.EnableCounterScript = false;
            else
            {
                CounterCommand.EnableCounterScript = true;
                _client.SendCommandAsync<CounterCommand, int>(0);
            }
        }

        if (Input.GetMouseButton(0))
        {
            _client.SendCommandByNameAsync(nameof(G9ReservedCommandName.G9EchoCommand), _counter.ToString());
            _counter++;
        }


        // Handle send receive in frame
        HandleSendReceiveInFrame();
    }

    private void OnApplicationQuit()
    {
        if (_client != null)
        {
            _client.Disconnect().Wait(369);
            _client = null;
            Thread.Sleep(3639);
        }
    }
}