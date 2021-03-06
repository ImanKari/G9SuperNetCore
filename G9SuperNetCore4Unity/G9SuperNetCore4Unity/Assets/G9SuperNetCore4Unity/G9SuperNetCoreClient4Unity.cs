using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using G9Common.Enums;
using G9SuperNetCoreClient.Abstract;
using G9SuperNetCoreClient.AbstractClient;
using G9SuperNetCoreClient.Client.Socket;
using G9SuperNetCoreClient.Config;
using UnityEngine;

[Serializable]
[DefaultExecutionOrder(-799)]
// ReSharper disable once CheckNamespace
public class G9SuperNetCoreClient4Unity : G9SuperNetCoreClient4UnityHelper
{
    /// <summary>
    ///     Specified version of Super Net Core 4 Unity
    /// </summary>
    public const string Version = "2.2.3.0";

    #region Fields And Properties

    /// <summary>
    ///     Specified game is playing or no
    /// </summary>
    public static bool GameIsPlaying { private set; get; }

    /// <summary>
    ///     Specified Client object
    /// </summary>
    private static object _g9SuperNetCoreClient;

    /// <summary>
    ///     Access to disconnect server method, used in OnApplicationQuit
    /// </summary>
    public static Action DisconnectServerMethod { get; private set; }

    /// <summary>
    ///     Specify Validation is true or false
    /// </summary>
    public bool Validation;

    private static bool _validation;

    /// <summary>
    ///     Field for save server ip address
    /// </summary>
    public string ServerIpAddress = "192.168.1.103";

    private static string _serverIpAddress;

    /// <summary>
    ///     Specified port
    /// </summary>
    [Range(10, ushort.MaxValue)] public ushort Port;

    private static ushort _port;

    /// <summary>
    ///     Specified ssl connection is enable
    /// </summary>
    public bool EnableSecureConnection = true;

    private static bool _enableSecureConnection;

    /// <summary>
    ///     Specified private key
    /// </summary>
    public string PrivateKey =
        "9ZdBx9VQ6D97XZwFlTjqR6QtL1hXZhkCIQCFTw1vlf9QO5ZdxnuqjfSeXj2A4hibPQdEiMu/mEgp2lIX5Tbvvskmz7ue7F1MYEWybe8kdq9ByLTQPBEuEMoiJxQr7Nqj";

    private static string _privateKey;

    /// <summary>
    ///     Specified unique identity for connection
    /// </summary>
    public string UniqueIdentity;

    private static string _uniqueIdentity;

    /// <summary>
    ///     Access to events handler
    /// </summary>
    public static G9Events4Unity AccessToG9Events4Unity;

    #endregion Fields And Properties

    #region Methods

    /// <summary>
    ///     Run automatic in first time just one time
    /// </summary>

    #region Awake

    // ReSharper disable once UnusedMember.Local
    private void Awake()
    {
        // Set settings
        _validation = Validation;
        _serverIpAddress = ServerIpAddress;
        _port = Port;
        _enableSecureConnection = EnableSecureConnection;
        _privateKey = PrivateKey;
        _uniqueIdentity = UniqueIdentity;

        // Set events object
        AccessToG9Events4Unity = GetComponent<G9Events4Unity>();
    }

    #endregion

    /// <summary>
    ///     Initialize client with custom account and session
    /// </summary>
    /// <typeparam name="TAccount">Specified type of account</typeparam>
    /// <typeparam name="TSession">Specified type of session</typeparam>
    /// <returns>Instance of client</returns>

    #region Initialize

    public static async Task<G9SuperNetCoreSocketClient<TAccount, TSession>> Initialize<TAccount, TSession>(
        TAccount gameAccount)
        where TAccount : AClientAccount<TSession>, new()
        where TSession : AClientSession, new()
    {
        return await Task.Run(() =>
        {
            // Check validation
            if (!_validation)
                throw new Exception("Invalid config exception. Please check G9NetCoreClient configuration!");

            // Check client initialized
            if (_g9SuperNetCoreClient != null)
                throw new Exception("Client initialize just one time!");

            // Initialize
            _g9SuperNetCoreClient = _enableSecureConnection
                ? new G9SuperNetCoreSocketClient<TAccount, TSession>(
                    new G9ClientConfig(IPAddress.Parse(_serverIpAddress), _port, SocketMode.Tcp),
                    privateKeyForSslConnection: _privateKey,
                    clientUniqueIdentity: _uniqueIdentity,
                    customAccount: gameAccount)
                : new G9SuperNetCoreSocketClient<TAccount, TSession>(
                    new G9ClientConfig(IPAddress.Parse(_serverIpAddress), _port, SocketMode.Tcp),
                    customAccount: gameAccount);

            // Set disconnect action
            DisconnectServerMethod = () =>
                (_g9SuperNetCoreClient as G9SuperNetCoreSocketClient<TAccount, TSession>)?.Disconnect();

            // Set event handler
            InitializeEvents((G9SuperNetCoreSocketClient<TAccount, TSession>) _g9SuperNetCoreClient);

            return (G9SuperNetCoreSocketClient<TAccount, TSession>) _g9SuperNetCoreClient;
        });
    }

    #endregion

    /// <summary>
    ///     Initialize events after initialize client
    /// </summary>
    /// <typeparam name="TAccount">Specified type of account</typeparam>
    /// <typeparam name="TSession">Specified type of session</typeparam>
    /// <param name="superNetCoreSocketClient">Instance of client</param>

    #region InitializeEvents

    private static void InitializeEvents<TAccount, TSession>(
        G9SuperNetCoreSocketClient<TAccount, TSession> superNetCoreSocketClient)
        where TAccount : AClientAccount<TSession>, new()
        where TSession : AClientSession, new()
    {
        // On connected
        superNetCoreSocketClient.OnConnected += account =>
            EventsQueue.Enqueue(() => AccessToG9Events4Unity.OnConnectedHandler?.Invoke(account));


        // On disconnect
        superNetCoreSocketClient.OnDisconnected += (account, reason) =>
            EventsQueue.Enqueue(() => AccessToG9Events4Unity.OnDisconnectedHandler?.Invoke(account, reason));

        // On error
        superNetCoreSocketClient.OnError += (error, reason) =>
            EventsQueue.Enqueue(() => AccessToG9Events4Unity.OnErrorHandler?.Invoke(error, reason));

        // On reconnect
        superNetCoreSocketClient.OnReconnect +=
            (account, tryReconnectNumber) =>
                EventsQueue.Enqueue(
                    () => AccessToG9Events4Unity.OnReconnectHandler?.Invoke(account, tryReconnectNumber));

        // On unhandled command
        superNetCoreSocketClient.OnUnhandledCommand +=
            (packet, account) =>
                EventsQueue.Enqueue(() => AccessToG9Events4Unity.OnUnhandledCommand?.Invoke(packet, account));

        // On unable to connect
        superNetCoreSocketClient.OnUnableToConnect +=
            () =>
                EventsQueue.Enqueue(() => AccessToG9Events4Unity.OnUnableToConnect?.Invoke());
    }

    #endregion

    /// <summary>
    ///     Handle send, receive and events in update
    /// </summary>

    #region Update

    // ReSharper disable once UnusedMember.Local
    private void Update()
    {
        // Set game is playing
        GameIsPlaying = Application.isPlaying;
        // Handle send receive in frame
        HandleSendReceiveAndEventsInFrame();
    }

    #endregion

    /// <summary>
    ///     On application quit - disconnect from server
    /// </summary>

    #region OnApplicationQuit

    // ReSharper disable once UnusedMember.Local
    private void OnApplicationQuit()
    {
        GameIsPlaying = false;
        if (_g9SuperNetCoreClient == null) return;
        _g9SuperNetCoreClient.GetType().GetMethod("Disconnect")?.Invoke(_g9SuperNetCoreClient, null);
        _g9SuperNetCoreClient = null;
        Thread.Sleep(3639);
    }

    #endregion

    #endregion Methods
}