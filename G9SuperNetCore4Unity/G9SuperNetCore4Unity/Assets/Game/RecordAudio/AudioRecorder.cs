using G9SuperNetCoreServerSampleApp_GameServer.Commands;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//Use the PointerDown and PointerUP interfaces to detect a mouse down and up on a ui element
public class AudioRecorder : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private bool _enableRecord;

    private float _time;

    //Keep this one as a global variable (outside the functions) too and use GetComponent during start to save resources
    private AudioSource audioSource;
    private AudioClip recording;
    private float startRecordingTime;

    private RawImage _accessToImage;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_enableRecord)
        {
            _enableRecord = false;
            _accessToImage.color = Color.red;
        }
        else
        {
            _accessToImage.color = Color.blue;
            _time = Time.time;
            _enableRecord = true;
            //Get the max frequency of a microphone, if it's less than 6999 record at the max frequency, else record at 6999
            int minFreq;
            int maxFreq;
            var freq = 6999;
            Microphone.GetDeviceCaps("", out minFreq, out maxFreq);
            if (maxFreq < 6999)
                freq = maxFreq;

            //Start the recording, the length of 300 gives it a cap of 5 minutes
            recording = Microphone.Start("", false, 300, 6999);
            startRecordingTime = Time.time;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
    }

    private void Update()
    {
        if (!_enableRecord) return;

        if (Time.time - _time <= 0.2) return;

        //End the recording when the mouse comes back up, then play it
        Microphone.End("");

        var data = new float[(int) ((Time.time - startRecordingTime) * recording.frequency)];
        recording.GetData(data, 0);
        
        G9SuperNetCoreClient4UnityInitializeGameClient.G9SuperNetCoreClient.SendCommandAsync<CVoice, float[]>(data);

        //Start the recording, the length of 300 gives it a cap of 5 minutes
        recording = Microphone.Start("", false, 300, 6999);

        _time = Time.time;
        startRecordingTime = Time.time;
    }

    //Get the audiosource here to save resources
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        _accessToImage = GetComponent<RawImage>();
        _accessToImage.color = Color.red;
    }
}