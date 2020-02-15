using System;
using G9Common.Packet;
using G9SuperNetCoreClient.Enums;
using UnityEngine;
using UnityEngine.Events;

[DefaultExecutionOrder(-999)]
[Serializable]
public class G9Events4Unity : MonoBehaviour
{
    /// <summary>
    ///     Management connected
    /// </summary>
    public ConnectedHandler OnConnectedHandler;

    /// <summary>
    ///     Management disconnected
    /// </summary>
    public DisconnectedHandler OnDisconnectedHandler;

    /// <summary>
    ///     Management error
    /// </summary>
    public ErrorHandler OnErrorHandler;

    /// <summary>
    ///     Management reconnect
    /// </summary>
    public ReconnectHandler OnReconnectHandler;

    /// <summary>
    ///     Management Unable to connect
    /// </summary>
    public UnityEvent OnUnableToConnect;

    /// <summary>
    ///     Management Unhandled commands
    /// </summary>
    public UnhandledCommand OnUnhandledCommand;
}

[Serializable]
public class ConnectedHandler : UnityEvent<object>
{
}

[Serializable]
public class DisconnectedHandler : UnityEvent<object, DisconnectReason>
{
}

[Serializable]
public class ErrorHandler : UnityEvent<Exception, ClientErrorReason>
{
}

[Serializable]
public class ReconnectHandler : UnityEvent<object, sbyte>
{
}

[Serializable]
public class UnhandledCommand : UnityEvent<G9SendAndReceivePacket, object>
{
}