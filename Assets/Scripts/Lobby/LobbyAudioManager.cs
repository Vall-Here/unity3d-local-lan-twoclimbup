using UnityEngine;

public class LobbyAudioManager : MonoBehaviour
{
    private static LobbyAudioManager _instance;
    public static LobbyAudioManager Instance => _instance;

    [Header("Audio Source")]
    [SerializeField] private AudioSource _backsoundSource;
    [SerializeField] private AudioSource _sfxSource;

    [Space(10)]
    [Header("Audio Clips")]
    [SerializeField] private AudioClip _lobbyBacksound;
    [SerializeField] private AudioClip _buttonClickSfx;
    [SerializeField] private AudioClip _openUISfx;
    [SerializeField] private AudioClip _closeUISfx;
    [SerializeField] private AudioClip _successUISfx;
    [SerializeField] private AudioClip _failUISfx;

    [Header("Audio Settings")]
    [SerializeField] private AudioSettingsSO audioSettings;

    private void Awake()
    {
        _instance = this;
    }

    void Start()
    {
        LoadAudioSettings();

        _backsoundSource.clip = _lobbyBacksound;
        _backsoundSource.loop = true;
        _backsoundSource.volume = audioSettings.volumeBacksound;
        _backsoundSource.Play();
    }

    public void PlayButtonClickSfx()
    {
        _sfxSource.volume = audioSettings.volumeSFX;
        _sfxSource.PlayOneShot(_buttonClickSfx);
    }

    public void PlayOpenUISfx()
    {
        _sfxSource.volume = audioSettings.volumeSFX;
        _sfxSource.PlayOneShot(_openUISfx);
    }

    public void PlayCloseUISfx()
    {
        _sfxSource.volume = audioSettings.volumeSFX;
        _sfxSource.PlayOneShot(_closeUISfx);
    }

    public void PlaySuccessUISfx()
    {
        _sfxSource.volume = audioSettings.volumeSFX;
        _sfxSource.PlayOneShot(_successUISfx);
    }

    public void PlayFailUISfx()
    {
        _sfxSource.volume = audioSettings.volumeSFX;
        _sfxSource.PlayOneShot(_failUISfx);
    }

    public void StopAllAudio()
    {
        _backsoundSource.Stop();
    }

    public void SaveAudioSettings()
    {
        PlayerPrefs.SetFloat("VolumeBacksound", audioSettings.volumeBacksound);
        PlayerPrefs.SetFloat("VolumeSFX", audioSettings.volumeSFX);
        PlayerPrefs.Save();
    }

    private void LoadAudioSettings()
    {
        if (PlayerPrefs.HasKey("VolumeBacksound"))
        {
            audioSettings.volumeBacksound = PlayerPrefs.GetFloat("VolumeBacksound");
        }
        if (PlayerPrefs.HasKey("VolumeSFX"))
        {
            audioSettings.volumeSFX = PlayerPrefs.GetFloat("VolumeSFX");
        }
    }

    public void UpdateBacksoundVolume(float volume)
    {
        _backsoundSource.volume = volume;
    }

    public void UpdateSFXVolume(float volume)
    {
        _sfxSource.volume = volume;
    }
}
