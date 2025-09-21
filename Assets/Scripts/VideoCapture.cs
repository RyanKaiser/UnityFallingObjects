#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.IO;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Encoder;
using UnityEditor.Recorder.Input;

public class VideoCapture : MonoBehaviour
{
    #region Singleton Implementation
    private static VideoCapture _instance;

    public static VideoCapture Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<VideoCapture>();
                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject("VideoCapture");
                    _instance = singletonObject.AddComponent<VideoCapture>();
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
        Initialize();
    }
    #endregion

    #region Recorder Settings
    private readonly string _filePath = "./Build/Screenshots/";
    private MovieRecorderSettings _settings = null;
    private RecorderController _recorderController = null;
    private bool _isRecording = false;
    [SerializeField] private bool _recordAudio = true;

    public FileInfo OutputFile
    {
        get
        {
            var fileName = _settings.OutputFile + ".mp4";
            return new FileInfo(fileName);
        }
    }

    public bool RecordAudio
    {
        set
        {
            _recordAudio = value;
            _settings.AudioInputSettings.PreserveAudio = value;
        }
        get
        {
            return _recordAudio;
        }
    }

    private void Initialize()
    {
        Debug.Log("VideoCapture: Initializing Recorder...");
        try
        {
            if (!Directory.Exists(_filePath))
            {
                Directory.CreateDirectory(_filePath);
            }
        }
        catch (IOException ex)
        {
            Console.WriteLine($"VideoCapture: Error creating directory: {ex.Message}");
        }

        RecorderOptions.VerboseMode = true;

        var controllerSettings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
        _recorderController = new RecorderController(controllerSettings);

        // Video Recorder 설정
        _settings = ScriptableObject.CreateInstance<MovieRecorderSettings>();
        _settings.name = "My Video Recorder";
        _settings.Enabled = true;

        // This example performs an MP4 recording
        _settings.EncoderSettings = new CoreEncoderSettings
        {
            EncodingQuality = CoreEncoderSettings.VideoEncodingQuality.High,
            Codec = CoreEncoderSettings.OutputCodec.MP4
        };
        _settings.CaptureAlpha = true;

        _settings.ImageInputSettings = new GameViewInputSettings
        {
            OutputWidth = Screen.width >> 1 << 1,
            OutputHeight = Screen.height >> 1 << 1
        };

        // _settings.AudioInputSettings.PreserveAudio = true;

        controllerSettings.AddRecorderSettings(_settings);
        // controllerSettings.SetRecordModeToFrameInterval(0, 60 * 60 * 10); // 10 minutes
        controllerSettings.SetRecordModeToManual();
        controllerSettings.FrameRate = 60;
        Debug.Log("VideoCapture: Recorder Initialized.");
    }
    #endregion

    #region Recording Control
    private void Update()
    {
        if (Keyboard.current.backquoteKey.wasPressedThisFrame && Keyboard.current.leftCtrlKey.isPressed)
        {
            ToggleRecording();
        }
        else if (Keyboard.current.backquoteKey.wasPressedThisFrame)
        {
            CaptureScreenshot();
        }
    }

    public void ToggleRecording()
    {
        _isRecording = !_isRecording;
        if (_isRecording)
        {
            StartRecording();
        }
        else
        {
            StopRecording();
        }
    }

    public void StartRecording()
    {
        string filename = _filePath + "Video" + DateTime.Now.ToString("MMddyyyyHHmmss") + DateTime.Now.Millisecond;
        _settings.OutputFile = filename;

        RecorderOptions.VerboseMode = false;
        _recorderController.PrepareRecording();
        _recorderController.StartRecording();

        Debug.Log($"Started recording for file {filename}");
    }

    public void StopRecording()
    {
        _recorderController.StopRecording();

        Debug.Log("Stopped recording.");
    }

    public void CaptureScreenshot()
    {
        string filename = _filePath + "Screenshot" + DateTime.Now.Second + DateTime.Now.Millisecond + ".png";
        Debug.Log("VideoCapture: Screenshot " + filename);
        ScreenCapture.CaptureScreenshot(filename);
    }
    #endregion
}

#endif