﻿using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(G9SuperNetCoreClient4Unity))]
public class G9SuperNetCoreClient4UnityEditor : Editor
{
    private G9SuperNetCoreClient4Unity _clientTarget;

    private void OnEnable()
    {
        _clientTarget = (target as G9SuperNetCoreClient4Unity);
    }

    public override void OnInspectorGUI()
    {
        _clientTarget.Validation = true;

        #region Enable auto connected

        _clientTarget.EnableAutoStartConnection =
            EditorGUILayout.Toggle("Auto Connected", _clientTarget.EnableAutoStartConnection);
        if (_clientTarget.EnableAutoStartConnection)
            EditorGUILayout.HelpBox(
                "Enable auto connected for client.\nConnected automatic to server in first time after start game.",
                MessageType.Info);
        else
            EditorGUILayout.HelpBox("Disable auto connected for client.", MessageType.Warning);
        #endregion

        #region IpAddress
        string ipAddress = _clientTarget.ServerIpAddress?.ToString();
        ipAddress = EditorGUILayout.TextField("Server Ip Address", ipAddress);
        if (string.IsNullOrEmpty(ipAddress))
        {
            EditorGUILayout.HelpBox("Please enter the ip address", MessageType.Error);
            _clientTarget.Validation = false;
        }
        else if (!IPAddress.TryParse(ipAddress, out _clientTarget.ServerIpAddress))
        {
            EditorGUILayout.HelpBox("Invalid ip address!", MessageType.Error);
            _clientTarget.Validation = false;
        }
        else
            EditorGUILayout.HelpBox("The IP address format is correct.", MessageType.None);
        #endregion

        #region Port

        int port = _clientTarget.Port;
        port = EditorGUILayout.IntField("Server Port", port);
        if (port < 10 || port > ushort.MaxValue)
        {
            EditorGUILayout.HelpBox($"Invalid port number! port number between 10 and {ushort.MaxValue}",
                MessageType.Error);
            _clientTarget.Validation = false;
        }
        else
        {
            EditorGUILayout.HelpBox("The port number is correct.", MessageType.None);
            _clientTarget.Port = (ushort) port;
        }

        #endregion

        #region Enable ssl connection

        _clientTarget.EnableSecureConnection =
            EditorGUILayout.Toggle("Enable ssl connection", _clientTarget.EnableSecureConnection);
        if (_clientTarget.EnableSecureConnection)
        {
            EditorGUILayout.HelpBox("Enable ssl connection for client.\nConnected with ssl to server.",
                MessageType.Info);

            GUIStyle myTextAreaStyle = new GUIStyle(EditorStyles.textArea);
            myTextAreaStyle.wordWrap = true;
            myTextAreaStyle.fixedHeight = 69;
            _clientTarget.PrivateKey = EditorGUILayout.TextArea(_clientTarget.PrivateKey, myTextAreaStyle);
            if (_clientTarget.PrivateKey.Length > 0)
            {
                EditorGUILayout.HelpBox($"Private key length: {_clientTarget.PrivateKey.Length}", MessageType.Info);

                _clientTarget.UniqueIdentity = SystemInfo.deviceUniqueIdentifier;
                EditorGUILayout.HelpBox($"Device Unique Identifier\n{_clientTarget.UniqueIdentity}", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox($"Private is empty or null exception.", MessageType.Error);
                _clientTarget.Validation = false;
            }

        }
        else
            EditorGUILayout.HelpBox("Disable ssl connection for client.", MessageType.Warning);
        #endregion
    }
}