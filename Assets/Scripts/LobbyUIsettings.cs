using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUIsettings : MonoBehaviour
{
    [SerializeField] private Slider volumeBacksoundSlider;
    [SerializeField] private Slider volumeSFXSlider;

    [SerializeField] private AudioSettingsSO audioSettings;

    private void Start()
    {
        volumeBacksoundSlider.value = audioSettings.volumeBacksound;
        volumeSFXSlider.value = audioSettings.volumeSFX;

        volumeBacksoundSlider.onValueChanged.AddListener(ChangeVolumeBacksound);
        volumeSFXSlider.onValueChanged.AddListener(ChangeVolumeSFX);
    }

    private void ChangeVolumeBacksound(float value)
    {
        audioSettings.volumeBacksound = value;
        LobbyAudioManager.Instance.UpdateBacksoundVolume(value);
        LobbyAudioManager.Instance.SaveAudioSettings();
    }

    private void ChangeVolumeSFX(float value)
    {
        audioSettings.volumeSFX = value;
        LobbyAudioManager.Instance.UpdateSFXVolume(value);
        LobbyAudioManager.Instance.SaveAudioSettings();
    }
}
