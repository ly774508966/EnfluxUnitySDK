﻿// Copyright (c) 2017 Enflux Inc.
// By downloading, accessing or using this SDK, you signify that you have read, understood and agree to the terms and conditions of the End User License Agreement located at: https://www.getenflux.com/pages/sdk-eula
using System;
using System.Collections;
using Enflux.SDK.Core;
using Enflux.SDK.Extensions;
using Enflux.SDK.Recording;
using UnityEngine;
using UnityEngine.UI;

namespace Enflux.SDK.UI
{
    public class EnfluxExampleRecordingPanel : MonoBehaviour
    {
        [SerializeField] private EnfluxFileRecorder _fileRecorder;
        [SerializeField] private EnfluxFilePlayer _filePlayer;

        [SerializeField] private InputField _filenameInputField;
        [SerializeField] private Button _startRecordingButton;
        [SerializeField] private Button _stopRecordingButton;
        [SerializeField] private Button _startPlaybackButton;
        [SerializeField] private Button _stopPlaybackButton;
        [SerializeField] private Text _errorText;

        private void Reset()
        {
            _fileRecorder = FindObjectOfType<EnfluxFileRecorder>();
            _fileRecorder.RecordingError += OnRecordingError;
            _filePlayer = FindObjectOfType<EnfluxFilePlayer>();
            _filePlayer.PlaybackError += OnPlaybackError;

            _filenameInputField = gameObject.FindChildComponent<InputField>("InputField_Filename");
            _startRecordingButton = gameObject.FindChildComponent<Button>("Button_StartRecording");
            _stopRecordingButton = gameObject.FindChildComponent<Button>("Button_StopRecording");
            _startPlaybackButton = gameObject.FindChildComponent<Button>("Button_StartPlayback");
            _stopPlaybackButton = gameObject.FindChildComponent<Button>("Button_StopPlayback");
            _errorText.text = "";
        }

        private void OnRecordingError(RecordingResult error)
        {
            _errorText.text = "Error: " + error.ToString();
        }

        private void OnPlaybackError(PlaybackResult error)
        {
            _errorText.text = "Error: " + error.ToString();
        }

        private void OnEnable()
        {
            _fileRecorder = _fileRecorder ?? FindObjectOfType<EnfluxFileRecorder>();
            _fileRecorder.RecordingError += OnRecordingError;
            _filePlayer = _filePlayer ?? FindObjectOfType<EnfluxFilePlayer>();
            _filePlayer.PlaybackError += OnPlaybackError;
            _startRecordingButton.onClick.AddListener(StartRecordingButtonOnClick);
            _stopRecordingButton.onClick.AddListener(StopRecordingButtonOnClick);
            _startPlaybackButton.onClick.AddListener(StartPlaybackButtonOnClick);
            _stopPlaybackButton.onClick.AddListener(StopPlaybackButtonOnClick);
            _filenameInputField.onEndEdit.AddListener(FilenameInputFieldOnEndEdit);
        }

        private void OnDisable()
        {
            _fileRecorder.RecordingError -= OnRecordingError;
            _filePlayer.PlaybackError -= OnPlaybackError;
            _startRecordingButton.onClick.RemoveListener(StartRecordingButtonOnClick);
            _stopRecordingButton.onClick.RemoveListener(StopRecordingButtonOnClick);
            _startPlaybackButton.onClick.RemoveListener(StartPlaybackButtonOnClick);
            _stopPlaybackButton.onClick.RemoveListener(StopPlaybackButtonOnClick);
            _filenameInputField.onEndEdit.AddListener(FilenameInputFieldOnEndEdit);
        }

        private IEnumerator Start()
        {
            yield return null;
            _filenameInputField.text = _fileRecorder.Filename;
            _filePlayer.Filename = _fileRecorder.Filename;
            _errorText.text = "";
        }

        private void Update()
        {
            if (_fileRecorder != null)
            {
                _startRecordingButton.gameObject.SetActive(!_fileRecorder.IsRecording);
                _stopRecordingButton.gameObject.SetActive(_fileRecorder.IsRecording);
            }
            if (_filePlayer != null)
            {
                _startPlaybackButton.gameObject.SetActive(!_filePlayer.IsPlaying);
                _stopPlaybackButton.gameObject.SetActive(_filePlayer.IsPlaying);
            }
            _startRecordingButton.interactable = _fileRecorder != null;
            _stopRecordingButton.interactable = _fileRecorder != null;
            _startPlaybackButton.interactable = _filePlayer != null;
            _stopRecordingButton.interactable = _filePlayer != null;
        }

        private void StartRecordingButtonOnClick()
        {
            _errorText.text = "";
            _fileRecorder.IsRecording = true;
        }

        private void StopRecordingButtonOnClick()
        {
            _errorText.text = "";
            _fileRecorder.IsRecording = false;
        }

        private void StartPlaybackButtonOnClick()
        {
            _errorText.text = "";
            _filePlayer.IsPlaying = true;
        }

        private void StopPlaybackButtonOnClick()
        {
            _errorText.text = "";
            _filePlayer.IsPlaying = false;
        }

        private void FilenameInputFieldOnEndEdit(string filename)
        {
            if (_fileRecorder != null)
            {
                _fileRecorder.Filename = filename;
            }
            if (_filePlayer != null)
            {
                _filePlayer.Filename = filename;
            }
        }
    }
}