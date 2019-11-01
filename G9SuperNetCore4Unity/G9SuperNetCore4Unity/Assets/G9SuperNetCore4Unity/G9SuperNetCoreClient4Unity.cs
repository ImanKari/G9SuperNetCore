using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using G9Common.Enums;
using G9SuperNetCoreClient.Client.Socket;
using G9SuperNetCoreClient.Config;
using G9SuperNetCoreClient.Sample;
using G9SuperNetCoreClientSampleApp.Commands;
using UnityEngine;

public class G9SuperNetCoreClient4Unity : MonoBehaviour
{

    private const string privateKey =
        "9ZdBx9VQ6D97XZwFlTjqR6QtL1hXZhkCIQCFTw1vlf9QO5ZdxnuqjfSeXj2A4hibPQdEiMu/mEgp2lIX5Tbvvskmz7ue7F1MYEWybe8kdq9ByLTQPBEuEMoiJxQr7Nqj";

    private G9SuperNetCoreSocketClient<ClientAccountSample, ClientSessionSample> _client;

    // Start is called before the first frame update
    async Task Start()
    {
     
        string ipAddress = "127.0.0.1";
        ushort port = 9639;
        _client =
            new G9SuperNetCoreSocketClient<ClientAccountSample, ClientSessionSample>(
                new G9ClientConfig(IPAddress.Parse(ipAddress), port, SocketMode.Tcp),
                Assembly.GetExecutingAssembly(), privateKey, Guid.NewGuid().ToString("P"));
        await _client.StartConnection();

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (CounterCommand.EnableCounterScript)
                CounterCommand.EnableCounterScript = false;
            else
            {
                CounterCommand.EnableCounterScript = true;
                _client.SendCommandAsync<CounterCommand, int>(0);
            }
        }
    }
}
