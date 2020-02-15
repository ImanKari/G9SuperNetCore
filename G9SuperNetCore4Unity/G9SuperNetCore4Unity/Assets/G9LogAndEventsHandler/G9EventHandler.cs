using System;
using G9Common.Abstract;
using G9Common.Packet;
using G9SuperNetCoreClient.Enums;
using UnityEngine;
using UnityEngine.UI;

public class G9EventHandler : MonoBehaviour
{
    [Header("Text for show events log")] public InputField TextForEventsLog;

    public void OnConnect(object account)
    {
        ShowEventLogs($"OnConnect: Client Connected.\n{(account as AAccount)?.SessionSendCommand.GetSessionInfo()}");
    }

    public void OnDisconnected(object account, DisconnectReason reason)
    {
        ShowEventLogs(
            $"OnDisconnected: Client Disconnected.\nReason: {reason}\n{(account as AAccount)?.SessionSendCommand.GetSessionInfo()}");
    }

    public void OnError(Exception ex, ClientErrorReason reason)
    {
        ShowEventLogs($"OnError: Client Error.\nError reason: {reason}\n{ex.StackTrace}");
    }

    public void OnReconnect(object account, sbyte tryReconnectNumber)
    {
        ShowEventLogs(
            $"OnReconnect: Client Try for reconnect, Number of try {tryReconnectNumber}.\n{(account as AAccount)?.SessionSendCommand.GetSessionInfo()}");
    }

    public void OnUnhandledCommand(G9SendAndReceivePacket packet, object account)
    {
        ShowEventLogs(
            $"OnUnhandledCommand: Client receive unhandled command.\nCommand name: {packet.Command}\nPacket type: {packet.PacketType}\nPacket data type: {packet.PacketDataType}\n{(account as AAccount)?.SessionSendCommand.GetSessionInfo()}");
    }

    public void OnUnableToConnect()
    {
        ShowEventLogs(
            $"OnUnableToConnect: Client unable to connect!");
    }

    private void ShowEventLogs(string message)
    {
        if (TextForEventsLog == null) return;
        var newLog = $"###### [Events | {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] ######\n{message}\n\n{TextForEventsLog.text}";
        TextForEventsLog.text = newLog.Substring(0, Mathf.Clamp(newLog.Length, 0, 6000));
    }
}