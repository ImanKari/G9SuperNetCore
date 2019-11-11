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
using UnityEngine;

// ReSharper disable once CheckNamespace
public class G9SuperNetCoreClient4Unity : G9SuperNetCoreClient4UnityHelper
{

    #region Fields And Properties

    /// <summary>
    ///     Specified Client object
    /// </summary>
    public static G9SuperNetCoreSocketClient<ClientAccountSample, ClientSessionSample> G9SuperNetCoreClient;

    /// <summary>
    ///     Specify Validation is true or false
    /// </summary>
    public bool Validation;

    /// <summary>
    ///     Specified Enable auto start connection for client. connected automatic to server in first time after start game
    /// </summary>
    public bool EnableAutoStartConnection;

    /// <summary>
    ///     Field for save server ip address
    /// </summary>
    public IPAddress ServerIpAddress = IPAddress.Parse("192.168.1.103");

    /// <summary>
    ///     Specified port
    /// </summary>
    [Range(10, ushort.MaxValue)] public ushort Port = 9639;

    /// <summary>
    ///     Specified ssl connection is enable
    /// </summary>
    public bool EnableSecureConnection = true;

    /// <summary>
    ///     Specified private key
    /// </summary>
    public string PrivateKey =
        "9ZdBx9VQ6D97XZwFlTjqR6QtL1hXZhkCIQCFTw1vlf9QO5ZdxnuqjfSeXj2A4hibPQdEiMu/mEgp2lIX5Tbvvskmz7ue7F1MYEWybe8kdq9ByLTQPBEuEMoiJxQr7Nqj";

    /// <summary>
    ///     Specified unique identity for connection
    /// </summary>
    public string UniqueIdentity;

    #endregion Fields And Properties

    #region Methods

    /// <summary>
    ///     Start is called before the first frame update
    /// </summary>
    /// <returns></returns>

    #region Start

    private async Task Start()
    {
        if (!Validation)
            throw new Exception("Invalid config exception. Please check G9NetCoreClient configuration!");

        // Initialize
        G9SuperNetCoreClient =
            EnableSecureConnection
                ? new G9SuperNetCoreSocketClient<ClientAccountSample, ClientSessionSample>(
                    new G9ClientConfig(ServerIpAddress, Port, SocketMode.Tcp),
                    Assembly.GetExecutingAssembly(), PrivateKey, UniqueIdentity)
                : new G9SuperNetCoreSocketClient<ClientAccountSample, ClientSessionSample>(
                    new G9ClientConfig(ServerIpAddress, Port, SocketMode.Tcp),
                    Assembly.GetExecutingAssembly());

        if (EnableAutoStartConnection)
            await G9SuperNetCoreClient.StartConnection();
    }

    #endregion

    /// <summary>
    ///     Handle send and receive in fixed update
    /// </summary>

    #region FixedUpdate

    private void FixedUpdate()
    {
        // Handle send receive in frame
        HandleSendReceiveInFrame();
    }

    #endregion

    /// <summary>
    ///     On application quit - disconnect from server
    /// </summary>

    #region OnApplicationQuit

    private void OnApplicationQuit()
    {
        if (G9SuperNetCoreClient != null)
        {
            G9SuperNetCoreClient.Disconnect().Wait(369);
            G9SuperNetCoreClient = null;
            Thread.Sleep(3639);
        }
    }

    #endregion

    #endregion Methods

}