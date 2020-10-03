using System;
using System.Threading.Tasks;
using G9SuperNetCoreClient.Client.Socket;
using G9SuperNetCoreClient.Sample;
using G9SuperNetCoreServerSampleApp_GameServer.AccountAndSession;
using UnityEngine;

[Serializable]
public class G9SuperNetCoreClient4UnityInitializeGameClient : MonoBehaviour
{
    private float deltaTime;

    /// <summary>
    ///     Specified custom game account for client
    /// </summary>
    [Header("Custom game account for client")]
    public GameAccount GameAccount;

    /// <summary>
    ///     Access to client instance
    /// </summary>
    public static G9SuperNetCoreSocketClient<GameAccount, GameSession> G9SuperNetCoreClient
    {
        private set;
        get;
    }

    /// <summary>
    ///     Run automatic in first time just one time
    /// </summary>

    #region Awake

    // ReSharper disable once UnusedMember.Local
    private void Awake()
    {
        // Run in background
        Application.runInBackground = true;
        // Screen set NeverSleep
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        // Make the game run as fast as possible
        Application.targetFrameRate = 60;
    }

    #endregion

    public async Task StartConnection(string ip, int port)
    {
        // Initialize client
        G9SuperNetCoreClient =
            await G9SuperNetCoreClient4Unity.Initialize<GameAccount, GameSession>(GameAccount);
        await G9SuperNetCoreClient.StartConnection(ip, port);
    }

    private void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }

    private void OnGUI()
    {
        int w = Screen.width, h = Screen.height;

        var style = new GUIStyle();

        var rect = new Rect(w - 239, 69, w, h * 2 / 100);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = h * 2 / 100;
        style.normal.textColor = new Color(0.69f, 0.0f, 0.0f, 1.0f);
        var msec = deltaTime * 1000.0f;
        var fps = 1.0f / deltaTime;
        var text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
        GUI.Label(rect, text, style);
    }
}