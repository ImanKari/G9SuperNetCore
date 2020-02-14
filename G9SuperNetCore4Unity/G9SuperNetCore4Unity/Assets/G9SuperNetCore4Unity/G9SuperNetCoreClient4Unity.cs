using System;
using System.Net;
using System.Reflection;
using System.Threading;
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
    public const string Version = "2.1.0.1";

    #region Fields And Properties

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
    public IPAddress ServerIpAddress = IPAddress.Parse("192.168.1.103");

    private static IPAddress _serverIpAddress;

    /// <summary>
    ///     Specified port
    /// </summary>
    [Range(10, ushort.MaxValue)] public ushort Port = 9639;

    private static ushort _port = 9639;

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
    ///     Start is called before the first frame update
    /// </summary>

    #region Start

    private void Start()
    {
    }

    #endregion

    /// <summary>
    ///     Initialize client with custom account and session
    /// </summary>
    /// <typeparam name="TAccount">Specified type of account</typeparam>
    /// <typeparam name="TSession">Specified type of session</typeparam>
    /// <returns>Instance of client</returns>

    #region Initialize

    public static G9SuperNetCoreSocketClient<TAccount, TSession> Initialize<TAccount, TSession>()
        where TAccount : AClientAccount<TSession>, new()
        where TSession : AClientSession, new()
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
                new G9ClientConfig(_serverIpAddress, _port, SocketMode.Tcp), Assembly.GetExecutingAssembly(),
                _privateKey,
                _uniqueIdentity)
            : new G9SuperNetCoreSocketClient<TAccount, TSession>(
                new G9ClientConfig(_serverIpAddress, _port, SocketMode.Tcp), Assembly.GetExecutingAssembly());

        // Set disconnect action
        DisconnectServerMethod = () =>
            (_g9SuperNetCoreClient as G9SuperNetCoreSocketClient<TAccount, TSession>)?.Disconnect();

        // Set event handler
        InitializeEvents((G9SuperNetCoreSocketClient<TAccount, TSession>) _g9SuperNetCoreClient);

        return (G9SuperNetCoreSocketClient<TAccount, TSession>) _g9SuperNetCoreClient;
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
        ((G9SuperNetCoreSocketClient<TAccount, TSession>) _g9SuperNetCoreClient).OnConnected += account =>
            EventsQueue.Enqueue(() => AccessToG9Events4Unity.OnConnectedHandler?.Invoke(account));


        // On disconnect
        ((G9SuperNetCoreSocketClient<TAccount, TSession>) _g9SuperNetCoreClient).OnDisconnected += (account, reason) =>
            EventsQueue.Enqueue(() => AccessToG9Events4Unity.OnDisconnectedHandler?.Invoke(account, reason));

        // On error
        ((G9SuperNetCoreSocketClient<TAccount, TSession>) _g9SuperNetCoreClient).OnError += (error, reason) =>
            EventsQueue.Enqueue(() => AccessToG9Events4Unity.OnErrorHandler?.Invoke(error, reason));

        // On reconnect
        ((G9SuperNetCoreSocketClient<TAccount, TSession>) _g9SuperNetCoreClient).OnReconnect += account =>
            EventsQueue.Enqueue(() => AccessToG9Events4Unity.OnReconnectHandler?.Invoke(account));

        // On unhandled command
        ((G9SuperNetCoreSocketClient<TAccount, TSession>) _g9SuperNetCoreClient).OnUnhandledCommand +=
            (packet, account) =>
                EventsQueue.Enqueue(() => AccessToG9Events4Unity.OnUnhandledCommand?.Invoke(packet, account));
    }

    #endregion

    /// <summary>
    ///     Handle send, receive and events in update
    /// </summary>

    #region Update

    // ReSharper disable once UnusedMember.Local
    private void Update()
    {
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
        if (_g9SuperNetCoreClient == null) return;
        _g9SuperNetCoreClient.GetType().GetMethod("Disconnect")?.Invoke(_g9SuperNetCoreClient, null);
        _g9SuperNetCoreClient = null;
        Thread.Sleep(3639);
    }

    #endregion

    #endregion Methods
}