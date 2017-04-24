// Copyright (c) 2017 Enflux Inc.
// By downloading, accessing or using this SDK, you signify that you have read, understood and agree to the terms and conditions of the End User License Agreement located at: https://www.getenflux.com/pages/sdk-eula

using System;
using System.Collections;
using System.IO;
using System.Linq;
using Enflux.SDK.Core;
using Enflux.SDK.Extensions;
using Enflux.SDK.FileDialogs;
using Enflux.SDK.Recording;
using Enflux.SDK.Recording.DataTypes;
using UnityEngine;
using UnityEngine.UI;
using Application = UnityEngine.Application;
using Button = UnityEngine.UI.Button;

namespace Enflux.Examples.UI
{
    public class EnfluxFilePlayerPanel : MonoBehaviour
    {
        [SerializeField] private EnfluxFilePlayer _filePlayer;

        [SerializeField] private InputField _filenameInputField;
        [SerializeField] private Slider _timelineSlider;
        [SerializeField] private Text _filenameText;
        [SerializeField] private Text _currentTimeText;
        [SerializeField] private Text _durationText;
        [SerializeField] private Slider _speedSlider;
        [SerializeField] private Text _speedText;
        [SerializeField] private Toggle _isLoopingToggle;
        [SerializeField] private Toggle _autoplayOnLoadToggle;
        [SerializeField] private Button _openFileButton;
        [SerializeField] private Button _loadButton;
        [SerializeField] private Button _unloadButton;
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _pauseButton;
        [SerializeField] private Button _stopButton;

        [SerializeField, HideInInspector] private string _previousDirectory;


        private void Reset()
        {
            _filePlayer = FindObjectOfType<EnfluxFilePlayer>();

            _filenameInputField = gameObject.FindChildComponent<InputField>("InputField_Filename");
            _timelineSlider = gameObject.FindChildComponent<Slider>("Slider_Timeline");
            _filenameText = gameObject.FindChildComponent<Text>("Text_Filename");
            _currentTimeText = gameObject.FindChildComponent<Text>("Text_CurrentTime");
            _durationText = gameObject.FindChildComponent<Text>("Text_Duration");
            _speedSlider = gameObject.FindChildComponent<Slider>("Slider_Speed");
            _speedText = gameObject.FindChildComponent<Text>("Text_Speed");
            _isLoopingToggle = gameObject.FindChildComponent<Toggle>("Toggle_IsLooping");
            _autoplayOnLoadToggle = gameObject.FindChildComponent<Toggle>("Toggle_AutoplayOnLoad");
            _openFileButton = gameObject.FindChildComponent<Button>("Button_OpenFile");
            _loadButton = gameObject.FindChildComponent<Button>("Button_Load");
            _unloadButton = gameObject.FindChildComponent<Button>("Button_Unload");
            _playButton = gameObject.FindChildComponent<Button>("Button_Play");
            _pauseButton = gameObject.FindChildComponent<Button>("Button_Pause");
            _stopButton = gameObject.FindChildComponent<Button>("Button_Stop");
        }

        private void OnEnable()
        {
            //TODO: Maybe sure fields aren't null!
            _filePlayer = _filePlayer ?? FindObjectOfType<EnfluxFilePlayer>();
            _filePlayer.StateChanged += FilePlayerOnStateChanged;
            _filePlayer.PlaybackReceivedError += FilePlayerOnPlaybackReceivedError;
            _filePlayer.IsLoopingChanged += FilePlayerOnIsLoopingChanged;
            _filePlayer.AutoplayOnLoadChanged += FilePlayerOnAutoplayOnLoadChanged;
            _filePlayer.SpeedChanged += FilePlayerOnSpeedChanged;

            _timelineSlider.onValueChanged.AddListener(TimelineSliderOnValueChanged);
            _speedSlider.onValueChanged.AddListener(SpeedSliderOnValueChanged);

            _isLoopingToggle.onValueChanged.AddListener(IsLoopingToggleOnValueChanged);
            _autoplayOnLoadToggle.onValueChanged.AddListener(AutoplayOnLoadToggleOnValueChanged);

            _openFileButton.onClick.AddListener(OpenFileButtonOnClick);
            _loadButton.onClick.AddListener(LoadButtonOnClick);
            _unloadButton.onClick.AddListener(UnloadButtonOnClick);
            _playButton.onClick.AddListener(StartButtonOnClick);
            _pauseButton.onClick.AddListener(PauseButtonOnClick);
            _stopButton.onClick.AddListener(StopButtonOnClick);
        }

        private void OnDisable()
        {
            //TODO: Maybe sure fields aren't null!
            _filePlayer.StateChanged -= FilePlayerOnStateChanged;
            _filePlayer.PlaybackReceivedError -= FilePlayerOnPlaybackReceivedError;
            _filePlayer.IsLoopingChanged -= FilePlayerOnIsLoopingChanged;
            _filePlayer.AutoplayOnLoadChanged -= FilePlayerOnAutoplayOnLoadChanged;
            _filePlayer.SpeedChanged -= FilePlayerOnSpeedChanged;

            _timelineSlider.onValueChanged.RemoveListener(TimelineSliderOnValueChanged);
            _speedSlider.onValueChanged.RemoveListener(SpeedSliderOnValueChanged);

            _openFileButton.onClick.RemoveListener(OpenFileButtonOnClick);
            _loadButton.onClick.RemoveListener(LoadButtonOnClick);
            _unloadButton.onClick.RemoveListener(UnloadButtonOnClick);
            _playButton.onClick.RemoveListener(StartButtonOnClick);
            _pauseButton.onClick.RemoveListener(PauseButtonOnClick);
            _stopButton.onClick.RemoveListener(StopButtonOnClick);
        }

        private IEnumerator Start()
        {
            yield return null;
            _filenameInputField.text = _filenameInputField.text != "" ? _filenameInputField.text : Application.streamingAssetsPath;
            _previousDirectory = !string.IsNullOrEmpty(_previousDirectory) ? _previousDirectory : Application.streamingAssetsPath;

            SpeedSliderOnValueChanged(_filePlayer.Speed);
            FilePlayerOnAutoplayOnLoadChanged(_filePlayer.AutoplayOnLoad);
            FilePlayerOnIsLoopingChanged(_filePlayer.IsLooping);
        }

        private void Update()
        {
            UpdateUi();
        }

        private void FilePlayerOnStateChanged(StateChange<PlaybackState> stateChange)
        {
            if (!_filePlayer.IsError)
            {
                _filenameText.color = Color.white;
            }
            if (_filePlayer.IsLoaded)
            {
                RefreshCurrentTimeText();

                if (stateChange.Previous == PlaybackState.Loading)
                {
                    _filenameText.text = "<b>Filename</b> " + _filePlayer.Filename;
                    var durationTimeSpan = TimeSpan.FromMilliseconds(_filePlayer.DurationMs);
                    _durationText.text = string.Format("{0:00}:{1:00}.{2:000}", durationTimeSpan.Minutes, durationTimeSpan.Seconds, durationTimeSpan.Milliseconds);
                }
            }
            else
            {
                _filenameText.text = "No file loaded.";
                _durationText.text = "00:00.000";
                _currentTimeText.text = "00:00.000";
            }
            UpdateUi();
        }

        private void FilePlayerOnPlaybackReceivedError(Notification<PlaybackResult> errorNotification)
        {
            _filenameText.color = Color.red;
            _filenameText.text = string.Format("{0} error: {1}", errorNotification.Value, errorNotification.Message);
        }

        private void FilePlayerOnAutoplayOnLoadChanged(bool autoplayOnLoad)
        {
            _autoplayOnLoadToggle.onValueChanged.RemoveListener(FilePlayerOnAutoplayOnLoadChanged);
            _autoplayOnLoadToggle.isOn = autoplayOnLoad;
            _autoplayOnLoadToggle.onValueChanged.AddListener(FilePlayerOnAutoplayOnLoadChanged);
        }

        private void FilePlayerOnIsLoopingChanged(bool isLooping)
        {
            _isLoopingToggle.onValueChanged.RemoveListener(FilePlayerOnIsLoopingChanged);
            _isLoopingToggle.isOn = isLooping;
            _isLoopingToggle.onValueChanged.AddListener(FilePlayerOnIsLoopingChanged);
        }

        private void FilePlayerOnSpeedChanged(float speed)
        {
            _speedSlider.onValueChanged.RemoveListener(SpeedSliderOnValueChanged);
            _speedSlider.value = speed;
            _speedSlider.onValueChanged.AddListener(SpeedSliderOnValueChanged);
        }

        private void TimelineSliderOnValueChanged(float value)
        {
            if (!_filePlayer.IsLoaded)
            {
                return;
            }
            if (!_filePlayer.IsPaused)
            {
                _filePlayer.Pause();
            }
            _filePlayer.CurrentTimeNormalized = _timelineSlider.normalizedValue;
            RefreshCurrentTimeText();
        }

        private void SpeedSliderOnValueChanged(float value)
        {
            _filePlayer.Speed = value;
            UpdateUi();
        }

        private void IsLoopingToggleOnValueChanged(bool isOn)
        {
            _filePlayer.IsLooping = isOn;
            UpdateUi();
        }

        private void AutoplayOnLoadToggleOnValueChanged(bool isOn)
        {
            _filePlayer.AutoplayOnLoad = isOn;
            UpdateUi();
        }

        private void OpenFileButtonOnClick()
        {
            var extensionFilters = new[] {new ExtensionFilter("Enflux Animation", "enfl"), new ExtensionFilter("All Files", "*")};
            var filename = FileDialog.OpenFilePanel("Open .enfl File", _previousDirectory, extensionFilters).FirstOrDefault();

            if (!string.IsNullOrEmpty(filename))
            {
                _previousDirectory = Path.GetDirectoryName(filename);
                _filenameInputField.text = filename;
            }
        }

        private void LoadButtonOnClick()
        {
            _filePlayer.Load(_filenameInputField.text);
        }

        private void UnloadButtonOnClick()
        {
            _filePlayer.Unload();
        }

        private void StartButtonOnClick()
        {
            _filePlayer.Play();
        }

        private void PauseButtonOnClick()
        {
            _filePlayer.Pause();
        }

        private void StopButtonOnClick()
        {
            _filePlayer.Stop();
        }

        private void UpdateUi()
        {
            if (_filePlayer != null)
            {
                _playButton.gameObject.SetActive(!_filePlayer.IsPlaying);
                _pauseButton.gameObject.SetActive(_filePlayer.IsPlaying);
                _stopButton.gameObject.SetActive(true);
                _speedText.text = string.Format("{0:0.0}x", _filePlayer.Speed);

                if (!_filePlayer.IsPaused || !Mathf.Approximately(_timelineSlider.normalizedValue, _filePlayer.CurrentTimeNormalized))
                {
                    _timelineSlider.onValueChanged.RemoveListener(TimelineSliderOnValueChanged);
                    _timelineSlider.normalizedValue = _filePlayer.CurrentTimeNormalized;
                    _timelineSlider.onValueChanged.AddListener(TimelineSliderOnValueChanged);
                }
                if (_filePlayer.IsPlaying)
                {
                    RefreshCurrentTimeText();
                }
            }
            else
            {
                _speedText.text = "---";
            }
            _unloadButton.gameObject.SetActive(_filePlayer != null && _filePlayer.IsLoaded);

            _openFileButton.interactable = _filePlayer != null;
            _loadButton.interactable = _filePlayer != null && _filenameInputField != null && !string.IsNullOrEmpty(_filenameInputField.text);
            _unloadButton.interactable = _filePlayer != null;
            _playButton.interactable = _filePlayer != null && _filePlayer.IsLoaded;
            _pauseButton.interactable = _filePlayer != null && _filePlayer.IsLoaded;
            _stopButton.interactable = _filePlayer != null && _filePlayer.IsLoaded;
        }

        private void RefreshCurrentTimeText()
        {
            var currentTimeSpan = TimeSpan.FromMilliseconds(_filePlayer.CurrentTimeMs);
            _currentTimeText.text = string.Format("{0:00}:{1:00}.{2:000}", currentTimeSpan.Minutes, currentTimeSpan.Seconds, currentTimeSpan.Milliseconds);
        }
    }
}