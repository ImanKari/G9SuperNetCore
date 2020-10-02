#if UNITY_EDITOR
using System.Net;
using UnityEditor;
using UnityEngine;

/// <inheritdoc />
[CustomEditor(typeof(G9SuperNetCoreClient4Unity))]
// ReSharper disable once CheckNamespace
public class G9SuperNetCoreClient4UnityEditor : Editor
{
    private G9SuperNetCoreClient4Unity _clientTarget;

    private void OnEnable()
    {
        _clientTarget = target as G9SuperNetCoreClient4Unity;
    }

    public override void OnInspectorGUI()
    {
        _clientTarget.Validation = true;

        EditorGUILayout.HelpBox(
            $"Version G9SuperNetCoreClient4Unity: {G9SuperNetCoreClient4Unity.Version}",
            MessageType.Info, true);

        #region IpAddress

        _clientTarget.ServerIpAddress = EditorGUILayout.TextField("Server Ip Address", _clientTarget.ServerIpAddress);
        if (string.IsNullOrEmpty(_clientTarget.ServerIpAddress))
        {
            EditorGUILayout.HelpBox("Please enter the ip address", MessageType.Error);
            _clientTarget.Validation = false;
        }
        else if (!IPAddress.TryParse(_clientTarget.ServerIpAddress, out _))
        {
            EditorGUILayout.HelpBox("Invalid ip address!", MessageType.Error);
            _clientTarget.Validation = false;
        }
        else
        {
            EditorGUILayout.HelpBox("The IP address format is correct.", MessageType.None);
        }

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

            var myTextAreaStyle = new GUIStyle(EditorStyles.textArea);
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
                EditorGUILayout.HelpBox("Private is empty or null exception.", MessageType.Error);
                _clientTarget.Validation = false;
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Disable ssl connection for client.", MessageType.Warning);
        }

        #endregion
    }
}
#endif