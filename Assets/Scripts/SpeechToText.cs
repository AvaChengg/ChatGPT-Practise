using OpenAI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpeechToText : MonoBehaviour
{
    [Header("Microphone Setting: ")]
    [SerializeField] private Button _recordButton;
    [SerializeField] private Image _progressBar;
    [SerializeField] private Dropdown _dropdown;

    [Header("ChatGPT UI Settings: ")]
    [SerializeField] private InputField _inputField;
    [SerializeField] private InputField _dallInputField;
    [SerializeField] private Button _sentButton;

    private readonly string _fileName = "output.wav";
    private readonly int _duration = 5;
    private float _time;

    private AudioClip _audioClip;
    private bool _isRecording;
    private OpenAIApi _openAI = new OpenAIApi();

    private void Start()
    {
        // find microphone devices
        foreach (var device in Microphone.devices)
        {
            _dropdown.options.Add(new Dropdown.OptionData(device));
        }
        _recordButton.onClick.AddListener(StartRecording);
        _dropdown.onValueChanged.AddListener(ChangeMicrophone);

        var index = PlayerPrefs.GetInt("user-mic-device-index");
        _dropdown.SetValueWithoutNotify(index);
    }

    private void ChangeMicrophone(int index)
    {
        PlayerPrefs.SetInt("user-mic-device-index", index);
    }

    private void StartRecording()
    {
        _isRecording = true;
        _recordButton.enabled = false;
        _inputField.enabled = false;
        _dallInputField.enabled = false;
        _sentButton.enabled = false;

        var index = PlayerPrefs.GetInt("user-mic-device-index");
        _audioClip = Microphone.Start(_dropdown.options[index].text, false, _duration, 44100);
    }

    private async void EndRecording()
    {
        _inputField.text = "Transcripting...";
        _dallInputField.text = "Transcripting...";

        Microphone.End(null);
        byte[] data = SaveWav.Save(_fileName, _audioClip);

        var req = new CreateAudioTranslationRequest()
        {
            FileData = new FileData() { Data = data, Name = "audio.wav" },
            Model = "whisper-1",
        };
        var res = await _openAI.CreateAudioTranslation(req);

        _progressBar.fillAmount = 0;
        _inputField.text = res.Text;
        _dallInputField.text = res.Text;
        _recordButton.enabled = true;
        _inputField.enabled = true;
        _dallInputField.enabled = true;
        _sentButton.enabled = true;
    }

    private void Update()
    {
        if (_isRecording)
        {
            _time += Time.deltaTime;
            _progressBar.fillAmount = _time / _duration;

            if (_time >= _duration)
            {
                _time = 0;
                _isRecording = false;
                EndRecording();
            }
        }
    }
}
