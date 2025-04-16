using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainSceneAudioManager : MonoBehaviour
{
    private static MainSceneAudioManager _instance;
    public static MainSceneAudioManager Instance => _instance;


    [SerializeField] private AudioSettingsSO _audioSettings;

    [Header("Audio Source")]
    [SerializeField] private AudioSource _backsoundSource;
    [SerializeField] private AudioSource _sfxSource;

    [Space(10)]
    [Header("Audio Clips")]
    [SerializeField] private AudioClip _buttonClickSfx;
    [SerializeField] private AudioClip _openUISfx;
    [SerializeField] private AudioClip _closeUISfx;

    public delegate void OnAudioSettingsChanged(float volumeBacksound, float volumeSFX);
    public event OnAudioSettingsChanged AudioSettingsChanged;


    private void Awake()
    {
        _instance = this;   }
    
     public void UpdateBacksoundVolume(float volume)
    {
        _backsoundSource.volume = volume;
        AudioSettingsChanged?.Invoke(volume, _audioSettings.volumeSFX);
    }

    public void UpdateSFXVolume(float volume)
    {
        _sfxSource.volume = volume;
        AudioSettingsChanged?.Invoke(_audioSettings.volumeBacksound, volume);
    }

      public void SaveAudioSettings()
    {
        PlayerPrefs.SetFloat("VolumeBacksound", _audioSettings.volumeBacksound);
        PlayerPrefs.SetFloat("VolumeSFX", _audioSettings.volumeSFX);
        PlayerPrefs.Save();
    }

     public void PlayButtonClickSfx()
    {
        _sfxSource.volume = _audioSettings.volumeSFX;
        _sfxSource.PlayOneShot(_buttonClickSfx);
    }

    public void PlayOpenUISfx()
    {
        _sfxSource.volume = _audioSettings.volumeSFX;
        _sfxSource.PlayOneShot(_openUISfx);
    }

    public void PlayCloseUISfx()
    {
        _sfxSource.volume = _audioSettings.volumeSFX;
        _sfxSource.PlayOneShot(_closeUISfx);
    }

}
