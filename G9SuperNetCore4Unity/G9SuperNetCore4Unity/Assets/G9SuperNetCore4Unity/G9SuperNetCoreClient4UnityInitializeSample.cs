using System.Threading.Tasks;
using G9SuperNetCoreClient.Client.Socket;
using G9SuperNetCoreClient.Sample;
using UnityEngine;

[System.Serializable]
public class G9SuperNetCoreClient4UnityInitializeSample : MonoBehaviour
{
    /// <summary>
    ///     Access to client instance
    /// </summary>
    public static G9SuperNetCoreSocketClient<ClientAccountSample, ClientSessionSample> G9SuperNetCoreClient
    {
        private set;
        get;
    }


    /// <summary>
    ///     Run automatic in first time just one time
    /// </summary>

    #region Awake

    // ReSharper disable once UnusedMember.Local
    private async Task Awake()
    {
        // Run in background
        Application.runInBackground = true;
        // Screen set NeverSleep
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        // Make the game run as fast as possible
        Application.targetFrameRate = 60;
        // Initialize client
        G9SuperNetCoreClient = await G9SuperNetCoreClient4Unity.Initialize<ClientAccountSample, ClientSessionSample>();
        await G9SuperNetCoreClient.StartConnection();
    }

    #endregion

    float deltaTime = 0.0f;

    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }

    void OnGUI()
    {
        int w = Screen.width, h = Screen.height;

        GUIStyle style = new GUIStyle();

        Rect rect = new Rect(w - 239, 69, w, h * 2 / 100);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = h * 2 / 100;
        style.normal.textColor = new Color(0.69f, 0.0f, 0.0f, 1.0f);
        float msec = deltaTime * 1000.0f;
        float fps = 1.0f / deltaTime;
        string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
        GUI.Label(rect, text, style);
    }
}